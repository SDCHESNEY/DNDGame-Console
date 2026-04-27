using DNDGame.Core.Models;

namespace DNDGame.Core.Content;

public static class StarterGameContent
{
    private static readonly LootDefinition MinorPotion = new(
        "minor-potion",
        "Minor Potion",
        "A simple restorative draught carried for desperate moments.",
        "consumable",
        1);

    private static readonly LootDefinition ScoutSatchel = new(
        "scout-satchel",
        "Scout's Satchel",
        "A weathered satchel holding a rough map of the watchtower grounds.",
        "quest",
        1);

    private static readonly LootDefinition WatchtowerSigil = new(
        "watchtower-sigil",
        "Watchtower Sigil",
        "A bronze sigil proving the tower has been reclaimed from raiders.",
        "quest",
        1);

    private static readonly LootDefinition FrontierCharm = new(
        "frontier-charm",
        "Frontier Charm",
        "A small token from Captain Elira recognizing your first victory.",
        "reward",
        1);

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
                8,
                [
                    new AbilityDefinition("fighter-shield-bash", "Shield Bash", "A crushing close-range strike that staggers the enemy line.", 2, false),
                    new AbilityDefinition("fighter-guard-stance", "Guard Stance", "Sets the fighter's footing and reduces damage on the next exchange.", 2, true),
                ],
                [MinorPotion]),
            [CharacterClass.Ranger] = new(
                CharacterClass.Ranger,
                "Ranger",
                "A mobile skirmisher who strikes cleanly before the enemy can settle.",
                20,
                7,
                2,
                "Aimed Shot",
                9,
                [
                    new AbilityDefinition("ranger-aimed-shot", "Aimed Shot", "A deliberate attack that punishes exposed targets.", 2, false),
                    new AbilityDefinition("ranger-evasion", "Evasion", "A quick sidestep that blunts the next enemy response.", 2, true),
                ],
                [MinorPotion]),
            [CharacterClass.Mage] = new(
                CharacterClass.Mage,
                "Mage",
                "A fragile caster who trades staying power for decisive arcane bursts.",
                16,
                5,
                1,
                "Arc Bolt",
                10,
                [
                    new AbilityDefinition("mage-arc-bolt", "Arc Bolt", "A focused burst of force meant to break a target's momentum.", 5, false),
                    new AbilityDefinition("mage-warding-sigil", "Warding Sigil", "A brief ward that absorbs part of the next attack.", 2, true),
                ],
                [MinorPotion]),
        };

    public static StarterQuestDefinition OpeningQuest { get; } = new(
        "watchtower-road",
        "The Watchtower Road",
        "Travel east from Northgate Outpost and investigate the ruined watchtower.",
        "Defeat the goblin scout holding the watchtower approach.",
        "Break through the courtyard and defeat the hobgoblin raider holding the tower center.",
        "Return to Captain Elira at Northgate Outpost.",
        "Northreach Frontier",
        "Northgate Outpost",
        "Old Watchtower Approach",
        "Ruined Watchtower Courtyard",
        "Ruined Watchtower Courtyard");

    public static EncounterDefinition OpeningEncounter { get; } = new(
        "watchtower-goblin-scout",
        "Watchtower Approach",
        "A lone goblin scout bars the cracked road leading into the ruined watchtower.",
        "goblin-scout",
        "Goblin Scout",
        18,
        4,
        1,
        [ScoutSatchel]);

    public static EncounterDefinition CourtyardEncounter { get; } = new(
        "watchtower-hobgoblin-raider",
        "Watchtower Courtyard",
        "A hobgoblin raider rallies the last defenders around a cracked signal brazier in the courtyard.",
        "hobgoblin-raider",
        "Hobgoblin Raider",
        26,
        6,
        2,
        [WatchtowerSigil]);

    public static LootDefinition QuestCompletionReward => FrontierCharm;
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