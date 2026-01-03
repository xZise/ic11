using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class Root : Node, IStatementsContainer
{
    public new Scope Scope => base.Scope!;

    public List<IStatement> Statements { get; init; } = new();
    private readonly List<PinDeclaration> pinDeclarations = new();

    public Root(string filename) : base(new(filename, 1, 0))
    {
        base.Scope = new Scope();
    }

    public (PinDeclaration? existingName, PinDeclaration? existingDevice) TryAdd(PinDeclaration pinDeclaration)
    {
        PinDeclaration? existingName = null;
        PinDeclaration? existingDevice = null;
        foreach (var existingDeclaration in pinDeclarations)
        {
            if (existingDeclaration.Name == pinDeclaration.Name)
                existingName = existingDeclaration;
            if (existingDeclaration.Device == pinDeclaration.Device)
                existingDevice = existingDeclaration;
        }
        if (existingName is null && existingDevice is null)
            pinDeclarations.Add(pinDeclaration);
        return (existingName, existingDevice);
    }
}
