using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class StatementParam0 : Node, IStatement
{
    public readonly string Operation;

    public StatementParam0(SourceLocation sourceLocation, string operation): base(sourceLocation)
    {
        Operation = operation;
    }
}
