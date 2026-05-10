using DNDGame.Core.Content;
using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class CampaignProgressionServiceTests
{
    [TestMethod]
    public void Advance_FromAccepted_MovesQuestToWatchtowerAndCreatesEncounter()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);

        var result = CampaignProgressionService.Advance(campaign);

        Assert.AreEqual(QuestStage.AtWatchtowerApproach, result.Campaign.ActiveQuest.Stage);
        Assert.IsNotNull(result.Campaign.CurrentEncounter);
        Assert.AreEqual("Goblin Scout", result.Campaign.CurrentEncounter.Enemy.Name);
        Assert.AreEqual("Old Watchtower Approach", result.Campaign.LocationName);
    }

    [TestMethod]
    public void Advance_FromCourtyardCleared_MovesQuestToSummitAndCreatesBossEncounter()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage) with
        {
            ActiveQuest = new QuestProgress(
                StarterGameContent.OpeningQuest.QuestId,
                StarterGameContent.OpeningQuest.Title,
                StarterGameContent.OpeningQuest.BossObjective,
                QuestStage.WatchtowerCourtyardCleared,
                false),
            LocationName = StarterGameContent.OpeningQuest.BossLocation,
        };

        var result = CampaignProgressionService.Advance(campaign);

        Assert.AreEqual(QuestStage.InWatchtowerSummit, result.Campaign.ActiveQuest.Stage);
        Assert.IsNotNull(result.Campaign.CurrentEncounter);
        Assert.AreEqual("Raider Captain Vark", result.Campaign.CurrentEncounter.Enemy.Name);
        Assert.AreEqual("Watchtower Summit", result.Campaign.LocationName);
    }
}