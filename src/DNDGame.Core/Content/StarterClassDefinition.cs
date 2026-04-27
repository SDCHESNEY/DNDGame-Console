using DNDGame.Core.Models;

namespace DNDGame.Core.Content;

public sealed record StarterClassDefinition(
    CharacterClass Class,
    string Title,
    string Description,
    int MaxHealth,
    int AttackPower,
    int Armor,
    string SpecialActionName,
    int SpecialAttackPower);