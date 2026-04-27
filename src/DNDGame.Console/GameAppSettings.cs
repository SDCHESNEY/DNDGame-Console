using DNDGame.Core.Models;

namespace DNDGame.Console;

public sealed record GameAppSettings(
    string? SaveDirectory,
    bool EnableLocalLlmNarration,
    LocalLlmSettings LocalLlm)
{
    public static GameAppSettings Default { get; } = new(null, false, LocalLlmSettings.Default);
}