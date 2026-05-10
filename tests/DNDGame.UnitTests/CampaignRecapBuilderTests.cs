using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class CampaignRecapBuilderTests
{
    [TestMethod]
    public void Build_WithoutEncounter_IncludesHeroQuestInventoryAndLatestJournal()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);

        var recap = NormalizeNewLines(CampaignRecapBuilder.Build(campaign));

        StringAssert.Contains(recap, "Hero: Mira the level 1 Mage\n");
        StringAssert.Contains(recap, "Quest Stage: Accepted\n");
        StringAssert.Contains(recap, "Inventory: Minor Potion\n");
        StringAssert.Contains(recap, "Latest Journal: ");
    }

    [TestMethod]
    public void Build_WithEncounter_IncludesEncounterState()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        var recap = NormalizeNewLines(CampaignRecapBuilder.Build(campaign));

        StringAssert.Contains(recap, "Quest Stage: AtWatchtowerApproach\n");
        StringAssert.Contains(recap, "Encounter: Watchtower Approach vs Goblin Scout (18/18)\n");
        StringAssert.Contains(recap, "Latest Journal: You followed the eastern road and reached the ruined watchtower approach.");
    }

    [TestMethod]
    public void Build_WithActiveStatuses_IncludesEncounterStatusSummary()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign;
        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;

        var recap = NormalizeNewLines(CampaignRecapBuilder.Build(campaign));

        StringAssert.Contains(recap, "Encounter Status: Enemy Sundered (1)\n");
    }

    [TestMethod]
    public void Build_WithRecapSnapshots_IncludesCompactedHistorySummary()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage) with
        {
            RecapSnapshots =
            [
                new RecapSnapshot(DateTimeOffset.UtcNow, "Earlier campaign recap: Reached the watchtower | Drove back the scout")
            ],
        };

        var recap = NormalizeNewLines(CampaignRecapBuilder.Build(campaign));

        StringAssert.Contains(recap, "Recap Snapshots: Earlier campaign recap: Reached the watchtower | Drove back the scout\n");
    }

    private static string NormalizeNewLines(string text)
    {
        return text.Replace("\r\n", "\n");
    }
}