using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class ConstantDeclaration : Node, IStatement, IExpressionContainer
{
    public readonly LocatedText Name;
    public readonly INodeExpression Expression;

    public ConstantDeclaration(LocatedText name, SourceLocation sourceLocation, INodeExpression expression): base(sourceLocation)
    {
        Name = name;
        Expression = expression;
        expression.Parent = this;
    }

    public IEnumerable<INodeExpression> Expressions
    {
        get
        {
            yield return Expression;
        }
    }
}
