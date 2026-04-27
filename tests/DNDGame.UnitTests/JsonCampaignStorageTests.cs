using DNDGame.Core.Models;
using DNDGame.Core.Services;
using DNDGame.Infrastructure.Persistence;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class JsonCampaignStorageTests
{
    [TestMethod]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsCampaignState()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"dndgame-tests-{Guid.NewGuid():N}");

        try
        {
            ICampaignStorage storage = new JsonCampaignStorage(saveDirectory);
            var campaign = NewCampaignFactory.Create("roundtrip", "Tarin", CharacterClass.Ranger);

            await storage.SaveAsync(campaign);
            var loadedCampaign = await storage.LoadAsync("roundtrip");

            Assert.IsNotNull(loadedCampaign);
            Assert.AreEqual(campaign.SaveSlot, loadedCampaign.SaveSlot);
            Assert.AreEqual(campaign.Hero.Name, loadedCampaign.Hero.Name);
            Assert.AreEqual(campaign.Hero.Class, loadedCampaign.Hero.Class);
            Assert.AreEqual(campaign.ActiveQuest.QuestId, loadedCampaign.ActiveQuest.QuestId);
            Assert.AreEqual(campaign.Journal.Count, loadedCampaign.Journal.Count);
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
    }
}