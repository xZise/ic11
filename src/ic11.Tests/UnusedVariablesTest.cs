namespace ic11.Tests;

using ic11.ControlFlow.Including;
using ic11.ControlFlow.Messages;

[TestClass]
public sealed class UnusedVariablesTest
{
    [TestMethod]
    public void UnusedVariable()
    {
        var code = @"
            void Main()
            {
                var x = 42;
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(4, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void ValidVariable()
    {
        var code = @"
            void Main()
            {
                var x = 42;
                Base.Result = x;
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(0, messages.Count);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void ValidVariableConditional()
    {
        var code = @"
            void Main()
            {
                var x = 42;
                if (Base.Condition)
                    Base.Result = x;
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(0, messages.Count);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void UnusedDevice()
    {
        var code = @"
            pin A d0;

            void Main()
            {
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(2, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }
}