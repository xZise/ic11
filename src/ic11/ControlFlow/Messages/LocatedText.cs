using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ic11.ControlFlow.Nodes;

public readonly struct LocatedText(IToken token)
{
    public LocatedText(ITerminalNode node): this(node.Symbol)
    {}

    public string Text { get; } = token.Text;
    public SourceLocation SourceLocation { get; } = new(token);

    public override string ToString() => Text;
}