using ic11.ControlFlow.Nodes;

namespace ic11.ControlFlow.Messages;

public readonly record struct CompilerMessage(string Message, SourceLocation SourceLocation, Severity Severity = Severity.Error);
