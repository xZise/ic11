using ic11.ControlFlow.Instructions;

namespace ic11.ControlFlow.InstructionsProcessing;
public static class UselessInstructionRemover
{
    public static void Remove(List<Instruction> instructions)
    {
        for (int i = instructions.Count - 1; i >= 0; i--)
        {
            switch (instructions[i])
            {
                case Move mv:
                    if (mv.Expression.Render() == (mv.Register ?? mv.Destination!.Register))
                        instructions.RemoveAt(i);
                    break;
                case Jump jump:
                    for (int j = i + 1; j < instructions.Count && instructions[j] is Label label; j++)
                    {
                        if (label.Name == jump.Destination)
                        {
                            instructions.RemoveAt(i);
                            break;
                        }
                    }
                    break;
            }
        }
    }
}
