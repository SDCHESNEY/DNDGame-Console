namespace DNDGame.Core.Models;

public sealed record EncounterState(
    string EncounterId,
    string Title,
    string Description,
    int RoundNumber,
    EnemyState Enemy,
    bool IsCompleted);