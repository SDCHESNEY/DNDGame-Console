using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public interface ISceneNarrator
{
    Task<string> DescribeOpeningSceneAsync(CampaignState campaign, CancellationToken cancellationToken = default);

    Task<string> DescribeQuestUpdateAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default);

    Task<string> DescribeCombatResolutionAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default);
}