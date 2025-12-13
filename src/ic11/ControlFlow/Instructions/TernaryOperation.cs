using ic11.ControlFlow.Context;
using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Instructions;
public class TernaryOperation : Instruction
{
    public readonly Variable Destination;
    public readonly IExpression OperandA;
    public readonly IExpression OperandB;
    public readonly IExpression OperandC;
    public readonly string Operation;

    public TernaryOperation(Variable destination, IExpression operandA, IExpression operandB, IExpression operandC, string operation)
    {
        Destination = destination;
        OperandA = operandA;
        OperandB = operandB;
        OperandC = operandC;
        Operation = operation;
    }

    public override string Render() => $"{Operation} {Destination.Register} {OperandA.Render()} {OperandB.Render()} {OperandC.Render()}";
}
