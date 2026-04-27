using DNDGame.Core.Models;

namespace DNDGame.Core.Services;

public interface ICampaignStorage
{
    string SaveDirectory { get; }

    Task SaveAsync(CampaignState campaign, CancellationToken cancellationToken = default);

    Task<CampaignState?> LoadAsync(string saveSlot, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListSaveSlotsAsync(CancellationToken cancellationToken = default);
}