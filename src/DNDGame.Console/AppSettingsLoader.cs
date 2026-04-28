using System.Text.Json;
using DNDGame.Core.Models;

namespace DNDGame.Console;

public static class AppSettingsLoader
{
    private static readonly HashSet<string> AllowedNarrativeVerbosityValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "concise",
        "balanced",
        "rich",
    };

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

        return Validate(new GameAppSettings(saveDirectory, enableLocalLlmNarration, localLlm));
    }

    private static GameAppSettings Validate(GameAppSettings settings)
    {
        if (!settings.EnableLocalLlmNarration)
        {
            return settings;
        }

        var endpointUrl = settings.LocalLlm.EndpointUrl.Trim();
        var modelName = settings.LocalLlm.ModelName.Trim();
        var narrativeVerbosity = settings.LocalLlm.NarrativeVerbosity.Trim();

        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            throw new ConfigurationValidationException("Invalid local LLM configuration: 'localLlm.endpointUrl' is required when local narration is enabled.");
        }

        if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out var endpointUri)
            || (endpointUri.Scheme != Uri.UriSchemeHttp && endpointUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ConfigurationValidationException("Invalid local LLM configuration: 'localLlm.endpointUrl' must be an absolute HTTP or HTTPS URL when local narration is enabled.");
        }

        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ConfigurationValidationException("Invalid local LLM configuration: 'localLlm.modelName' is required when local narration is enabled.");
        }

        if (string.IsNullOrWhiteSpace(narrativeVerbosity)
            || !AllowedNarrativeVerbosityValues.Contains(narrativeVerbosity))
        {
            throw new ConfigurationValidationException("Invalid local LLM configuration: 'localLlm.narrativeVerbosity' must be one of concise, balanced, or rich.");
        }

        var normalizedLocalLlm = settings.LocalLlm with
        {
            EndpointUrl = endpointUrl.TrimEnd('/'),
            ModelName = modelName,
            NarrativeVerbosity = narrativeVerbosity.ToLowerInvariant(),
        };

        return settings with { LocalLlm = normalizedLocalLlm };
    }
}