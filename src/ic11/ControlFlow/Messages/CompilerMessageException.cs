using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.Messages;

public class CompilerMessageException(string message, SourceLocation? sourceLocation) : Exception(message)
{
    public SourceLocation? SourceLocation => sourceLocation;

    public CompilerMessageException WithLocation(SourceLocation sourceLocation) => new(Message, sourceLocation);

    public CompilerMessage Error(SourceLocation alternativeSourceLocation) => new(Message, sourceLocation ?? alternativeSourceLocation);
}
