using DNDGame.Console;
using DNDGame.Core.Models;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class CommandLineOptionsTests
{
    [TestMethod]
    public void TryParse_NoArgs_ReturnsMenuCommand()
    {
        var success = CommandLineOptions.TryParse([], out var options, out var error);

        Assert.IsTrue(success);
        Assert.IsNull(error);
        Assert.AreEqual(GameCommand.Menu, options.Command);
    }

    [TestMethod]
    public void TryParse_NewCommand_ParsesCharacterClass()
    {
        var success = CommandLineOptions.TryParse(["new", "--slot", "alpha", "--name", "Tarin", "--class", "ranger"], out var options, out var error);

        Assert.IsTrue(success);
        Assert.IsNull(error);
        Assert.AreEqual(GameCommand.New, options.Command);
        Assert.AreEqual("alpha", options.SaveSlot);
        Assert.AreEqual("Tarin", options.HeroName);
        Assert.AreEqual(CharacterClass.Ranger, options.CharacterClass);
    }
}