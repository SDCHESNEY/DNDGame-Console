namespace DNDGame.Core.Content;

public sealed record StarterQuestDefinition(
    string QuestId,
    string Title,
    string InitialObjective,
    string WatchtowerObjective,
    string ReturnObjective,
    string RegionName,
    string OutpostLocation,
    string WatchtowerLocation,
    string ClearedWatchtowerLocation);