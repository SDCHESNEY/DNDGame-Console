using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class NewCampaignFactoryTests
{
    [TestMethod]
    public void Create_ValidInputs_ReturnsExpectedStartingCampaign()
    {
        var campaign = NewCampaignFactory.Create("slot-one", "Mira", CharacterClass.Mage);

        Assert.AreEqual("slot-one", campaign.SaveSlot);
        Assert.AreEqual("Mira", campaign.Hero.Name);
        Assert.AreEqual(CharacterClass.Mage, campaign.Hero.Class);
        Assert.AreEqual("Northgate Outpost", campaign.LocationName);
        Assert.AreEqual("The Watchtower Road", campaign.ActiveQuest.Title);
        Assert.AreEqual(QuestStage.Accepted, campaign.ActiveQuest.Stage);
        Assert.AreEqual(2, campaign.Journal.Count);
        Assert.AreEqual(1, campaign.Inventory.Count);
        Assert.AreEqual("Minor Potion", campaign.Inventory[0].Name);
        Assert.IsNull(campaign.CurrentEncounter);
    }
}