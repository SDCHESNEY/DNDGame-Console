using System.Text.Json;

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
            return GameAppSettings.Default;
        }

        var json = File.ReadAllText(resolvedFilePath);
        var settings = JsonSerializer.Deserialize<GameAppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        return settings ?? GameAppSettings.Default;
    }
}