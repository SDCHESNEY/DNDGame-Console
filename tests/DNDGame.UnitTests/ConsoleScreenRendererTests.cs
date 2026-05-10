using DNDGame.Console;
using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class ConsoleScreenRendererTests
{
    [TestMethod]
    public async Task WriteMenuAsync_WithLeadingBlankLine_WritesHeadingAndOptions()
    {
        using var writer = new StringWriter();
        var renderer = new ConsoleScreenRenderer(writer);

        await renderer.WriteMenuAsync("Main Menu", ["1. New game", "2. Quit"], leadingBlankLine: true);

        var output = NormalizeNewLines(writer.ToString());
        Assert.AreEqual("\n== Main Menu ==\n1. New game\n2. Quit\n", output);
    }

    [TestMethod]
    public async Task WriteNarratedBlockAsync_WritesHeadingRecapAndNarration()
    {
        using var writer = new StringWriter();
        var renderer = new ConsoleScreenRenderer(writer);
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);

        await renderer.WriteNarratedBlockAsync("New Campaign", campaign, "A quiet wind crosses the outpost.");

        var output = NormalizeNewLines(writer.ToString());
        StringAssert.Contains(output, "== New Campaign ==\n");
        StringAssert.Contains(output, CampaignRecapBuilder.Build(campaign).Replace(Environment.NewLine, "\n"));
        StringAssert.Contains(output, "\nA quiet wind crosses the outpost.\n");
    }

    [TestMethod]
    public async Task WriteEncounterScreenAsync_WritesEnemyHeroAndCombatOptions()
    {
        using var writer = new StringWriter();
        var renderer = new ConsoleScreenRenderer(writer);
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        await renderer.WriteEncounterScreenAsync(campaign);

        var output = NormalizeNewLines(writer.ToString());
        StringAssert.Contains(output, "== Encounter ==\n");
        StringAssert.Contains(output, "Enemy: Goblin Scout 18/18\n");
        StringAssert.Contains(output, "Hero: Mira 24/24\n");
        StringAssert.Contains(output, "1. Attack\n2. Defend\n3. Special\n4. Use item\n5. Retreat to campaign menu\n");
    }

    private static string NormalizeNewLines(string text)
    {
        return text.Replace("\r\n", "\n");
    }
}