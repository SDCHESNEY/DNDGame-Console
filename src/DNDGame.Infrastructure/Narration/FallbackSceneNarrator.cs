using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Infrastructure.Narration;

public sealed class FallbackSceneNarrator : ISceneNarrator
{
    private readonly ISceneNarrator _primary;
    private readonly ISceneNarrator _fallback;

    public FallbackSceneNarrator(ISceneNarrator primary, ISceneNarrator fallback)
    {
        _primary = primary;
        _fallback = fallback;
    }

    public Task<string> DescribeOpeningSceneAsync(CampaignState campaign, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(narrator => narrator.DescribeOpeningSceneAsync(campaign, cancellationToken), cancellationToken);
    }

    public Task<string> DescribeQuestUpdateAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(narrator => narrator.DescribeQuestUpdateAsync(campaign, summary, cancellationToken), cancellationToken);
    }

    public Task<string> DescribeCombatResolutionAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(narrator => narrator.DescribeCombatResolutionAsync(campaign, summary, cancellationToken), cancellationToken);
    }

    public Task<string> DescribeRecapAsync(CampaignState campaign, string recap, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(narrator => narrator.DescribeRecapAsync(campaign, recap, cancellationToken), cancellationToken);
    }

    public Task<string> DescribeJournalAsync(CampaignState campaign, IReadOnlyList<JournalEntry> journalEntries, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(narrator => narrator.DescribeJournalAsync(campaign, journalEntries, cancellationToken), cancellationToken);
    }

    public Task<string> DescribeNpcDialogueAsync(CampaignState campaign, string npcName, string npcContext, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(narrator => narrator.DescribeNpcDialogueAsync(campaign, npcName, npcContext, cancellationToken), cancellationToken);
    }

    private async Task<string> ExecuteAsync(Func<ISceneNarrator, Task<string>> operation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await operation(_primary);
        }
        catch (NarrationBoundaryException) when (!cancellationToken.IsCancellationRequested)
        {
            return await operation(_fallback);
        }
    }
}