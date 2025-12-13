namespace ic11.ControlFlow.Instructions;
public class StatementParam0 : Instruction
{
    public readonly string Operation;

    public StatementParam0(string operation)
    {
        Operation = operation;
    }

    public override string Render() => $"{Operation}";
}
