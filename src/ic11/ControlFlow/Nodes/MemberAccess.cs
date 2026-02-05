using ic11.ControlFlow.Context;
using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class MemberAccess : Node, INodeExpression, IExpressionContainer
{
    public readonly string? MemberName;
    public readonly DeviceAddress Device;
    public readonly DeviceTarget Target;
    public readonly INodeExpression? TargetIndexExpr;

    public Variable? Variable { get; set; }

    public MemberAccess(SourceLocation sourceLocation, DeviceAddress device, string memberName): base(sourceLocation)
    {
        Device = device;
        MemberName = memberName;
        Target = DeviceTarget.Device;
        Validate();
    }

    public MemberAccess(SourceLocation sourceLocation, DeviceAddress device, DeviceTarget target, INodeExpression targetIndexExpr, string? memberName): base(sourceLocation)
    {
        Device = device;
        TargetIndexExpr = targetIndexExpr;
        MemberName = memberName;
        Target = target;
        targetIndexExpr.Parent = this;
        Validate();
    }

    public IEnumerable<INodeExpression> Expressions
    {
        get
        {
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

        // Magic properties
        if (Target != DeviceTarget.Device && MemberName == Consts.PinSetProperty)
            throw new Exception($"Property {Consts.PinSetProperty} is only relevant for device itself");

        if (Target != DeviceTarget.Reagents && MemberName == Consts.RmapProperty)
            throw new Exception($"Property {Consts.RmapProperty} is only relevant for reagents");
    }
}
