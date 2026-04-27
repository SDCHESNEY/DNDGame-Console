using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public sealed record CampaignUpdateResult(
    CampaignState Campaign,
    string Summary,
    bool ShouldSave = true);