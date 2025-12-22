namespace ic11.Tests.Utils;

using ic11.Emulator;

public static class EmulatorHelper
{
    public static Emulator Create(string code, int cyclesPerTick = 128, string filename = "test.ic11")
    {
        var compileText = Program.CompileText(code, filename).Instructions;
        Console.WriteLine(compileText);

        var program = compileText.Split("\n");

        Emulator emulator = new(cyclesPerTick);
        emulator.LoadProgram(program);
        return emulator;
    }

    public static Emulator Run(string code, int cyclesPerTick = 128, string filename = "test.ic11", int maxCycles = 1000)
    {
        var emulator = Create(code, cyclesPerTick, filename);
        Run(emulator, maxCycles);
        return emulator;
    }

    public static void Run(Emulator emulator, int maxCycles = 1000)
    {
        while (!emulator.Stopped && --maxCycles > 0)
        {
            emulator.Run(1);
            emulator.PrintSummary();
        }

        emulator.PrintSummary();
    }
}