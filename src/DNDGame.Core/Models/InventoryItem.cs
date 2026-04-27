namespace DNDGame.Core.Models;

public sealed record InventoryItem(
    string ItemId,
    string Name,
    string Description,
    string Category,
    int Quantity);