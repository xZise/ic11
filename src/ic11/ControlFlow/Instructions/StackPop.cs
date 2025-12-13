using ic11.ControlFlow.Context;

namespace ic11.ControlFlow.Instructions;
public class StackPop : Instruction
{
    public readonly Variable? Destination;
    public readonly string? Register;

    public StackPop(Variable destination)
    {
        Destination = destination;
    }

    public StackPop(string? register)
    {
        Register = register;
    }

    public override string Render() => $"pop {Register ?? Destination!.Register}";
}
