namespace DNDGame.Core.Models;

public sealed record LocalLlmSettings(
    string EndpointUrl,
    string ModelName,
    string NarrativeVerbosity)
{
    public static LocalLlmSettings Default { get; } = new(
        "http://localhost:11434",
        "local-dm",
        "balanced");
}