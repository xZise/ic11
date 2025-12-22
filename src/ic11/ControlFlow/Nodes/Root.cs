using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Nodes;
public class Root : Node, IStatementsContainer
{
    public new Scope Scope => base.Scope!;

    public List<IStatement> Statements { get; init; } = new();
    public Dictionary<string, string> DevicePinMap = new();

    public Root(string filename) : base(new(filename, 1, 0))
    {
        base.Scope = new Scope();
    }
}
