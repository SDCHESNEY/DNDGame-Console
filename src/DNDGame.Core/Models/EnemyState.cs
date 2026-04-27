namespace DNDGame.Core.Models;

public sealed record EnemyState(
    string EnemyId,
    string Name,
    int MaxHealth,
    int CurrentHealth,
    int AttackPower,
    int Armor);