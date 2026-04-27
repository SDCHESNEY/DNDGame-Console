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

        Assert.AreEqual(QuestStage.AtWatchtower, result.Campaign.ActiveQuest.Stage);
        Assert.IsNotNull(result.Campaign.CurrentEncounter);
        Assert.AreEqual("Goblin Scout", result.Campaign.CurrentEncounter.Enemy.Name);
        Assert.AreEqual("Old Watchtower Approach", result.Campaign.LocationName);
    }
}