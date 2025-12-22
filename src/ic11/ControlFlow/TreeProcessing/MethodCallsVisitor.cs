using ic11.ControlFlow.Context;
using ic11.ControlFlow.Messages;
using ic11.ControlFlow.NodeInterfaces;
using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.TreeProcessing;
public class MethodCallsVisitor
{
    private readonly FlowContext _flowContext;
    private readonly List<MethodCall> _methodCalls = new();

    public MethodCallsVisitor(FlowContext context)
    {
        _flowContext = context;
    }

    public void VisitRoot(Root root)
    {
        Visit((IStatementsContainer)root);

        foreach (var item in _methodCalls)
        {
            if (!_flowContext.DeclaredMethods.TryGetValue(item.Name, out var declaredMethod))
                throw new CompilerMessageException($"Method '{item.Name}' is not defined", item.SourceLocation);

            if (item.Name == "Main")
                throw new CompilerMessageException("Don't call Main", item.SourceLocation);

            if (item.ArgumentExpressions.Count != declaredMethod.Parameters.Count)
                throw new CompilerMessageException($"Expected {declaredMethod.Parameters.Count} parameter(s) but called with {item.ArgumentExpressions.Count} parameter(s)", item.SourceLocation);

            item.Method = declaredMethod;
        }
    }

    private void Visit(IStatementsContainer node)
    {
        IEnumerable<IStatement> statements;

        if (node is If ifn)
        {
            ifn.CurrentStatementsContainer = DataHolders.IfStatementsContainer.If;
            statements = ifn.Statements;

            ifn.CurrentStatementsContainer = DataHolders.IfStatementsContainer.Else;
            statements = statements.Concat(ifn.Statements);
        }
        else
        {
            statements = node.Statements;
        }

        foreach (var item in statements)
            Visit(item);
    }

    private void Visit(IExpressionContainer node)
    {
        foreach (var item in node.Expressions)
            Visit(item);
    }

    private void Visit(INode node)
    {
        if (node is IExpressionContainer ic)
            Visit(ic);

        if (node is IStatementsContainer sc)
            Visit(sc);

        if (node is MethodCall mc)
            _methodCalls.Add(mc);
    }
}
