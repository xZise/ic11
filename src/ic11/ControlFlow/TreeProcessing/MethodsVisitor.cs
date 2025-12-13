using ic11.ControlFlow.Context;
using ic11.ControlFlow.Messages;
using ic11.ControlFlow.NodeInterfaces;
using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.TreeProcessing;
public class MethodsVisitor : ControlFlowContextTreeVisitorBase<bool>
{
    protected override Type VisitorType => typeof(MethodsVisitor);

    private MethodDeclaration _currentMethod;

    public MethodsVisitor(FlowContext flowContext) : base(flowContext)
    {
        AllowMethodSkip = true;
        SkippedReturnValue = false;
    }

    public void Visit(Root root)
    {
        var methodDeclarations = root.Statements.OfType<MethodDeclaration>();

        foreach (MethodDeclaration method in methodDeclarations)
        {
            try
            {
                Visit(method);
            }
            catch (CompilerMessageException ex)
            {
                _flowContext.CompilerMessages.Add(ex.Error(method.SourceLocation));
            }
        }
    }

    private bool Visit(Return node)
    {
        if (_currentMethod.ReturnType == DataHolders.MethodReturnType.Void && node.HasValue)
            throw new CompilerMessageException($"Unexpected return value in a void method", node.SourceLocation);

        if (_currentMethod.ReturnType != DataHolders.MethodReturnType.Void && !node.HasValue)
            throw new CompilerMessageException($"Return value expected", node.SourceLocation);

        return true;
    }

    private bool ContainReturn(IEnumerable<IStatement> statements)
    {
        bool foundReturnStatement = false;

        foreach (INode statement in statements)
        {
            if (foundReturnStatement)
            {
                statement.IsUnreachableCode = true;
                continue;
            }

            var guaranteedReturn = Visit(statement);

            if (guaranteedReturn)
                foundReturnStatement = true;
        }

        return foundReturnStatement;
    }

    private void Visit(MethodDeclaration node)
    {
        if (_flowContext.DeclaredMethods.ContainsKey(node.Name))
            throw new CompilerMessageException($"Method '{node.Name}' already exists", node.SourceLocation);

        if (node.Name == "Main" && node.Parameters.Any())
            throw new CompilerMessageException($"Method '{node.Name}' cannot have parameters", node.SourceLocation);

        if (node.Name == "Main" && node.ReturnType != DataHolders.MethodReturnType.Void)
            throw new CompilerMessageException($"Method '{node.Name}' cannot return value", node.SourceLocation);

        _flowContext.DeclaredMethods[node.Name] = node;
        _currentMethod = node;

        bool containReturn = ContainReturn(node.Statements);

        if (!containReturn && node.ReturnType == DataHolders.MethodReturnType.Real)
        {
            node.NotAllPathsReturnValue = true;
            throw new CompilerMessageException("Not all paths return a value", node.SourceLocation);
        }
    }

    private bool Visit(If node)
    {
        bool ifGuaranteedReturn = ContainReturn(node.IfStatements);
        bool elseGuaranteedReturn = ContainReturn(node.ElseStatements);

        return ifGuaranteedReturn && elseGuaranteedReturn;
    }

    private bool Visit(While node)
    {
        ContainReturn(node.Statements);

        return false;
    }

    private bool Visit(For node)
    {
        ContainReturn(node.Statements);

        return false;
    }

    private bool Visit(ArrayDeclaration node)
    {
        _currentMethod.ContainsArrays = true;

        return false;
    }
}
