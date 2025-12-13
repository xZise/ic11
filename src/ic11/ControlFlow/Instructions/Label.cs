namespace ic11.ControlFlow.Instructions;
public class Label : Instruction
{
    public readonly string Name;

    public Label(string name)
    {
        Name = name;
    }

    public override string Render() => $"{Name}:";
}
