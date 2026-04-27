namespace DNDGame.Core.Models;

public sealed record Hero(
    string Name,
    CharacterClass Class,
    int Level,
    int MaxHealth,
    int CurrentHealth);