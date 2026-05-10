using System.Text.Json;
using System.Text.Json.Serialization;
using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Infrastructure.Persistence;

public sealed class JsonCampaignStorage : ICampaignStorage
{
    private const int CurrentSchemaVersion = 2;
    private const int MaxPersistedJournalEntries = 8;

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

        var normalizedCampaign = NormalizeForPersistence(campaign);

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

        try
        {
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return MigrateToCurrent(document.RootElement, saveSlot);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Save migration failed for slot '{saveSlot}'. The save file could not be read.", exception);
        }
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

    public async Task<IReadOnlyList<SaveSlotMetadata>> ListSaveSlotMetadataAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(SaveDirectory))
        {
            return Array.Empty<SaveSlotMetadata>();
        }

        var metadata = new List<SaveSlotMetadata>();
        foreach (var filePath in Directory.EnumerateFiles(SaveDirectory, "*.json", SearchOption.TopDirectoryOnly)
                     .OrderBy(static path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var stream = File.OpenRead(filePath);

            try
            {
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var campaign = MigrateToCurrent(document.RootElement, Path.GetFileNameWithoutExtension(filePath) ?? "unknown");
                if (campaign is null)
                {
                    metadata.Add(CreateUnavailableMetadata(filePath));
                    continue;
                }

                metadata.Add(new SaveSlotMetadata(
                    campaign.SaveSlot,
                    campaign.Hero.Name,
                    campaign.Hero.Class,
                    campaign.ActiveQuest.Stage,
                    $"Level {campaign.Hero.Level} {campaign.Hero.Class} at {campaign.LocationName}",
                    campaign.UpdatedUtc));
            }
            catch (JsonException)
            {
                metadata.Add(CreateUnavailableMetadata(filePath));
            }
        }

        return metadata;
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

    private static SaveSlotMetadata CreateUnavailableMetadata(string filePath)
    {
        return new SaveSlotMetadata(
            Path.GetFileNameWithoutExtension(filePath) ?? "unknown",
            "Unavailable",
            null,
            null,
            "Save metadata unavailable",
            File.GetLastWriteTimeUtc(filePath));
    }

    private static CampaignState NormalizeForPersistence(CampaignState campaign)
    {
        var updatedUtc = DateTimeOffset.UtcNow;
        var orderedJournal = campaign.Journal.OrderBy(entry => entry.TimestampUtc).ToArray();
        var existingSnapshots = campaign.RecapSnapshots.OrderBy(snapshot => snapshot.CreatedUtc).ToList();

        if (orderedJournal.Length <= MaxPersistedJournalEntries)
        {
            return campaign with
            {
                SchemaVersion = CurrentSchemaVersion,
                UpdatedUtc = updatedUtc,
                Journal = orderedJournal,
                RecapSnapshots = existingSnapshots,
            };
        }

        var compactedCount = orderedJournal.Length - MaxPersistedJournalEntries;
        var entriesToCompact = orderedJournal.Take(compactedCount).ToArray();
        var recentEntries = orderedJournal.Skip(compactedCount).ToArray();
        var snapshotSummary = BuildSnapshotSummary(entriesToCompact);

        existingSnapshots.Add(new RecapSnapshot(entriesToCompact[^1].TimestampUtc, snapshotSummary));

        return campaign with
        {
            SchemaVersion = CurrentSchemaVersion,
            UpdatedUtc = updatedUtc,
            Journal = recentEntries,
            RecapSnapshots = existingSnapshots,
        };
    }

    private static string BuildSnapshotSummary(IReadOnlyList<JournalEntry> entries)
    {
        var milestoneCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "quest",
            "combat",
            "loot",
            "item",
            "exploration",
            "travel",
            "prologue",
        };

        var milestoneTexts = entries
            .Where(entry => milestoneCategories.Contains(entry.Category))
            .Select(entry => entry.Text.Trim())
            .Where(static text => !string.IsNullOrWhiteSpace(text))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        if (milestoneTexts.Length == 0)
        {
            milestoneTexts = entries
                .Select(entry => entry.Text.Trim())
                .Where(static text => !string.IsNullOrWhiteSpace(text))
                .Distinct(StringComparer.Ordinal)
                .Take(2)
                .ToArray();
        }

        return $"Earlier campaign recap: {string.Join(" | ", milestoneTexts)}";
    }

    private static CampaignState MigrateToCurrent(JsonElement rootElement, string saveSlot)
    {
        var schemaVersion = TryReadSchemaVersion(rootElement);
        return schemaVersion switch
        {
            <= 1 => MigrateV1ToCurrent(rootElement, saveSlot),
            CurrentSchemaVersion => DeserializeCurrent(rootElement, saveSlot),
            _ => throw new InvalidOperationException($"Save migration failed for slot '{saveSlot}'. Unsupported schema version '{schemaVersion}'."),
        };
    }

    private static CampaignState DeserializeCurrent(JsonElement rootElement, string saveSlot)
    {
        var campaign = rootElement.Deserialize<CampaignState>(SerializerOptions);
        if (campaign is null)
        {
            throw new InvalidOperationException($"Save migration failed for slot '{saveSlot}'. The current save payload is incomplete.");
        }

        return campaign with
        {
            SchemaVersion = CurrentSchemaVersion,
            RecapSnapshots = campaign.RecapSnapshots ?? Array.Empty<RecapSnapshot>(),
        };
    }

    private static CampaignState MigrateV1ToCurrent(JsonElement rootElement, string saveSlot)
    {
        var legacyCampaign = rootElement.Deserialize<CampaignStateV1>(SerializerOptions);
        if (legacyCampaign is null)
        {
            throw new InvalidOperationException($"Save migration failed for slot '{saveSlot}'. The legacy save payload is incomplete.");
        }

        return new CampaignState(
            CurrentSchemaVersion,
            legacyCampaign.SaveSlot,
            legacyCampaign.CreatedUtc,
            legacyCampaign.UpdatedUtc,
            legacyCampaign.RegionName,
            legacyCampaign.LocationName,
            legacyCampaign.Hero,
            legacyCampaign.ActiveQuest,
            legacyCampaign.Journal,
            Array.Empty<RecapSnapshot>(),
            legacyCampaign.Inventory,
            legacyCampaign.CurrentEncounter is null
                ? null
                : new EncounterState(
                    legacyCampaign.CurrentEncounter.EncounterId,
                    legacyCampaign.CurrentEncounter.Title,
                    legacyCampaign.CurrentEncounter.Description,
                    legacyCampaign.CurrentEncounter.RoundNumber,
                    legacyCampaign.CurrentEncounter.Enemy,
                    legacyCampaign.CurrentEncounter.IsCompleted));
    }

    private static int TryReadSchemaVersion(JsonElement rootElement)
    {
        return rootElement.TryGetProperty("SchemaVersion", out var property)
               && property.ValueKind == JsonValueKind.Number
               && property.TryGetInt32(out var schemaVersion)
            ? schemaVersion
            : 1;
    }

    private sealed record CampaignStateV1(
        int SchemaVersion,
        string SaveSlot,
        DateTimeOffset CreatedUtc,
        DateTimeOffset UpdatedUtc,
        string RegionName,
        string LocationName,
        Hero Hero,
        QuestProgress ActiveQuest,
        IReadOnlyList<JournalEntry> Journal,
        IReadOnlyList<InventoryItem> Inventory,
        EncounterStateV1? CurrentEncounter);

    private sealed record EncounterStateV1(
        string EncounterId,
        string Title,
        string Description,
        int RoundNumber,
        EnemyState Enemy,
        bool IsCompleted);
}