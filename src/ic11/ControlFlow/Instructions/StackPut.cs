using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Instructions;
public class StackPut : Instruction
{
    public readonly string Device;
    public readonly IExpression AddressExpression;
    public readonly IExpression ValueExpression;

    public StackPut(string device, IExpression addressExpression, IExpression valueExpression)
    {
        Device = device;
        AddressExpression = addressExpression;
        ValueExpression = valueExpression;
    }

    public override string Render() => $"put {Device} {AddressExpression.Render()} {ValueExpression.Render()}";
}
