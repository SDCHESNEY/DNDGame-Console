using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class CombatResolutionServiceTests
{
    [TestMethod]
    public void ResolveTurn_WhenSpecialUsedByMage_ClearsOpeningEncounter()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);
        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        var firstTurn = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;
        var secondTurn = CombatResolutionService.ResolveTurn(firstTurn, CombatAction.Special);

        Assert.AreEqual(QuestStage.WatchtowerCleared, secondTurn.Campaign.ActiveQuest.Stage);
        Assert.IsNull(secondTurn.Campaign.CurrentEncounter);
        Assert.AreEqual("Ruined Watchtower Courtyard", secondTurn.Campaign.LocationName);
    }
}