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
/*
Searches for any code after an unconditional jump (without link)

j jumpLabel
# unreachable code
otherLabel:
# maybe reachable code
jumpLabel:

Searches for any jumps immediately followed by labels (one of which is the destination)

j*,b* jumpLabel
otherLabel:
jumpLabel:
yetAnotherLabel:
*/
                    bool foundLabel = false;
                    for (int j = i + 1; j < instructions.Count; j++)
                    {
                        if (instructions[j] is Label label)
                        {
                            if (label.Name == jump.Destination)
                            {
                                instructions.RemoveAt(i);
                                j--;
                            }
                            foundLabel = true;
                        }
                        else if (!foundLabel && jump.Type == DataHolders.JumpType.J)
                        {
                            instructions.RemoveAt(j);
                            j--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
            }
        }
    }
}
