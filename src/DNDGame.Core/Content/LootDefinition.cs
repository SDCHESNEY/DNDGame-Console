namespace DNDGame.Core.Content;

public sealed record LootDefinition(
    string LootId,
    string Name,
    string Description,
    string Category,
    int Quantity);