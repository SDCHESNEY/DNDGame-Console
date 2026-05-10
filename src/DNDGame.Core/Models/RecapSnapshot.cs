namespace DNDGame.Core.Models;

public sealed record RecapSnapshot(
    DateTimeOffset CreatedUtc,
    string Summary);