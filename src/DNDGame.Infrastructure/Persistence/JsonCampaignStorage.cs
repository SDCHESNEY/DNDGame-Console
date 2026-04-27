using System.Text.Json;
using System.Text.Json.Serialization;
using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Infrastructure.Persistence;

public sealed class JsonCampaignStorage : ICampaignStorage
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public JsonCampaignStorage(string? saveDirectory = null)
    {
        SaveDirectory = string.IsNullOrWhiteSpace(saveDirectory)
            ? DefaultSaveDirectoryProvider.GetSaveDirectory()
            : saveDirectory;
    }

    public string SaveDirectory { get; }

    public async Task SaveAsync(CampaignState campaign, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        Directory.CreateDirectory(SaveDirectory);

        var normalizedCampaign = campaign with
        {
            UpdatedUtc = DateTimeOffset.UtcNow,
        };

        var filePath = GetFilePath(normalizedCampaign.SaveSlot);
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, normalizedCampaign, SerializerOptions, cancellationToken);
    }

    public async Task<CampaignState?> LoadAsync(string saveSlot, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(saveSlot);
        if (!File.Exists(filePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<CampaignState>(stream, SerializerOptions, cancellationToken);
    }

    public Task<IReadOnlyList<string>> ListSaveSlotsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(SaveDirectory))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var saveSlots = Directory.EnumerateFiles(SaveDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(static fileName => !string.IsNullOrWhiteSpace(fileName))
            .Cast<string>()
            .OrderBy(static fileName => fileName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(saveSlots);
    }

    private string GetFilePath(string saveSlot)
    {
        if (string.IsNullOrWhiteSpace(saveSlot))
        {
            throw new ArgumentException("Save slot is required.", nameof(saveSlot));
        }

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safeFileName = string.Concat(saveSlot.Trim().Select(ch => invalidCharacters.Contains(ch) ? '-' : ch));
        return Path.Combine(SaveDirectory, $"{safeFileName}.json");
    }
}