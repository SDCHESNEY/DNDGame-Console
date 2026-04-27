using DNDGame.Core.Content;
using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public static class CombatResolutionService
{
    public static CampaignUpdateResult ResolveTurn(CampaignState campaign, CombatAction action)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        var encounter = campaign.CurrentEncounter;
        if (encounter is null || encounter.IsCompleted)
        {
            return new CampaignUpdateResult(campaign, "There is no active encounter to resolve.", false);
        }

        var now = DateTimeOffset.UtcNow;
        var classDefinition = StarterGameContent.GetClassDefinition(campaign.Hero.Class);
        var heroDamage = action switch
        {
            CombatAction.Attack => classDefinition.AttackPower,
            CombatAction.Defend => 0,
            CombatAction.Special => classDefinition.SpecialAttackPower,
            _ => classDefinition.AttackPower,
        };

        var defenseBonus = action == CombatAction.Defend ? 2 : 0;
        var actionLabel = action switch
        {
            CombatAction.Attack => "attack",
            CombatAction.Defend => "defend",
            CombatAction.Special => classDefinition.SpecialActionName,
            _ => "attack",
        };

        var enemy = encounter.Enemy;
        var enemyDamageTaken = Math.Max(0, heroDamage - enemy.Armor);
        var remainingEnemyHealth = Math.Max(0, enemy.CurrentHealth - enemyDamageTaken);

        if (remainingEnemyHealth == 0)
        {
            var victoryCampaign = campaign with
            {
                UpdatedUtc = now,
                LocationName = StarterGameContent.OpeningQuest.ClearedWatchtowerLocation,
                ActiveQuest = campaign.ActiveQuest with
                {
                    Objective = StarterGameContent.OpeningQuest.ReturnObjective,
                    Stage = QuestStage.WatchtowerCleared,
                },
                CurrentEncounter = null,
                Journal = campaign.Journal.Concat(new[]
                {
                    new JournalEntry(now, "combat", $"You used {actionLabel} to defeat the goblin scout at the watchtower."),
                }).ToArray(),
            };

            return new CampaignUpdateResult(victoryCampaign, $"Your {actionLabel} lands cleanly. The goblin scout falls, and the road into the ruined watchtower is finally open.");
        }

        var enemyDamage = Math.Max(1, enemy.AttackPower - (classDefinition.Armor + defenseBonus));
        var remainingHeroHealth = Math.Max(0, campaign.Hero.CurrentHealth - enemyDamage);

        if (remainingHeroHealth == 0)
        {
            var resetHero = campaign.Hero with { CurrentHealth = campaign.Hero.MaxHealth };
            var defeatCampaign = campaign with
            {
                UpdatedUtc = now,
                Hero = resetHero,
                LocationName = StarterGameContent.OpeningQuest.OutpostLocation,
                ActiveQuest = campaign.ActiveQuest with
                {
                    Objective = StarterGameContent.OpeningQuest.InitialObjective,
                    Stage = QuestStage.Accepted,
                },
                CurrentEncounter = null,
                Journal = campaign.Journal.Concat(new[]
                {
                    new JournalEntry(now, "combat", "You were driven back from the watchtower and recovered at Northgate Outpost."),
                }).ToArray(),
            };

            return new CampaignUpdateResult(defeatCampaign, "The goblin scout drives you back. You stagger to Northgate Outpost, regroup, and prepare to try again.");
        }

        var updatedEncounter = encounter with
        {
            RoundNumber = encounter.RoundNumber + 1,
            Enemy = enemy with { CurrentHealth = remainingEnemyHealth },
        };

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            Hero = campaign.Hero with { CurrentHealth = remainingHeroHealth },
            CurrentEncounter = updatedEncounter,
            Journal = campaign.Journal.Concat(new[]
            {
                new JournalEntry(now, "combat", $"Round {encounter.RoundNumber}: you used {actionLabel}, dealt {enemyDamageTaken} damage, and took {enemyDamage} in return."),
            }).ToArray(),
        };

        return new CampaignUpdateResult(updatedCampaign, $"You use {actionLabel}, dealing {enemyDamageTaken} damage. The {enemy.Name} strikes back for {enemyDamage}. {enemy.Name} has {remainingEnemyHealth} health left; you have {remainingHeroHealth}.");
    }
}