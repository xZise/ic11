using ic11.ControlFlow.Instructions;

public static class UnreachableCodeRemover
{
    public static void Remove(List<Instruction> instructions)
    {
        bool[] visited = new bool[instructions.Count];
        List<string> destinations = [];
        int destinationIndex = -1;
        Array.Fill(visited, false);
        bool foundDestination = true;
        do
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                if (!foundDestination)
                {
                    if (instructions[i] is not Label label || label.Name != destinations[destinationIndex])
                        continue;

                    foundDestination = true;
                }

                if (visited[i])
                    break;

                visited[i] = true;
                if (instructions[i] is Jump jump)
                {
                    if (!destinations.Contains(jump.Destination))
                        destinations.Add(jump.Destination);
                    if (jump.Type == ic11.ControlFlow.DataHolders.JumpType.J)
                        break;
                }
            }
            
            destinationIndex++;
            foundDestination = false;
        }
        while (destinationIndex < destinations.Count);

        for (int i = instructions.Count - 1; i >= 0 ; i--)
            if (!visited[i])
                instructions.RemoveAt(i);
    }
}