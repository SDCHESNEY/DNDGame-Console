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
        var quest = new QuestProgress(
            "watchtower-road",
            "The Watchtower Road",
            "Travel east from Northgate Outpost and clear the ruined watchtower.",
            false);

        var journal = new List<JournalEntry>
        {
            new(timestamp, "prologue", "You arrived at Northgate Outpost on the edge of the Gloamwood."),
            new(timestamp, "quest", "Captain Elira asked you to secure the ruined watchtower before nightfall."),
        };

        return new CampaignState(
            1,
            saveSlot.Trim(),
            timestamp,
            timestamp,
            "Northreach Frontier",
            "Northgate Outpost",
            hero,
            quest,
            journal);
    }

    private static Hero CreateHero(string heroName, CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterClass.Fighter => new Hero(heroName, characterClass, 1, 24, 24),
            CharacterClass.Ranger => new Hero(heroName, characterClass, 1, 20, 20),
            CharacterClass.Mage => new Hero(heroName, characterClass, 1, 16, 16),
            _ => throw new ArgumentOutOfRangeException(nameof(characterClass), characterClass, "Unsupported class."),
        };
    }
}