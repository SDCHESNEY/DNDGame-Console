namespace DNDGame.Core.Content;

public sealed record AbilityDefinition(
    string AbilityId,
    string Name,
    string Description,
    int PowerBonus,
    bool IsDefensive);