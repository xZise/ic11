using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Instructions;

public class LiteralExpression: IExpression
{
    public Variable? Variable { get => null; set { } }
    public decimal? CtKnownValue { get; }

    public LiteralExpression(decimal? ctKnownValue)
    {
        CtKnownValue = ctKnownValue;
    }
}