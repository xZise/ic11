using ic11.ControlFlow.Context;

namespace ic11.ControlFlow.Instructions;
public class NullaryOperation : Instruction
{
    public readonly Variable Destination;
    public readonly string Operation;

    public NullaryOperation(Variable destination, string operation)
    {
        Destination = destination;
        Operation = operation;
    }

    public override string Render() =>
        $"{Operation} {Destination.Register}";
}
