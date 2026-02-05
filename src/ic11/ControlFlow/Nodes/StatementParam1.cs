using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class StatementParam1 : Node, IStatement, IExpressionContainer
{
    public readonly string Operation;
    public readonly INodeExpression Parameter;

    public IEnumerable<INodeExpression> Expressions
    {
        get
        {
            yield return Parameter;
        }
    }

    public StatementParam1(SourceLocation sourceLocation, string operation, INodeExpression parameter): base(sourceLocation)
    {
        Operation = operation;
        Parameter = parameter;
    }
}