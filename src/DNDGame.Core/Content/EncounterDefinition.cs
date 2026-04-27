namespace DNDGame.Core.Content;

public sealed record EncounterDefinition(
    string EncounterId,
    string Title,
    string Description,
    string EnemyId,
    string EnemyName,
    int EnemyMaxHealth,
    int EnemyAttackPower,
    int EnemyArmor);