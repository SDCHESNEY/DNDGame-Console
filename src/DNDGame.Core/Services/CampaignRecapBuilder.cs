using System.Text;
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

        if (campaign.CurrentEncounter is not null)
        {
            builder.AppendLine($"Encounter: {campaign.CurrentEncounter.Title} vs {campaign.CurrentEncounter.Enemy.Name} ({campaign.CurrentEncounter.Enemy.CurrentHealth}/{campaign.CurrentEncounter.Enemy.MaxHealth})");
        }

        var latestEntry = campaign.Journal.OrderByDescending(entry => entry.TimestampUtc).FirstOrDefault();
        if (latestEntry is not null)
        {
            builder.AppendLine($"Latest Journal: {latestEntry.Text}");
        }

        return builder.ToString().TrimEnd();
    }
}