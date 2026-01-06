using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;
using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.TreeProcessing;

public class UnreachableCodeVisitor : ControlFlowContextTreeVisitorBase<bool>
{
    protected override Type VisitorType => typeof(UnreachableCodeVisitor);

    public UnreachableCodeVisitor(FlowContext flowContext) : base(flowContext)
    {
        AllowMethodSkip = true;
        SkippedReturnValue = false;
    }

    private bool CheckStatements(IEnumerable<IStatement> statements)
    {
        bool exitedLoopBody = false;
        bool addedWarning = false;
        foreach (var statement in statements)
        {
            if (exitedLoopBody)
            {
                if (!addedWarning)
                {
                    _flowContext.CompilerMessages.Add(new("Unreachable code detected", statement.SourceLocation, Messages.Severity.Warning));
                    addedWarning = true;
                }
                statement.IsUnreachableCode = true;
            }
            else if (Visit(statement))
            {
                exitedLoopBody = true;
            }
        }
        return exitedLoopBody;
    }

    private bool Visit(Return node) => true;
    private bool Visit(Break node) => true;
    private bool Visit(Continue node) => true;

    protected override bool Visit(If node)
    {
        if (node.IfStatements.Count == 0 || node.ElseStatements.Count == 0)
            return false;

        bool exitedInThenPart = CheckStatements(node.IfStatements);
        bool exitedInElsePart = CheckStatements(node.ElseStatements);

        return exitedInThenPart && exitedInElsePart;
    }

    protected override bool Visit(While node)
    {
        CheckStatements(node.Statements);
        return false;
    }

    protected override bool Visit(For node)
    {
        CheckStatements(node.Statements);
        return false;
    }
}
