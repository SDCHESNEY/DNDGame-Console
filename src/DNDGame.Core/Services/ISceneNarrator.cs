using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public interface ISceneNarrator
{
    Task<string> DescribeOpeningSceneAsync(CampaignState campaign, CancellationToken cancellationToken = default);
}