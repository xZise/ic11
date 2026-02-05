using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;

public class DeviceStackClear : Node, IStatement, IExpressionContainer
{
    public readonly DeviceAddress Device;

    public IEnumerable<INodeExpression> Expressions
    {
        get
        {
            if (Device.Expression is not null)
                yield return Device.Expression;
        }
    }

    public DeviceStackClear(SourceLocation sourceLocation, DeviceAddress device): base(sourceLocation)
    {
        Device = device;
    }    
}