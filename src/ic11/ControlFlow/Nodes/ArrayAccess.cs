using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class ArrayAccess : Node, INodeExpression, IExpressionContainer
{
    public readonly string Name;
    public readonly INodeExpression IndexExpression;
    public Variable? Variable { get; set; }
    public UserDefinedVariable? ArrayAddressVariable;

    public ArrayAccess(string name, SourceLocation sourceLocation, INodeExpression indexExpression): base(sourceLocation)
    {
        Name = name;
        IndexExpression = indexExpression;
        indexExpression.Parent = this;
    }

    public IEnumerable<INodeExpression> Expressions
    {
        get
        {
            yield return IndexExpression;
        }
    }
}
