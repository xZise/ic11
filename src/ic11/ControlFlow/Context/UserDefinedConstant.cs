using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.Context;
public class UserDefinedConstant
{
    public readonly string Name;
    public SourceLocation SourceLocation { get; }
    public readonly decimal CtKnownValue;
    public readonly int DeclaredIndex;
    public int LastReferencedIndex = -1;

    public UserDefinedConstant(string name, SourceLocation sourceLocation, decimal ctKnownValue, int declaredIndex)
    {
        Name = name;
        SourceLocation = sourceLocation;
        CtKnownValue = ctKnownValue;
        DeclaredIndex = declaredIndex;
    }

    public override string ToString() =>
        $"{{ {Name}, declared {DeclaredIndex}, last referenced {LastReferencedIndex}, value {CtKnownValue}";
}
