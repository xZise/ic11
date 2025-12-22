using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class BatchAssignment : Node, IStatement, IExpressionContainer
{
    public readonly INodeExpression DeviceTypeHashExpr;
    public readonly INodeExpression? NameHashExpr;
    public readonly INodeExpression? TargetIndexExpr;
    public readonly INodeExpression ValueExpr;
    public readonly DeviceTarget Target;
    public readonly string MemberName;
    public override int IndexSize => 2;

    public BatchAssignment(SourceLocation sourceLocation, INodeExpression deviceTypeHashExpr, INodeExpression? nameHashExpr, INodeExpression? targetIndexExpression,
        INodeExpression valueExpr, string memberName, DeviceTarget target): base(sourceLocation)
    {
        DeviceTypeHashExpr = deviceTypeHashExpr;
        NameHashExpr = nameHashExpr;
        ValueExpr = valueExpr;
        TargetIndexExpr = targetIndexExpression;
        deviceTypeHashExpr.Parent = this;
        valueExpr.Parent = this;

        if (nameHashExpr is not null)
            nameHashExpr.Parent = this;
            
        if (targetIndexExpression is not null)
            targetIndexExpression.Parent = this;

        Target = target;

        MemberName = memberName;
    }

    public IEnumerable<INodeExpression> Expressions
    {
        get
        {
            yield return ValueExpr;
            yield return DeviceTypeHashExpr;

            if (NameHashExpr is not null)
                yield return NameHashExpr;

            if (TargetIndexExpr is not null)
                yield return TargetIndexExpr;
        }
    }
}
