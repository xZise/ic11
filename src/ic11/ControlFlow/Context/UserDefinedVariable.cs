using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.Context;
public class UserDefinedVariable
{
    public readonly string Name;
    public SourceLocation SourceLocation { get; }
    public readonly Variable Variable;
    public readonly int DeclaredIndex;
    public int LastReassignedIndex = -1;
    public int LastReferencedIndex = -1;
    public readonly bool IsDeclaredWithCtKnownValue;

    public UserDefinedVariable(string name, SourceLocation sourceLocation, Variable variable, int declaredIndex, bool isDeclaredWithCtKnownValue)
    {
        Name = name;
        SourceLocation = sourceLocation;
        Variable = variable;
        DeclaredIndex = declaredIndex;
        IsDeclaredWithCtKnownValue = isDeclaredWithCtKnownValue;
    }

    public override string ToString() =>
        $"{{ {Name}, declared {DeclaredIndex}, last referenced {LastReferencedIndex}, " +
        $"last reassigned {LastReassignedIndex}, declared ctKnown = {IsDeclaredWithCtKnownValue} }}";
}
