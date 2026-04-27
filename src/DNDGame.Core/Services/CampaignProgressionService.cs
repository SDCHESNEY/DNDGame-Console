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
            QuestStage.Accepted => MoveToWatchtower(campaign, now, quest),
            QuestStage.AtWatchtower when campaign.CurrentEncounter is not null => new CampaignUpdateResult(
                campaign,
                "The goblin scout is still guarding the watchtower road. You must win the encounter before pressing deeper.",
                false),
            QuestStage.AtWatchtower => new CampaignUpdateResult(
                campaign,
                "The watchtower path is open. Look around, then continue when you are ready.",
                false),
            QuestStage.WatchtowerCleared => ReturnToOutpost(campaign, now, quest),
            QuestStage.ReturnedToCaptain => new CampaignUpdateResult(
                campaign,
                "Captain Elira has already been briefed. The opening quest is complete.",
                false),
            _ => new CampaignUpdateResult(campaign, "Nothing changes.", false),
        };
    }

    private static CampaignUpdateResult MoveToWatchtower(CampaignState campaign, DateTimeOffset now, StarterQuestDefinition quest)
    {
        var encounterDefinition = StarterGameContent.OpeningEncounter;
        var encounter = new EncounterState(
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

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            LocationName = quest.WatchtowerLocation,
            ActiveQuest = campaign.ActiveQuest with
            {
                Objective = quest.WatchtowerObjective,
                Stage = QuestStage.AtWatchtower,
            },
            CurrentEncounter = encounter,
            Journal = AddJournal(campaign, now, "travel", "You followed the eastern road and reached the ruined watchtower approach."),
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
            Journal = AddJournal(campaign, now, "quest", "You returned to Captain Elira, reported the cleared watchtower, and earned a hard-won level."),
        };

        return new CampaignUpdateResult(updatedCampaign, "You returned to Northgate Outpost, reported your success, and grew stronger from the first victory.");
    }

    private static IReadOnlyList<JournalEntry> AddJournal(CampaignState campaign, DateTimeOffset timestamp, string category, string text)
    {
        return campaign.Journal.Concat(new[] { new JournalEntry(timestamp, category, text) }).ToArray();
    }
}