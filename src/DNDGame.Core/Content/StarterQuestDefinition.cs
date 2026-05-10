namespace DNDGame.Core.Content;

public sealed record StarterQuestDefinition(
    string QuestId,
    string Title,
    string InitialObjective,
    string WatchtowerApproachObjective,
    string CourtyardObjective,
    string BossObjective,
    string ReturnObjective,
    string RegionName,
    string OutpostLocation,
    string WatchtowerApproachLocation,
    string CourtyardLocation,
    string BossLocation,
    string ClearedWatchtowerLocation);