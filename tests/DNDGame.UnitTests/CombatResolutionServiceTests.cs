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

        Assert.AreEqual(QuestStage.WatchtowerApproachCleared, secondTurn.Campaign.ActiveQuest.Stage);
        Assert.IsNull(secondTurn.Campaign.CurrentEncounter);
        Assert.AreEqual("Ruined Watchtower Courtyard", secondTurn.Campaign.LocationName);
        Assert.IsTrue(secondTurn.Campaign.Inventory.Any(item => item.ItemId == "scout-satchel"));
    }

    [TestMethod]
    public void ResolveTurn_WhenDefending_AppliesGuardedAndReducesIncomingDamage()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        var result = CombatResolutionService.ResolveTurn(campaign, CombatAction.Defend);

        Assert.AreEqual(23, result.Campaign.Hero.CurrentHealth);
        Assert.IsNotNull(result.Campaign.CurrentEncounter);
        Assert.AreEqual(1, result.Campaign.CurrentEncounter.HeroGuardedRounds);
        StringAssert.Contains(result.Summary, "guard reduces incoming damage by 2");
    }

    [TestMethod]
    public void ResolveTurn_WhenUsingSpecial_AppliesSunderedAndNextAttackDealsMoreDamage()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign;

        var firstTurn = CombatResolutionService.ResolveTurn(campaign, CombatAction.Special);

        Assert.IsNotNull(firstTurn.Campaign.CurrentEncounter);
        Assert.AreEqual(1, firstTurn.Campaign.CurrentEncounter.EnemySunderedRounds);
        Assert.AreEqual(11, firstTurn.Campaign.CurrentEncounter.Enemy.CurrentHealth);
        StringAssert.Contains(firstTurn.Summary, "enemy armor is sundered", StringComparison.OrdinalIgnoreCase);

        var secondTurn = CombatResolutionService.ResolveTurn(firstTurn.Campaign, CombatAction.Attack);

        Assert.IsNotNull(secondTurn.Campaign.CurrentEncounter);
        Assert.AreEqual(5, secondTurn.Campaign.CurrentEncounter.Enemy.CurrentHealth);
    }

    [TestMethod]
    public void UseCombatItem_WithMinorPotion_ConsumesItemAndHealsHero()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign with
        {
            Hero = campaign.Hero with { CurrentHealth = 12 },
        };

        var result = CombatResolutionService.UseCombatItem(campaign, "minor-potion");

        Assert.AreEqual(19, result.Campaign.Hero.CurrentHealth);
        Assert.AreEqual(0, result.Campaign.Inventory.Count(item => item.ItemId == "minor-potion"));
        Assert.IsNotNull(result.Campaign.CurrentEncounter);
        StringAssert.Contains(result.Summary, "Minor Potion");
    }

    [TestMethod]
    public void UseCombatItem_WithQuestItem_ReturnsValidationError()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Fighter);
        campaign = CampaignProgressionService.Advance(campaign).Campaign with
        {
            Inventory = campaign.Inventory.Concat(
                [new InventoryItem("scout-satchel", "Scout's Satchel", "Quest evidence.", "quest", 1)]).ToArray(),
        };

        var result = CombatResolutionService.UseCombatItem(campaign, "scout-satchel");

        Assert.AreEqual(campaign, result.Campaign);
        StringAssert.Contains(result.Summary, "cannot be used during combat");
    }
}