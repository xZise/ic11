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