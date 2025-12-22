using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class PinDeclaration : Node, IStatement
{
    public readonly string Name;
    public readonly string Device;

    public PinDeclaration(string name, SourceLocation sourceLocation, string device): base(sourceLocation)
    {
        Name = name;
        Device = device;
    }
}
