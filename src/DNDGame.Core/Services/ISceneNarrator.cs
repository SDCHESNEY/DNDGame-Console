using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public interface ISceneNarrator
{
    Task<string> DescribeOpeningSceneAsync(CampaignState campaign, CancellationToken cancellationToken = default);

    Task<string> DescribeQuestUpdateAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default);

    Task<string> DescribeCombatResolutionAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default);

    Task<string> DescribeRecapAsync(CampaignState campaign, string recap, CancellationToken cancellationToken = default);

    Task<string> DescribeJournalAsync(CampaignState campaign, IReadOnlyList<JournalEntry> journalEntries, CancellationToken cancellationToken = default);

    Task<string> DescribeNpcDialogueAsync(CampaignState campaign, string npcName, string npcContext, CancellationToken cancellationToken = default);
}