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
        builder.AppendLine($"Objective: {campaign.ActiveQuest.Objective}");

        var latestEntry = campaign.Journal.OrderByDescending(entry => entry.TimestampUtc).FirstOrDefault();
        if (latestEntry is not null)
        {
            builder.AppendLine($"Latest Journal: {latestEntry.Text}");
        }

        return builder.ToString().TrimEnd();
    }
}