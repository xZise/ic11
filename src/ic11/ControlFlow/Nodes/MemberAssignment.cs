using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class MemberAssignment : Node, IStatement, IExpressionContainer
{
    public readonly DeviceAddress Device;
    public readonly string? MemberName;
    public readonly DeviceTarget Target;
    public readonly INodeExpression? TargetIndexExpr;
    public readonly INodeExpression ValueExpression;

    public override int IndexSize => 2;

    public MemberAssignment(SourceLocation sourceLocation, DeviceAddress device, string memberName, INodeExpression valueExpression): base(sourceLocation)
    {
        Device = device;
        MemberName = memberName;
        ValueExpression = valueExpression;
        valueExpression.Parent = this;
        Target = DeviceTarget.Device;
        Validate();
    }

    public MemberAssignment(SourceLocation sourceLocation, DeviceAddress device, DeviceTarget target, string? memberName, INodeExpression targetIndexExpr, INodeExpression valueExpression): base(sourceLocation)
    {
        Device = device;
        MemberName = memberName;
        TargetIndexExpr = targetIndexExpr;
        ValueExpression = valueExpression;
        targetIndexExpr.Parent = this;
        valueExpression.Parent = this;
        Target = target;
        Validate();
    }

    public IEnumerable<INodeExpression> Expressions
    {
        get
        {
            yield return ValueExpression;
            if (Device.Expression is not null)
                yield return Device.Expression;
            if (TargetIndexExpr is not null)
                yield return TargetIndexExpr;
        }
    }

    private void Validate()
    {
        Device.Validate();
        if (Target != DeviceTarget.Stack && string.IsNullOrWhiteSpace(MemberName))
            throw new Exception($"Expected member name for device interaction");

        if (Target == DeviceTarget.Stack && MemberName is not null)
            throw new Exception($"Unexpected member name for device stack interaction");

        if (Target != DeviceTarget.Device && TargetIndexExpr is null)
            throw new Exception($"Expected target index expression");

        if (Target == DeviceTarget.Device && TargetIndexExpr is not null)
            throw new Exception($"Unexpected target index expression");

        if (Target == DeviceTarget.Reagents)
            throw new Exception($"Reagents are read-only");

        // Magic properties
        if (MemberName == Consts.PinSetProperty)
            throw new Exception($"Property {Consts.PinSetProperty} is read-only");

        if (MemberName == Consts.RmapProperty)
            throw new Exception($"Property {Consts.RmapProperty} is read-only");
    }
}
