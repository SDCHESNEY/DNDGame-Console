namespace DNDGame.Core.Models;

public sealed record SaveSlotMetadata(
    string SaveSlot,
    string HeroName,
    CharacterClass? HeroClass,
    QuestStage? QuestStage,
    string Summary,
    DateTimeOffset? LastPlayedUtc);