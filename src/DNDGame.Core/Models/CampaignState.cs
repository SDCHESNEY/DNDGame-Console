namespace DNDGame.Core.Models;

public sealed record CampaignState(
    int SchemaVersion,
    string SaveSlot,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc,
    string RegionName,
    string LocationName,
    Hero Hero,
    QuestProgress ActiveQuest,
    IReadOnlyList<JournalEntry> Journal);