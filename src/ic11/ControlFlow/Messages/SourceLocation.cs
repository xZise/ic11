using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ic11.ControlFlow.Nodes;

public readonly struct SourceLocation
{
    public string FileName { get; }
    public int LineNumber { get; }
    public int Column { get; }
    private readonly (int Start, int Stop, Ic11InputStream InputStream)? _fileData;

    public SourceLocation(IToken token): this(token, null)
    { }

    private SourceLocation(IToken start, IToken? end)
    {
        ArgumentNullException.ThrowIfNull(start);
        FileName = start.TokenSource?.InputStream.SourceName ?? "<unnamed>";
        LineNumber = start.Line;
        Column = start.Column + 1;

        if (start.TokenSource?.InputStream is Ic11InputStream input)
        {
            int stopIndex = end?.StopIndex ?? start.StopIndex;
            _fileData = (start.StartIndex, stopIndex, input);
        }
    }

    public SourceLocation(string filename, int line, int column)
    {
        FileName = filename;
        LineNumber = line;
        Column = column;
    }

    public static SourceLocation FromTerminalNode(ITerminalNode terminalNode)
    {
        return new SourceLocation(terminalNode.Symbol);
    }

    public static SourceLocation FromRuleContext(ParserRuleContext context)
    {
        return new SourceLocation(context.Start, context.Stop);
    }

    public override string ToString()
    {
        return $"{FileName}:line {LineNumber}:{Column}";
    }

    public string? ShowErrorCode()
    {
        if (_fileData == null)
            return null;

        Ic11InputStream input = _fileData.Value.InputStream;
        int start = _fileData.Value.Start;
        int stop = _fileData.Value.Stop;

        int lineStart = start;
        while (lineStart > 0)
        {
            char c = (char)input.ValueAt(lineStart);
            if (c == '\n' || c == '\r')
            {
                lineStart++;
                break;
            }
            lineStart--;
        }

        int lineEnd = start;
        while (lineEnd < input.Size)
        {
            char c = (char)input.ValueAt(lineEnd + 1);
            if (c == '\n' || c == '\r')
                break;
            lineEnd++;
        }

        ReadOnlySpan<char> lineText = input.AsSpan(new Interval(lineStart, lineEnd));

        var sb = new StringBuilder();
        sb.Append("  ");
        int columnStart = 0;
        int columnEnd = 0;
        int indentOffset = sb.Length;
        for (int i = 0; i < lineText.Length; i++)
        {
            char c = lineText[i];
            if (c == '\t')
                sb.Append(' ', 4);
            else
                sb.Append(c);
            
            if (lineStart + i < start)
                columnStart = sb.Length - indentOffset;
            if (lineStart + i <= stop)
                columnEnd = sb.Length - indentOffset;
        }
        sb.AppendLine();
        sb.Append(" -").Append('-', columnStart).Append('^', columnEnd - columnStart);

        return sb.ToString();
    }
}
