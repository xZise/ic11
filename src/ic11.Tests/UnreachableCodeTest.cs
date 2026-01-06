namespace ic11.Tests;

using ic11.ControlFlow.Including;
using ic11.ControlFlow.Messages;

[TestClass]
public sealed class UnreachableCodeTest
{

    [TestMethod]
    public void UnreachableForContinue()
    {
        var code = @"
            void Main()
            {
                for (var i = 0; i < 10; i = i + 1) {
                    continue;
                    Base.Member = 42;   //    warning here
                    Base.Second = 1337; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(6, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void UnreachableWhileContinue()
    {
        var code = @"
            void Main()
            {
                while (true) {
                    continue;
                    Base.Member = 42;   //    warning here
                    Base.Second = 1337; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(6, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void UnreachableForBreak()
    {
        var code = @"
            void Main()
            {
                for (var i = 0; i < 10; i = i + 1) {
                    break;
                    Base.Member = 42;   //    warning here
                    Base.Second = 1337; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(6, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void UnreachableWhileBreak()
    {
        var code = @"
            void Main()
            {
                while (true) {
                    break;
                    Base.Member = 42;   //    warning here
                    Base.Second = 1337; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(6, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void UnreachableForReturn()
    {
        var code = @"
            void Main()
            {
                for (var i = 0; i < 10; i = i + 1) {
                    return;
                    Base.Member = 42;   //    warning here
                    Base.Second = 1337; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(6, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void UnreachableWhileReturn()
    {
        var code = @"
            void Main()
            {
                while (true) {
                    return;
                    Base.Member = 42;   //    warning here
                    Base.Second = 1337; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(6, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void UnreachableConditional()
    {
        var code = @"
            void Main()
            {
                while (true) {
                    if (Base.Condition) {
                        break;
                    } else {
                        continue;
                    }
                    Base.Member = 42;   //    warning here
                    Base.Second = 1337; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(Severity.Warning, messages[0].Severity);
        Assert.AreEqual(10, messages[0].SourceLocation.LineNumber);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void ValidOnlyThenBreak()
    {
        var code = @"
            void Main()
            {
                while (true) {
                    if (Base.Condition) {
                        break;
                    }
                    Base.Member = 42; // no warning here
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(0, messages.Count);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void ValidOnlyThenBreakWithElse()
    {
        var code = @"
            void Main()
            {
                while (true) {
                    if (Base.Condition) {
                        break;
                    } else {
                        Base.Useless = 1337;
                    }
                    Base.Member = 42;
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(0, messages.Count);
        Assert.IsTrue(instructions.Length > 0);
    }

    [TestMethod]
    public void ValidOnlyElseBreak()
    {
        var code = @"
            void Main()
            {
                while (true) {
                    if (Base.Condition) {
                        Base.Useless = 1337;
                    } else {
                        break;
                    }
                    Base.Member = 42;
                }
            }
        ";

        var (instructions, messages) = Program.CompileText(code, "test.ic11", NoIncludeHandler.Instance);
        Assert.AreEqual(0, messages.Count);
        Assert.IsTrue(instructions.Length > 0);
    }
}