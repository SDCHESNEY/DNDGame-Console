using DNDGame.Core.Models;
using DNDGame.Core.Services;
using DNDGame.Infrastructure.Persistence;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class JsonCampaignStorageTests
{
    [TestMethod]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsCampaignState()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"dndgame-tests-{Guid.NewGuid():N}");

        try
        {
            ICampaignStorage storage = new JsonCampaignStorage(saveDirectory);
            var campaign = NewCampaignFactory.Create("roundtrip", "Tarin", CharacterClass.Ranger);

            await storage.SaveAsync(campaign);
            var loadedCampaign = await storage.LoadAsync("roundtrip");

            Assert.IsNotNull(loadedCampaign);
            Assert.AreEqual(campaign.SaveSlot, loadedCampaign.SaveSlot);
            Assert.AreEqual(campaign.Hero.Name, loadedCampaign.Hero.Name);
            Assert.AreEqual(campaign.Hero.Class, loadedCampaign.Hero.Class);
            Assert.AreEqual(campaign.ActiveQuest.QuestId, loadedCampaign.ActiveQuest.QuestId);
            Assert.AreEqual(campaign.Journal.Count, loadedCampaign.Journal.Count);
            Assert.AreEqual(campaign.Inventory.Count, loadedCampaign.Inventory.Count);
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
    }

    [TestMethod]
    public async Task ListSaveSlotMetadataAsync_ReturnsDerivedMetadataAndGracefullyHandlesInvalidSave()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"dndgame-tests-{Guid.NewGuid():N}");

        try
        {
            ICampaignStorage storage = new JsonCampaignStorage(saveDirectory);
            var campaign = NewCampaignFactory.Create("metadata-slot", "Tarin", CharacterClass.Ranger);

            await storage.SaveAsync(campaign);
            await File.WriteAllTextAsync(Path.Combine(saveDirectory, "broken-slot.json"), "{ not json }");

            var metadata = await storage.ListSaveSlotMetadataAsync();

            Assert.AreEqual(2, metadata.Count);
            var validMetadata = metadata.Single(item => item.SaveSlot == "metadata-slot");
            Assert.AreEqual("Tarin", validMetadata.HeroName);
            Assert.AreEqual(CharacterClass.Ranger, validMetadata.HeroClass);
            Assert.AreEqual(QuestStage.Accepted, validMetadata.QuestStage);
            StringAssert.Contains(validMetadata.Summary, "Level 1 Ranger");
            Assert.IsNotNull(validMetadata.LastPlayedUtc);

            var brokenMetadata = metadata.Single(item => item.SaveSlot == "broken-slot");
            Assert.AreEqual("Unavailable", brokenMetadata.HeroName);
            Assert.AreEqual("Save metadata unavailable", brokenMetadata.Summary);
            Assert.IsNull(brokenMetadata.HeroClass);
            Assert.IsNull(brokenMetadata.QuestStage);
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
    }

    [TestMethod]
    public async Task LoadAsync_WithLegacySchemaVersion_MigratesToCurrentSchema()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"dndgame-tests-{Guid.NewGuid():N}");

        try
        {
            ICampaignStorage storage = new JsonCampaignStorage(saveDirectory);
            Directory.CreateDirectory(saveDirectory);

            var legacySaveJson = """
                {
                  "SchemaVersion": 1,
                  "SaveSlot": "legacy-slot",
                  "CreatedUtc": "2026-05-01T00:00:00+00:00",
                  "UpdatedUtc": "2026-05-01T00:00:00+00:00",
                  "RegionName": "Northreach Frontier",
                  "LocationName": "Northgate Outpost",
                  "Hero": {
                    "Name": "Mira",
                    "Class": "Mage",
                    "Level": 1,
                    "MaxHealth": 16,
                    "CurrentHealth": 16
                  },
                  "ActiveQuest": {
                    "QuestId": "watchtower-road",
                    "Title": "The Watchtower Road",
                    "Objective": "Travel east from Northgate Outpost and investigate the ruined watchtower.",
                    "Stage": "Accepted",
                    "IsCompleted": false
                  },
                  "Journal": [
                    {
                      "TimestampUtc": "2026-05-01T00:00:00+00:00",
                      "Category": "prologue",
                      "Text": "You arrived at Northgate Outpost on the edge of the Gloamwood."
                    }
                  ],
                  "Inventory": [
                    {
                      "ItemId": "minor-potion",
                      "Name": "Minor Potion",
                      "Description": "A simple restorative draught carried for desperate moments.",
                      "Category": "consumable",
                      "Quantity": 1
                    }
                  ],
                  "CurrentEncounter": null
                }
                """;

            await File.WriteAllTextAsync(Path.Combine(saveDirectory, "legacy-slot.json"), legacySaveJson);

            var loadedCampaign = await storage.LoadAsync("legacy-slot");

            Assert.IsNotNull(loadedCampaign);
            Assert.AreEqual(2, loadedCampaign.SchemaVersion);
            Assert.AreEqual("legacy-slot", loadedCampaign.SaveSlot);
            Assert.AreEqual(0, loadedCampaign.RecapSnapshots.Count);
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
    }

    [TestMethod]
    public async Task LoadAsync_WithUnsupportedSchemaVersion_ThrowsInvalidOperationException()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"dndgame-tests-{Guid.NewGuid():N}");

        try
        {
            ICampaignStorage storage = new JsonCampaignStorage(saveDirectory);
            Directory.CreateDirectory(saveDirectory);

            await File.WriteAllTextAsync(Path.Combine(saveDirectory, "future-slot.json"), "{\"SchemaVersion\":99}");

            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => storage.LoadAsync("future-slot"));

            StringAssert.Contains(exception.Message, "Unsupported schema version");
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
    }

    [TestMethod]
    public async Task SaveAsync_WithLongJournal_CompactsOlderEntriesIntoRecapSnapshots()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"dndgame-tests-{Guid.NewGuid():N}");

        try
        {
            ICampaignStorage storage = new JsonCampaignStorage(saveDirectory);
            var campaign = NewCampaignFactory.Create("compact-slot", "Tarin", CharacterClass.Ranger);
            var expandedJournal = Enumerable.Range(0, 14)
                .Select(index => new JournalEntry(
                    campaign.CreatedUtc.AddMinutes(index),
                    index % 2 == 0 ? "quest" : "combat",
                    $"Journal entry {index}"))
                .ToArray();

            await storage.SaveAsync(campaign with { Journal = expandedJournal });
            var loadedCampaign = await storage.LoadAsync("compact-slot");

            Assert.IsNotNull(loadedCampaign);
            Assert.AreEqual(8, loadedCampaign.Journal.Count);
            Assert.AreEqual(1, loadedCampaign.RecapSnapshots.Count);
            StringAssert.Contains(loadedCampaign.RecapSnapshots[0].Summary, "Earlier campaign recap:");
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
    }
}