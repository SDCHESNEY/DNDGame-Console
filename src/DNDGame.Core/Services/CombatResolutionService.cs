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
        var defensiveAbility = classDefinition.Abilities.FirstOrDefault(static ability => ability.IsDefensive);
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
            var nextStage = campaign.ActiveQuest.Stage switch
            {
                QuestStage.AtWatchtowerApproach => QuestStage.WatchtowerApproachCleared,
                QuestStage.InWatchtowerCourtyard => QuestStage.WatchtowerCleared,
                _ => campaign.ActiveQuest.Stage,
            };

            var nextObjective = nextStage switch
            {
                QuestStage.WatchtowerApproachCleared => StarterGameContent.OpeningQuest.CourtyardObjective,
                QuestStage.WatchtowerCleared => StarterGameContent.OpeningQuest.ReturnObjective,
                _ => campaign.ActiveQuest.Objective,
            };

            var nextLocation = nextStage switch
            {
                QuestStage.WatchtowerApproachCleared => StarterGameContent.OpeningQuest.CourtyardLocation,
                QuestStage.WatchtowerCleared => StarterGameContent.OpeningQuest.ClearedWatchtowerLocation,
                _ => campaign.LocationName,
            };

            var rewardLoot = GetRewardLoot(encounter.EncounterId);
            var victoryCampaign = campaign with
            {
                UpdatedUtc = now,
                LocationName = nextLocation,
                ActiveQuest = campaign.ActiveQuest with
                {
                    Objective = nextObjective,
                    Stage = nextStage,
                },
                CurrentEncounter = null,
                Inventory = AddLoot(campaign.Inventory, rewardLoot),
                Journal = campaign.Journal.Concat(new[]
                {
                    new JournalEntry(now, "combat", $"You used {actionLabel} to defeat {enemy.Name} at the watchtower."),
                    new JournalEntry(now, "loot", $"You secured {rewardLoot.Name} from the fallen enemy."),
                }).ToArray(),
            };

            var summary = nextStage == QuestStage.WatchtowerApproachCleared
                ? $"Your {actionLabel} lands cleanly. The goblin scout falls, leaving the path into the ruined watchtower courtyard open."
                : $"Your {actionLabel} breaks the hobgoblin raider's defense. The courtyard falls quiet, and the tower is finally secure.";

            return new CampaignUpdateResult(victoryCampaign, summary);
        }

        var enemyDamage = Math.Max(1, enemy.AttackPower - (classDefinition.Armor + defenseBonus + (action == CombatAction.Defend ? defensiveAbility?.PowerBonus ?? 0 : 0)));
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
                    new JournalEntry(now, "combat", $"You were driven back by {enemy.Name} and recovered at Northgate Outpost."),
                }).ToArray(),
            };

            return new CampaignUpdateResult(defeatCampaign, $"{enemy.Name} drives you back. You stagger to Northgate Outpost, regroup, and prepare to try again.");
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

    private static LootDefinition GetRewardLoot(string encounterId)
    {
        return encounterId switch
        {
            "watchtower-goblin-scout" => StarterGameContent.OpeningEncounter.RewardLoot[0],
            "watchtower-hobgoblin-raider" => StarterGameContent.CourtyardEncounter.RewardLoot[0],
            _ => throw new InvalidOperationException($"Unknown encounter '{encounterId}'."),
        };
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