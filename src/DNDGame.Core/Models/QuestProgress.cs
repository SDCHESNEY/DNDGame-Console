namespace DNDGame.Core.Models;

public sealed record QuestProgress(
    string QuestId,
    string Title,
    string Objective,
    QuestStage Stage,
    bool IsCompleted);