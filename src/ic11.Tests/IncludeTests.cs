namespace ic11.Tests;

using ic11.Tests.Utils;

[TestClass]
public sealed class IncludeTests
{
    [TestMethod]
    public void TestIncludeConstant()
    {
        var code = @"
            #include ""math.ic11""

            pin Display d0;

            void Main()
            {
                Display.Value = PI;
            }
        ";

        var handler = new TestIncludeHandler();
        handler.AvailableIncludes.Add("math.ic11", "const PI = 3.14");
        var emulator = EmulatorHelper.Run(code, handler: handler);
        var dev = emulator.Devices[0]!;
        Assert.AreEqual(3.14, dev.GeneralProperties["Value"]);
    }
}
