using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Instructions;
public class BinaryOperation : Instruction
{
    public readonly Variable Destination;
    public readonly IExpression Left;
    public readonly IExpression Right;
    public readonly string Operation;

    public BinaryOperation(Variable destination, IExpression left, IExpression right, string operation)
    {
        Destination = destination;
        Left = left;
        Right = right;
        Operation = operation;
    }

    public override string Render() => $"{Operation} {Destination.Register} {Left.Render()} {Right.Render()}";
}
