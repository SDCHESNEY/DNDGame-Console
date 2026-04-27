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
}