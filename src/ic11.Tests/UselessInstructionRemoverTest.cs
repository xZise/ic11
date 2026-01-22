namespace ic11.Tests;

using ic11.ControlFlow.DataHolders;
using ic11.ControlFlow.Instructions;
using ic11.ControlFlow.InstructionsProcessing;

[TestClass]
public sealed class UselessInstructionRemoverTest
{
    private const string LABEL_NAME = "label";
    private const string OTHER_LABEL_NAME = "other";

    [TestMethod]
    [DataRow(JumpType.J)]
    [DataRow(JumpType.Jal)]
    [DataRow(JumpType.Bgtz)]
    [DataRow(JumpType.Blez)]
    [DataRow(JumpType.Beqz)]
    [DataRow(JumpType.Bnez)]
    public void TestJumpToNextLabel(JumpType jumpType)
    {
        var label = new Label(LABEL_NAME);
        List<Instruction> instructions = [
            new Jump(jumpType, LABEL_NAME),
            label,
        ];
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AreEqual(new[] { label }, instructions);
    }

    [TestMethod]
    [DataRow(JumpType.J)]
    [DataRow(JumpType.Jal)]
    [DataRow(JumpType.Bgtz)]
    [DataRow(JumpType.Blez)]
    [DataRow(JumpType.Beqz)]
    [DataRow(JumpType.Bnez)]
    public void TestJumpToNextLine(JumpType jumpType)
    {
        var label = new Label(LABEL_NAME);
        var otherLabel = new Label(OTHER_LABEL_NAME);
        List<Instruction> instructions = [
            new Jump(jumpType, LABEL_NAME),
            otherLabel,
            label,
        ];
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AreEqual(new[] { otherLabel, label }, instructions);
    }

    [TestMethod]
    public void TestJumpToNextLabelWithUnreachableCode()
    {
        var label = new Label(LABEL_NAME);
        List<Instruction> instructions = [
            new Jump(JumpType.J, LABEL_NAME),
            new Jump(JumpType.J, "does not matter"),
            label,
        ];
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AreEqual(new[] { label }, instructions);
    }

    [TestMethod]
    public void TestJumpToNextLineWithUnreachableCode()
    {
        var jump = new Jump(JumpType.J, LABEL_NAME);
        var otherLabel = new Label(OTHER_LABEL_NAME);
        var label = new Label(LABEL_NAME);
        List<Instruction> instructions = [
            jump,
            new Jump(JumpType.J, "does not matter"),
            otherLabel,
            label,
        ];
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AreEqual(new Instruction[] { otherLabel, label }, instructions);
    }

    [TestMethod]
    public void TestRemoveUnreachableCode()
    {
        List<Instruction> instructions = [
            new Jump(JumpType.J, LABEL_NAME),
            new Jump(JumpType.J, "does not matter"),
            new Label(OTHER_LABEL_NAME),
            new StackPush("r0"),
            new Label(LABEL_NAME),
        ];
        var expected = new List<Instruction>(instructions);
        expected.RemoveAt(1);
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AreEqual(expected, instructions);
    }

    [TestMethod]
    [DataRow(JumpType.Jal)]
    [DataRow(JumpType.Bgtz)]
    [DataRow(JumpType.Blez)]
    [DataRow(JumpType.Beqz)]
    [DataRow(JumpType.Bnez)]
    public void TestMayBeReachableCode(JumpType jumpType)
    {
        List<Instruction> instructions = [
            new Jump(jumpType, LABEL_NAME),
            new Jump(JumpType.J, "does not matter"),
            new Label(LABEL_NAME),
        ];
        var original = new List<Instruction>(instructions);
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AreEqual(original, instructions);
    }

    [TestMethod]
    [DataRow([JumpType.Beqz, "0"])]
    [DataRow([JumpType.Beqz, "r0"])]
    [DataRow([JumpType.Bgtz, "1"])]
    [DataRow([JumpType.Bnez, "1"])]
    [DataRow([JumpType.Blez, "0"])]
    public void TestUsefulBranch(JumpType jumpType, string arg1)
    {
        List<Instruction> instructions = [
            new Jump(jumpType, LABEL_NAME, arg1)
        ];
        var original = new List<Instruction>(instructions);
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AreEqual(original, instructions);
    }

    [TestMethod]
    public void TestJumpToNextLineAlternating()
    {
        List<Instruction> instructions = [
            new Jump(JumpType.J, LABEL_NAME),
            new Jump(JumpType.J, OTHER_LABEL_NAME),
            new Label(LABEL_NAME),
            new Label(OTHER_LABEL_NAME),
        ];
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AllItemsAreInstancesOfType(instructions, typeof(Label));
    }

    [TestMethod]
    public void TestJumpToNextLineOnion()
    {
        List<Instruction> instructions = [
            new Jump(JumpType.J, LABEL_NAME),
            new Jump(JumpType.J, OTHER_LABEL_NAME),
            new Label(OTHER_LABEL_NAME),
            new Label(LABEL_NAME),
        ];
        UselessInstructionRemover.Remove(instructions);
        CollectionAssert.AllItemsAreInstancesOfType(instructions, typeof(Label));
    }
}