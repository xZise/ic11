namespace ic11.Tests;

[TestClass]
public class ExampleTests
{
    private static readonly string EXAMPLES_DIRECTORY = Path.Combine(AppContext.BaseDirectory, "examples");
    private static readonly HashSet<string> KNOWN_BROKEN = [
    ];

    [TestMethod]
    [DynamicData(nameof(GetExampleFiles), DynamicDataSourceType.Method)]
    public void CompileExample(string filename)
    {
        string relativePath = Path.GetRelativePath(EXAMPLES_DIRECTORY, filename);
        if (KNOWN_BROKEN.Contains(relativePath.Replace('\\', '/')))
        {
            Assert.Inconclusive($"Known failing example: {filename}");
        }

        string code = File.ReadAllText(filename);
        (var Instructions, var CompilerMessages) = Program.CompileText(code, filename);

        Assert.IsFalse(CompilerMessages.Any(m => m.Severity == ControlFlow.Messages.Severity.Error));
        Assert.AreNotEqual(0, Instructions.Length);
    }

    public static IEnumerable<object[]> GetExampleFiles()
    {
        foreach (var file in Directory.EnumerateFiles(EXAMPLES_DIRECTORY, "*.ic11", SearchOption.AllDirectories))
        {
            yield return new object[] { file };
        }
    }
}
