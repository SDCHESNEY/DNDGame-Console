using DNDGame.Core.Content;
using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public static class CampaignProgressionService
{
    public static CampaignUpdateResult Advance(CampaignState campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        var now = DateTimeOffset.UtcNow;
        var quest = StarterGameContent.OpeningQuest;

        return campaign.ActiveQuest.Stage switch
        {
            QuestStage.Accepted => MoveToWatchtowerApproach(campaign, now, quest),
            QuestStage.AtWatchtowerApproach when campaign.CurrentEncounter is not null => new CampaignUpdateResult(
                campaign,
                "The goblin scout is still guarding the watchtower road. You must win the encounter before pressing deeper.",
                false),
            QuestStage.WatchtowerApproachCleared => MoveToCourtyard(campaign, now, quest),
            QuestStage.InWatchtowerCourtyard when campaign.CurrentEncounter is not null => new CampaignUpdateResult(
                campaign,
                "The hobgoblin raider still controls the courtyard. Defeat it before you try to claim the tower.",
                false),
            QuestStage.WatchtowerCourtyardCleared => MoveToSummit(campaign, now, quest),
            QuestStage.InWatchtowerSummit when campaign.CurrentEncounter is not null => new CampaignUpdateResult(
                campaign,
                "Raider Captain Vark still commands the summit. Defeat the boss before the watchtower can be declared secure.",
                false),
            QuestStage.WatchtowerCleared => ReturnToOutpost(campaign, now, quest),
            QuestStage.ReturnedToCaptain => new CampaignUpdateResult(
                campaign,
                "Captain Elira has already been briefed. The opening quest is complete.",
                false),
            _ => new CampaignUpdateResult(campaign, "Nothing changes.", false),
        };
    }

    private static CampaignUpdateResult MoveToWatchtowerApproach(CampaignState campaign, DateTimeOffset now, StarterQuestDefinition quest)
    {
        var encounterDefinition = StarterGameContent.OpeningEncounter;
        var encounter = CreateEncounterState(encounterDefinition);

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            LocationName = quest.WatchtowerApproachLocation,
            ActiveQuest = campaign.ActiveQuest with
            {
                Objective = quest.WatchtowerApproachObjective,
                Stage = QuestStage.AtWatchtowerApproach,
            },
            CurrentEncounter = encounter,
            Journal = AddJournal(campaign, now, "travel", "You followed the eastern road and reached the ruined watchtower approach."),
        };

        return new CampaignUpdateResult(updatedCampaign, encounterDefinition.Description);
    }

    private static CampaignUpdateResult MoveToCourtyard(CampaignState campaign, DateTimeOffset now, StarterQuestDefinition quest)
    {
        var encounterDefinition = StarterGameContent.CourtyardEncounter;
        var encounter = CreateEncounterState(encounterDefinition);

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            LocationName = quest.CourtyardLocation,
            ActiveQuest = campaign.ActiveQuest with
            {
                Objective = quest.CourtyardObjective,
                Stage = QuestStage.InWatchtowerCourtyard,
            },
            CurrentEncounter = encounter,
            Journal = AddJournal(campaign, now, "exploration", "You crossed the broken gate and stepped into the watchtower courtyard where a heavier threat waited."),
        };

        return new CampaignUpdateResult(updatedCampaign, encounterDefinition.Description);
    }

    private static CampaignUpdateResult MoveToSummit(CampaignState campaign, DateTimeOffset now, StarterQuestDefinition quest)
    {
        var encounterDefinition = StarterGameContent.BossEncounter;
        var encounter = CreateEncounterState(encounterDefinition);

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            LocationName = quest.BossLocation,
            ActiveQuest = campaign.ActiveQuest with
            {
                Objective = quest.BossObjective,
                Stage = QuestStage.InWatchtowerSummit,
            },
            CurrentEncounter = encounter,
            Journal = AddJournal(campaign, now, "exploration", "You climbed the broken stairs to the summit where Raider Captain Vark held the watchtower's last stand."),
        };

        return new CampaignUpdateResult(updatedCampaign, encounterDefinition.Description);
    }

    private static CampaignUpdateResult ReturnToOutpost(CampaignState campaign, DateTimeOffset now, StarterQuestDefinition quest)
    {
        var leveledHero = campaign.Hero with
        {
            Level = campaign.Hero.Level + 1,
            CurrentHealth = campaign.Hero.MaxHealth,
        };

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            LocationName = quest.OutpostLocation,
            Hero = leveledHero,
            ActiveQuest = campaign.ActiveQuest with
            {
                Objective = "Quest complete.",
                Stage = QuestStage.ReturnedToCaptain,
                IsCompleted = true,
            },
            Inventory = AddLoot(campaign.Inventory, StarterGameContent.QuestCompletionReward),
            Journal = AddJournal(campaign, now, "quest", "You returned to Captain Elira, reported the cleared watchtower, and earned a hard-won level."),
        };

        return new CampaignUpdateResult(updatedCampaign, "You returned to Northgate Outpost, reported your success, and grew stronger from the first victory.");
    }

    private static IReadOnlyList<JournalEntry> AddJournal(CampaignState campaign, DateTimeOffset timestamp, string category, string text)
    {
        return campaign.Journal.Concat(new[] { new JournalEntry(timestamp, category, text) }).ToArray();
    }

    private static EncounterState CreateEncounterState(EncounterDefinition encounterDefinition)
    {
        return new EncounterState(
            encounterDefinition.EncounterId,
            encounterDefinition.Title,
            encounterDefinition.Description,
            1,
            new EnemyState(
                encounterDefinition.EnemyId,
                encounterDefinition.EnemyName,
                encounterDefinition.EnemyMaxHealth,
                encounterDefinition.EnemyMaxHealth,
                encounterDefinition.EnemyAttackPower,
                encounterDefinition.EnemyArmor),
            false);
    }

    private static IReadOnlyList<InventoryItem> AddLoot(IReadOnlyList<InventoryItem> inventory, LootDefinition loot)
    {
        var existingItem = inventory.FirstOrDefault(item => item.ItemId == loot.LootId);
        if (existingItem is null)
        {
            return inventory.Concat(new[]
            {
                new InventoryItem(loot.LootId, loot.Name, loot.Description, loot.Category, loot.Quantity),
            }).ToArray();
        }

        return inventory.Select(item => item.ItemId == loot.LootId
                ? item with { Quantity = item.Quantity + loot.Quantity }
                : item)
            .ToArray();
    }
}