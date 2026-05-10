using DNDGame.Core.Content;
using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public static class NewCampaignFactory
{
    public static CampaignState Create(string saveSlot, string heroName, CharacterClass characterClass)
    {
        if (string.IsNullOrWhiteSpace(saveSlot))
        {
            throw new ArgumentException("Save slot is required.", nameof(saveSlot));
        }

        if (string.IsNullOrWhiteSpace(heroName))
        {
            throw new ArgumentException("Hero name is required.", nameof(heroName));
        }

        var timestamp = DateTimeOffset.UtcNow;
        var hero = CreateHero(heroName.Trim(), characterClass);
        var questDefinition = StarterGameContent.OpeningQuest;
        var quest = new QuestProgress(
            questDefinition.QuestId,
            questDefinition.Title,
            questDefinition.InitialObjective,
            QuestStage.Accepted,
            false);
        var classDefinition = StarterGameContent.GetClassDefinition(characterClass);

        var journal = new List<JournalEntry>
        {
            new(timestamp, "prologue", "You arrived at Northgate Outpost on the edge of the Gloamwood."),
            new(timestamp, "quest", "Captain Elira asked you to secure the ruined watchtower before nightfall."),
        };
        var inventory = classDefinition.StartingLoot
            .Select(static loot => new InventoryItem(loot.LootId, loot.Name, loot.Description, loot.Category, loot.Quantity))
            .ToArray();

        return new CampaignState(
            2,
            saveSlot.Trim(),
            timestamp,
            timestamp,
            questDefinition.RegionName,
            questDefinition.OutpostLocation,
            hero,
            quest,
            journal,
            Array.Empty<RecapSnapshot>(),
            inventory,
            null);
    }

    private static Hero CreateHero(string heroName, CharacterClass characterClass)
    {
        var definition = StarterGameContent.GetClassDefinition(characterClass);
        return new Hero(heroName, characterClass, 1, definition.MaxHealth, definition.MaxHealth);
    }
}