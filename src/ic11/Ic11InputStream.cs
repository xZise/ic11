using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace ic11;

public class Ic11InputStream : AntlrInputStream
{
    public Ic11InputStream(string input, string filename): base(input)
    {
        name = filename;
    }

    public ReadOnlySpan<char> AsSpan(Interval interval)
    {
        return data.AsSpan(interval.a, interval.Length);
    }

    public new int ValueAt(int i)
    {
        return base.ValueAt(i);
    }
}