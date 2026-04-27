using DNDGame.Core.Models;

namespace DNDGame.Core.Content;

public static class StarterGameContent
{
    private static readonly IReadOnlyDictionary<CharacterClass, StarterClassDefinition> ClassDefinitions =
        new Dictionary<CharacterClass, StarterClassDefinition>
        {
            [CharacterClass.Fighter] = new(
                CharacterClass.Fighter,
                "Fighter",
                "A disciplined frontline warrior who wins by endurance and pressure.",
                24,
                6,
                3,
                "Shield Bash",
                8),
            [CharacterClass.Ranger] = new(
                CharacterClass.Ranger,
                "Ranger",
                "A mobile skirmisher who strikes cleanly before the enemy can settle.",
                20,
                7,
                2,
                "Aimed Shot",
                9),
            [CharacterClass.Mage] = new(
                CharacterClass.Mage,
                "Mage",
                "A fragile caster who trades staying power for decisive arcane bursts.",
                16,
                5,
                1,
                "Arc Bolt",
                10),
        };

    public static StarterQuestDefinition OpeningQuest { get; } = new(
        "watchtower-road",
        "The Watchtower Road",
        "Travel east from Northgate Outpost and investigate the ruined watchtower.",
        "Defeat the goblin scout holding the watchtower approach.",
        "Return to Captain Elira at Northgate Outpost.",
        "Northreach Frontier",
        "Northgate Outpost",
        "Old Watchtower Approach",
        "Ruined Watchtower Courtyard");

    public static EncounterDefinition OpeningEncounter { get; } = new(
        "watchtower-goblin-scout",
        "Watchtower Approach",
        "A lone goblin scout bars the cracked road leading into the ruined watchtower.",
        "goblin-scout",
        "Goblin Scout",
        18,
        4,
        1);

    public static IReadOnlyCollection<StarterClassDefinition> Classes => ClassDefinitions.Values.ToArray();

    public static StarterClassDefinition GetClassDefinition(CharacterClass characterClass)
    {
        if (!ClassDefinitions.TryGetValue(characterClass, out var definition))
        {
            throw new ArgumentOutOfRangeException(nameof(characterClass), characterClass, "Unsupported class.");
        }

        return definition;
    }
}