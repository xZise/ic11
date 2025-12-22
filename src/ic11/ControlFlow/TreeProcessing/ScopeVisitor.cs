using ic11.ControlFlow.Context;
using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.Messages;
using ic11.ControlFlow.NodeInterfaces;
using ic11.ControlFlow.Nodes;
using Scope = ic11.ControlFlow.Context.Scope;

namespace ic11.ControlFlow.TreeProcessing;
public class ScopeVisitor
{
    private Scope _currentScope = null!;
    private readonly FlowContext _flowContext;

    public ScopeVisitor(FlowContext flowContext)
    {
        _flowContext = flowContext;
    }

    public object? Visit(Root node)
    {
        _currentScope = node.Scope;
        VisitStatementList(node.Statements);

        return default!;
    }

    private void VisitStatementList(IEnumerable<IStatement> list)
    {
        foreach (var item in list)
            VisitStatement(item);
    }

    private void VisitStatement(IStatement statement)
    {
        if (statement is If ifStatement)
        {
            Visit(ifStatement);
            return;
        }

        if (statement is For forStatement)
        {
            Visit(forStatement);
            return;
        }

        if (statement is ArrayDeclaration arrayDecStatement)
        {
            Visit(arrayDecStatement);
            return;
        }

        if (statement is IExpressionContainer ec)
            foreach (var item in ec.Expressions)
                VisitExpression(item);

        AssignScope(statement);

        if (statement is IStatementsContainer st)
        {
            var md = statement as MethodDeclaration;

            using var _ = new ChildScope(this, md);

            if (md is not null)
            {
                md.InnerScope = _currentScope;
                AddParameterVariables(md);
            }

            VisitStatementList(st.Statements);
        }
    }

    private void AddParameterVariables(MethodDeclaration node)
    {
        foreach (var parameter in node.Parameters)
        {
            try
            {
                var variable = _currentScope!.ClaimNewVariable(-1);
                variable.IsParameter = true;
                node.ParameterVariables.Add(variable);

                var newUserDefinedVariable = new UserDefinedVariable(parameter.Text, parameter.SourceLocation, variable!, -1, false);

                _currentScope.AddUserVariable(newUserDefinedVariable);
                _flowContext.AllUserDefinedVariables.Add(newUserDefinedVariable);
            }
            catch (CompilerMessageException ex)
            {
                _flowContext.CompilerMessages.Add(ex.Error(node.SourceLocation));
            }
        }
    }

    protected object? Visit(If node)
    {
        AssignScope(node);

        VisitExpression(node.Expression);

        node.CurrentStatementsContainer = IfStatementsContainer.If;

        if (node.Statements.Any())
        {
            using var _ = new ChildScope(this);
            VisitStatementList(node.Statements);
        }

        node.CurrentStatementsContainer = IfStatementsContainer.Else;

        if (node.Statements.Any())
        {
            using var _ = new ChildScope(this);
            VisitStatementList(node.Statements);
        }

        return default!;
    }

    protected object? Visit(For node)
    {
        AssignScope(node);

        using var _ = new ChildScope(this);

        IEnumerable<IStatement> innerStatements = node.Statements;

        if (node.HasStatement1)
        {
            innerStatements = innerStatements.Skip(1);
            VisitStatement(node.Statements.First());
        }

        VisitExpression(node.Expression);

        VisitStatementList(innerStatements);

        return null;
    }

    protected object? Visit(ArrayDeclaration node)
    {
        // Array declarations need their expressions only *after* the address is assigned, so the order is reversed
        AssignScope(node);

        foreach (var item in node.Expressions)
            VisitExpression(item);

        return null;
    }

    private void VisitExpression(INodeExpression expression)
    {
        if (expression is IExpressionContainer innerContainer)
        {
            foreach (var item in innerContainer.Expressions)
                VisitExpression(item);
        }

        AssignScope(expression);
    }

    private void AssignScope(INode node)
    {
        node.Scope = _currentScope;
        node.SetIndex(ref _currentScope.CurrentNodeOrder);
    }

    private ref struct ChildScope: IDisposable
    {
        private ScopeVisitor _visitor;

        public ChildScope(ScopeVisitor visitor, MethodDeclaration? method = null)
        {
            _visitor = visitor;
            visitor._currentScope = visitor._currentScope.CreateChildScope(method);
        }

        public void Dispose()
        {
            _visitor._currentScope.Parent!.CurrentNodeOrder = _visitor._currentScope.CurrentNodeOrder;
            _visitor._currentScope = _visitor._currentScope.Parent!;
        }
    }
}
