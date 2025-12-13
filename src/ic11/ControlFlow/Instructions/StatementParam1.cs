using ic11.ControlFlow.NodeInterfaces;

namespace ic11.ControlFlow.Instructions;
public class StatementParam1 : Instruction
{
    public readonly string Operation;
    public readonly IExpression Parameter;

    public StatementParam1(string operation, IExpression parameter)
    {
        Operation = operation;
        Parameter = parameter;
    }

    public override string Render() => $"{Operation} {Parameter.Render()}";
}
