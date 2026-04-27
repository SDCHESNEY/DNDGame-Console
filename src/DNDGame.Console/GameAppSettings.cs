using DNDGame.Core.Models;

namespace DNDGame.Console;

public sealed record GameAppSettings(
    string? SaveDirectory,
    LocalLlmSettings LocalLlm)
{
    public static GameAppSettings Default { get; } = new(null, LocalLlmSettings.Default);
}