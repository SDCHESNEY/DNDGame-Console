using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class FullQuestLoopTests
{
    [TestMethod]
    public void FullQuestLoop_CompletesSecondEncounterAndReturnTrip()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;
        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;
        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;

        Assert.AreEqual(QuestStage.WatchtowerApproachCleared, campaign.ActiveQuest.Stage);

        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;
        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;
        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;
        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;
        campaign = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special).Campaign;

        Assert.AreEqual(QuestStage.WatchtowerCleared, campaign.ActiveQuest.Stage);
        Assert.IsTrue(campaign.Inventory.Any(item => item.ItemId == "watchtower-sigil"));

        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        Assert.AreEqual(QuestStage.ReturnedToCaptain, campaign.ActiveQuest.Stage);
        Assert.IsTrue(campaign.ActiveQuest.IsCompleted);
        Assert.AreEqual(2, campaign.Hero.Level);
        Assert.IsTrue(campaign.Inventory.Any(item => item.ItemId == "frontier-charm"));
    }
}