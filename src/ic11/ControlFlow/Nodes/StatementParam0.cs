using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class StatementParam0 : Node, IStatement
{
    public readonly string Operation;

    public StatementParam0(string operation)
    {
        Operation = operation;
    }
}
