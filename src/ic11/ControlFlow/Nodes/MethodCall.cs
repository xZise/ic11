using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class MethodCall : Node, IStatement, INodeExpression, IExpressionContainer
{
    public readonly string Name;
    public MethodDeclaration? Method;
    public readonly List<INodeExpression> ArgumentExpressions;
    public Variable? Variable { get; set; }
    public HashSet<string> RegistersToPush;

    public override int IndexSize => 2;

    public MethodCall(string name, SourceLocation sourceLocation, List<INodeExpression> argumentExpressions): base(sourceLocation)
    {
        Name = name;
        ArgumentExpressions = argumentExpressions;

        foreach (var item in argumentExpressions)
            item.Parent = this;
    }

    public IEnumerable<INodeExpression> Expressions => ArgumentExpressions;
}
