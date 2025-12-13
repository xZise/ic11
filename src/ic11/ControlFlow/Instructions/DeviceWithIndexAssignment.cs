using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Instructions;
public class DeviceWithIndexAssignment : Instruction
{
    public readonly IExpression DeviceIndexExpr;
    public readonly DeviceIndexType IndexType;
    public readonly IExpression? TargetIndexExpr;
    public readonly DeviceTarget Target;
    public readonly string? MemberName;
    public readonly IExpression ValueExpr;

    public DeviceWithIndexAssignment(IExpression deviceIndexExpr, DeviceIndexType indexType, string memberName, IExpression expression)
    {
        DeviceIndexExpr = deviceIndexExpr;
        IndexType = indexType;
        Target = DeviceTarget.Device;
        MemberName = memberName;
        ValueExpr = expression;
    }

    public DeviceWithIndexAssignment(IExpression deviceIndexExpr, DeviceIndexType indexType, IExpression targetIndexExpr, DeviceTarget target,
        string? memberName, IExpression expression)
    {
        DeviceIndexExpr = deviceIndexExpr;
        IndexType = indexType;
        TargetIndexExpr = targetIndexExpr;
        Target = target;
        MemberName = memberName;
        ValueExpr = expression;
    }

    public override string Render()
    {
        return (IndexType, Target) switch
        {
            (DeviceIndexType.Pin, DeviceTarget.Device) => $"s d{DeviceIndexExpr.Render()} {MemberName} {ValueExpr.Render()}",
            (DeviceIndexType.Pin, DeviceTarget.Slots) => $"ss d{DeviceIndexExpr.Render()} {TargetIndexExpr!.Render()} {MemberName} {ValueExpr.Render()}",
            (DeviceIndexType.Pin, DeviceTarget.Stack) => $"put d{DeviceIndexExpr.Render()} {TargetIndexExpr!.Render()} {ValueExpr.Render()}",

            (DeviceIndexType.Id, DeviceTarget.Device) => $"sd {DeviceIndexExpr.Render()} {MemberName} {ValueExpr.Render()}",
            (DeviceIndexType.Id, DeviceTarget.Stack) => $"putd {DeviceIndexExpr.Render()} {TargetIndexExpr!.Render()} {ValueExpr.Render()}",

            _ => throw new Exception($"Write operation for device access by {IndexType} and target {Target} is not supported"),
        };
    }
}
