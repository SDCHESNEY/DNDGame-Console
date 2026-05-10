using System.Text;
using DNDGame.Core.Content;
using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public static class CampaignRecapBuilder
{
    public static string Build(CampaignState campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        var builder = new StringBuilder();
        builder.AppendLine($"Hero: {campaign.Hero.Name} the level {campaign.Hero.Level} {campaign.Hero.Class}");
        builder.AppendLine($"Region: {campaign.RegionName}");
        builder.AppendLine($"Location: {campaign.LocationName}");
        builder.AppendLine($"Active Quest: {campaign.ActiveQuest.Title}");
        builder.AppendLine($"Quest Stage: {campaign.ActiveQuest.Stage}");
        builder.AppendLine($"Objective: {campaign.ActiveQuest.Objective}");
        builder.AppendLine($"Health: {campaign.Hero.CurrentHealth}/{campaign.Hero.MaxHealth}");
        builder.AppendLine($"Abilities: {string.Join(", ", StarterGameContent.GetClassDefinition(campaign.Hero.Class).Abilities.Select(static ability => ability.Name))}");

        if (campaign.Inventory.Count > 0)
        {
            builder.AppendLine($"Inventory: {string.Join(", ", campaign.Inventory.Select(static item => item.Quantity > 1 ? $"{item.Name} x{item.Quantity}" : item.Name))}");
        }

        if (campaign.RecapSnapshots.Count > 0)
        {
            builder.AppendLine($"Recap Snapshots: {string.Join(" | ", campaign.RecapSnapshots.OrderBy(snapshot => snapshot.CreatedUtc).Select(static snapshot => snapshot.Summary))}");
        }

        if (campaign.CurrentEncounter is not null)
        {
            builder.AppendLine($"Encounter: {campaign.CurrentEncounter.Title} vs {campaign.CurrentEncounter.Enemy.Name} ({campaign.CurrentEncounter.Enemy.CurrentHealth}/{campaign.CurrentEncounter.Enemy.MaxHealth})");

            var statusEffects = new List<string>();
            if (campaign.CurrentEncounter.HeroGuardedRounds > 0)
            {
                statusEffects.Add($"Hero Guarded ({campaign.CurrentEncounter.HeroGuardedRounds})");
            }

            if (campaign.CurrentEncounter.EnemySunderedRounds > 0)
            {
                statusEffects.Add($"Enemy Sundered ({campaign.CurrentEncounter.EnemySunderedRounds})");
            }

            if (statusEffects.Count > 0)
            {
                builder.AppendLine($"Encounter Status: {string.Join(", ", statusEffects)}");
            }
        }

        var latestEntry = campaign.Journal.OrderByDescending(entry => entry.TimestampUtc).FirstOrDefault();
        if (latestEntry is not null)
        {
            builder.AppendLine($"Latest Journal: {latestEntry.Text}");
        }

        return builder.ToString().TrimEnd();
    }
}