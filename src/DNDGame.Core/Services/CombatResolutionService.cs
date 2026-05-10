using DNDGame.Core.Content;
using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public static class CombatResolutionService
{
    private const string MinorPotionItemId = "minor-potion";
    private const int MinorPotionHealing = 8;
    private const int GuardedDamageReduction = 2;
    private const int SunderedArmorReduction = 1;

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

        if (action == CombatAction.UseItem)
        {
            return new CampaignUpdateResult(campaign, "Choose a combat item before resolving a use-item turn.", false);
        }

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
    var armorPenalty = encounter.EnemySunderedRounds > 0 ? SunderedArmorReduction : 0;
    var effectiveEnemyArmor = Math.Max(0, enemy.Armor - armorPenalty);
    var enemyDamageTaken = Math.Max(0, heroDamage - effectiveEnemyArmor);
        var remainingEnemyHealth = Math.Max(0, enemy.CurrentHealth - enemyDamageTaken);
    var appliesGuarded = action == CombatAction.Defend;
    var appliesSundered = action == CombatAction.Special && remainingEnemyHealth > 0;

        if (remainingEnemyHealth == 0)
        {
            var nextStage = campaign.ActiveQuest.Stage switch
            {
                QuestStage.AtWatchtowerApproach => QuestStage.WatchtowerApproachCleared,
                QuestStage.InWatchtowerCourtyard => QuestStage.WatchtowerCourtyardCleared,
                QuestStage.InWatchtowerSummit => QuestStage.WatchtowerCleared,
                _ => campaign.ActiveQuest.Stage,
            };

            var nextObjective = nextStage switch
            {
                QuestStage.WatchtowerApproachCleared => StarterGameContent.OpeningQuest.CourtyardObjective,
                QuestStage.WatchtowerCourtyardCleared => StarterGameContent.OpeningQuest.BossObjective,
                QuestStage.WatchtowerCleared => StarterGameContent.OpeningQuest.ReturnObjective,
                _ => campaign.ActiveQuest.Objective,
            };

            var nextLocation = nextStage switch
            {
                QuestStage.WatchtowerApproachCleared => StarterGameContent.OpeningQuest.CourtyardLocation,
                QuestStage.WatchtowerCourtyardCleared => StarterGameContent.OpeningQuest.BossLocation,
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
                : nextStage == QuestStage.WatchtowerCourtyardCleared
                    ? $"Your {actionLabel} breaks the hobgoblin raider's defense. A path to the summit opens, but the tower's true commander still waits above."
                : $"Your {actionLabel} breaks the hobgoblin raider's defense. The courtyard falls quiet, and the tower is finally secure.";

            return new CampaignUpdateResult(victoryCampaign, summary);
        }

        var guardedReduction = appliesGuarded || encounter.HeroGuardedRounds > 0 ? GuardedDamageReduction : 0;
        var enemyDamage = Math.Max(1, enemy.AttackPower - (classDefinition.Armor + defenseBonus + guardedReduction + (action == CombatAction.Defend ? defensiveAbility?.PowerBonus ?? 0 : 0)));
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
            HeroGuardedRounds = appliesGuarded ? 1 : 0,
            EnemySunderedRounds = appliesSundered ? 1 : 0,
        };

        var statusFragments = BuildStatusFragments(effectiveEnemyArmor, guardedReduction, appliesSundered, appliesGuarded || encounter.HeroGuardedRounds > 0);

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            Hero = campaign.Hero with { CurrentHealth = remainingHeroHealth },
            CurrentEncounter = updatedEncounter,
            Journal = campaign.Journal.Concat(new[]
            {
                new JournalEntry(now, "combat", $"Round {encounter.RoundNumber}: you used {actionLabel}, dealt {enemyDamageTaken} damage, and took {enemyDamage} in return.{statusFragments.JournalSuffix}"),
            }).ToArray(),
        };

        return new CampaignUpdateResult(updatedCampaign, $"You use {actionLabel}, dealing {enemyDamageTaken} damage. The {enemy.Name} strikes back for {enemyDamage}. {enemy.Name} has {remainingEnemyHealth} health left; you have {remainingHeroHealth}.{statusFragments.SummarySuffix}");
    }

    public static CampaignUpdateResult UseCombatItem(CampaignState campaign, string itemId)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        var encounter = campaign.CurrentEncounter;
        if (encounter is null || encounter.IsCompleted)
        {
            return new CampaignUpdateResult(campaign, "There is no active encounter to use an item in.", false);
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return new CampaignUpdateResult(campaign, "Choose a valid combat item.", false);
        }

        var item = campaign.Inventory.FirstOrDefault(inventoryItem => string.Equals(inventoryItem.ItemId, itemId, StringComparison.Ordinal));
        if (item is null)
        {
            return new CampaignUpdateResult(campaign, $"No inventory item named '{itemId}' is available.", false);
        }

        if (!IsCombatUsable(item))
        {
            return new CampaignUpdateResult(campaign, $"{item.Name} cannot be used during combat.", false);
        }

        var now = DateTimeOffset.UtcNow;
        var healedAmount = item.ItemId switch
        {
            MinorPotionItemId => Math.Min(MinorPotionHealing, campaign.Hero.MaxHealth - campaign.Hero.CurrentHealth),
            _ => 0,
        };

        var inventoryAfterUse = RemoveOneItem(campaign.Inventory, item.ItemId);
        var heroAfterUse = campaign.Hero with
        {
            CurrentHealth = Math.Min(campaign.Hero.MaxHealth, campaign.Hero.CurrentHealth + healedAmount),
        };

        var journal = campaign.Journal.Concat(new[]
        {
            new JournalEntry(now, "item", healedAmount > 0
                ? $"You used {item.Name} and recovered {healedAmount} health."
                : $"You used {item.Name}, but it had no effect."),
        }).ToArray();

        var updatedCampaign = campaign with
        {
            UpdatedUtc = now,
            Hero = heroAfterUse,
            Inventory = inventoryAfterUse,
            Journal = journal,
        };

        return ResolveEnemyCounterAttack(updatedCampaign, encounter, item.Name, now);
    }

    private static LootDefinition GetRewardLoot(string encounterId)
    {
        return encounterId switch
        {
            "watchtower-goblin-scout" => StarterGameContent.OpeningEncounter.RewardLoot[0],
            "watchtower-hobgoblin-raider" => StarterGameContent.CourtyardEncounter.RewardLoot[0],
            "watchtower-raider-captain" => StarterGameContent.BossEncounter.RewardLoot[0],
            _ => throw new InvalidOperationException($"Unknown encounter '{encounterId}'."),
        };
    }

    private static CampaignUpdateResult ResolveEnemyCounterAttack(CampaignState campaign, EncounterState encounter, string itemName, DateTimeOffset timestamp)
    {
        var classDefinition = StarterGameContent.GetClassDefinition(campaign.Hero.Class);
        var enemy = encounter.Enemy;
        var guardedReduction = encounter.HeroGuardedRounds > 0 ? GuardedDamageReduction : 0;
        var enemyDamage = Math.Max(1, enemy.AttackPower - (classDefinition.Armor + guardedReduction));
        var remainingHeroHealth = Math.Max(0, campaign.Hero.CurrentHealth - enemyDamage);

        if (remainingHeroHealth == 0)
        {
            var resetHero = campaign.Hero with { CurrentHealth = campaign.Hero.MaxHealth };
            var defeatCampaign = campaign with
            {
                UpdatedUtc = timestamp,
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
                    new JournalEntry(timestamp, "combat", $"You were driven back by {enemy.Name} after using {itemName} and recovered at Northgate Outpost."),
                }).ToArray(),
            };

            return new CampaignUpdateResult(defeatCampaign, $"You use {itemName}, but {enemy.Name} punishes the opening and drives you back to Northgate Outpost.");
        }

        var updatedCampaign = campaign with
        {
            UpdatedUtc = timestamp,
            Hero = campaign.Hero with { CurrentHealth = remainingHeroHealth },
            CurrentEncounter = encounter with
            {
                RoundNumber = encounter.RoundNumber + 1,
                HeroGuardedRounds = 0,
                EnemySunderedRounds = Math.Max(0, encounter.EnemySunderedRounds - 1),
            },
            Journal = campaign.Journal.Concat(new[]
            {
                new JournalEntry(timestamp, "combat", $"After you used {itemName}, {enemy.Name} struck back for {enemyDamage} damage."),
            }).ToArray(),
        };

        var healedAmount = updatedCampaign.Hero.CurrentHealth - (campaign.Hero.CurrentHealth - enemyDamage);
        _ = healedAmount;

        return new CampaignUpdateResult(updatedCampaign, $"You use {itemName}. {enemy.Name} strikes back for {enemyDamage}. You now have {remainingHeroHealth} health.");
    }

    private static bool IsCombatUsable(InventoryItem item)
    {
        return string.Equals(item.ItemId, MinorPotionItemId, StringComparison.Ordinal)
            && string.Equals(item.Category, "consumable", StringComparison.OrdinalIgnoreCase)
            && item.Quantity > 0;
    }

    private static IReadOnlyList<InventoryItem> RemoveOneItem(IReadOnlyList<InventoryItem> inventory, string itemId)
    {
        return inventory
            .SelectMany(item => item.ItemId == itemId
                ? item.Quantity > 1
                    ? new[] { item with { Quantity = item.Quantity - 1 } }
                    : Array.Empty<InventoryItem>()
                : new[] { item })
            .ToArray();
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

    private static (string SummarySuffix, string JournalSuffix) BuildStatusFragments(int effectiveEnemyArmor, int guardedReduction, bool appliesSundered, bool guardedActive)
    {
        var summaryParts = new List<string>();
        var journalParts = new List<string>();

        if (appliesSundered)
        {
            summaryParts.Add($"{SunderedArmorReduction} enemy armor is sundered for the next exchange");
            journalParts.Add($"Enemy armor is sundered to {effectiveEnemyArmor} for the next round");
        }

        if (guardedActive)
        {
            summaryParts.Add($"guard reduces incoming damage by {guardedReduction}");
            journalParts.Add($"Guard reduced incoming damage by {guardedReduction}");
        }

        if (summaryParts.Count == 0)
        {
            return (string.Empty, string.Empty);
        }

        return ($" Active effects: {string.Join("; ", summaryParts)}.", $" {string.Join(". ", journalParts)}.");
    }
}