namespace DNDGame.Core.Models;

public sealed record JournalEntry(
    DateTimeOffset TimestampUtc,
    string Category,
    string Text);