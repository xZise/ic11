using ic11.ControlFlow.Context;
using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Instructions;
public class BatchAccess : Instruction
{
    public readonly Variable Destination;
    public readonly IExpression DeviceTypeHashExpr;
    public readonly IExpression? NameHashExpr;
    public readonly IExpression? TargetIndexExpr;
    public readonly DeviceTarget Target;
    public readonly string MemberName;
    public readonly string BatchMode;

    public BatchAccess(Variable destination, IExpression deviceTypeHashExpr, IExpression? nameHashExpr, IExpression? targetIndexExpression,
        DeviceTarget target, string memberName, string batchMode)
    {
        Destination = destination;
        DeviceTypeHashExpr = deviceTypeHashExpr;
        TargetIndexExpr = targetIndexExpression;
        Target = target;
        MemberName = memberName;
        NameHashExpr = nameHashExpr;
        BatchMode = batchMode;
    }

    public override string Render()
    {
        return (Target, NameHashExpr) switch
        {
            (DeviceTarget.Device, null) => $"lb {Destination.Register} {DeviceTypeHashExpr.Render()} {MemberName} {BatchMode}",
            (DeviceTarget.Device, not null) => $"lbn {Destination.Register} {DeviceTypeHashExpr.Render()} {NameHashExpr.Render()} {MemberName} {BatchMode}",

            (DeviceTarget.Slots, null) => $"lbs {Destination.Register} {DeviceTypeHashExpr.Render()} {TargetIndexExpr!.Render()} {MemberName} {BatchMode}",
            (DeviceTarget.Slots, not null) => $"lbns {Destination.Register} {DeviceTypeHashExpr.Render()} {NameHashExpr.Render()} {TargetIndexExpr!.Render()} {MemberName} {BatchMode}",

            _ => throw new Exception($"Batch read operation from devices of type{(NameHashExpr is null ? "" : " and name")} targeting {Target} is not supported"),
        };
    }
}
