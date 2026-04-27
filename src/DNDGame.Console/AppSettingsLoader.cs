using System.Text.Json;
using DNDGame.Core.Models;

namespace DNDGame.Console;

public static class AppSettingsLoader
{
    public static GameAppSettings Load(string? filePath = null)
    {
        var resolvedFilePath = string.IsNullOrWhiteSpace(filePath)
            ? Path.Combine(AppContext.BaseDirectory, "appsettings.json")
            : filePath;

        if (!File.Exists(resolvedFilePath))
        {
            return ApplyEnvironmentOverrides(GameAppSettings.Default);
        }

        var json = File.ReadAllText(resolvedFilePath);
        var settings = JsonSerializer.Deserialize<GameAppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        return ApplyEnvironmentOverrides(settings ?? GameAppSettings.Default);
    }

    private static GameAppSettings ApplyEnvironmentOverrides(GameAppSettings settings)
    {
        var saveDirectory = Environment.GetEnvironmentVariable("DNDGAME_SAVE_DIRECTORY") ?? settings.SaveDirectory;
        var enableLocalLlmNarration = bool.TryParse(Environment.GetEnvironmentVariable("DNDGAME_ENABLE_LOCAL_LLM_NARRATION"), out var enabled)
            ? enabled
            : settings.EnableLocalLlmNarration;

        var localLlm = settings.LocalLlm with
        {
            EndpointUrl = Environment.GetEnvironmentVariable("DNDGAME_LLM_ENDPOINT_URL") ?? settings.LocalLlm.EndpointUrl,
            ModelName = Environment.GetEnvironmentVariable("DNDGAME_LLM_MODEL") ?? settings.LocalLlm.ModelName,
            NarrativeVerbosity = Environment.GetEnvironmentVariable("DNDGAME_LLM_VERBOSITY") ?? settings.LocalLlm.NarrativeVerbosity,
        };

        return new GameAppSettings(saveDirectory, enableLocalLlmNarration, localLlm);
    }
}