using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Infrastructure.Narration;

public sealed class DeterministicSceneNarrator : ISceneNarrator
{
    private readonly LocalLlmSettings _settings;

    public DeterministicSceneNarrator(LocalLlmSettings? settings = null)
    {
        _settings = settings ?? LocalLlmSettings.Default;
    }

    public Task<string> DescribeOpeningSceneAsync(CampaignState campaign, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        cancellationToken.ThrowIfCancellationRequested();

        var narration = $"A cold wind moves through {campaign.LocationName} as {campaign.Hero.Name} prepares for the road east. " +
                        $"The frontier of {campaign.RegionName} is uneasy, and the ruined watchtower has gone silent again. " +
                        $"Your current objective is clear: {campaign.ActiveQuest.Objective} " +
                        $"Narration mode is currently using a deterministic fallback so the campaign stays playable even without a live local model at {_settings.EndpointUrl}.";

        return Task.FromResult(narration);
    }

    public Task<string> DescribeQuestUpdateAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult($"{summary} {campaign.Hero.Name} now stands at {campaign.LocationName}, with the quest focused on: {campaign.ActiveQuest.Objective}");
    }

    public Task<string> DescribeCombatResolutionAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult($"{summary} {campaign.Hero.Name} now has {campaign.Hero.CurrentHealth} health remaining.");
    }

    public Task<string> DescribeRecapAsync(CampaignState campaign, string recap, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult($"Campaign recap: {recap}");
    }

    public Task<string> DescribeJournalAsync(CampaignState campaign, IReadOnlyList<JournalEntry> journalEntries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        cancellationToken.ThrowIfCancellationRequested();

        var latestEntry = journalEntries.OrderByDescending(entry => entry.TimestampUtc).FirstOrDefault();
        return Task.FromResult(latestEntry is null
            ? "The journal is quiet for now."
            : $"Journal summary: the most recent note says '{latestEntry.Text}' and points toward {campaign.ActiveQuest.Objective}");
    }

    public Task<string> DescribeNpcDialogueAsync(CampaignState campaign, string npcName, string npcContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult($"{npcName} says: \"{npcContext} Hold the frontier, {campaign.Hero.Name}.\"");
    }
}