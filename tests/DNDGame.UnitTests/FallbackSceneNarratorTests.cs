using DNDGame.Core.Models;
using DNDGame.Core.Services;
using DNDGame.Infrastructure.Narration;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class FallbackSceneNarratorTests
{
    [TestMethod]
    public async Task DescribeOpeningSceneAsync_WhenPrimaryHasGuardrailFailure_UsesFallback()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);
        var narrator = new FallbackSceneNarrator(
            new ThrowingNarrator(new NarrationGuardrailException("invalid narration")),
            new DeterministicSceneNarrator());

        var narration = await narrator.DescribeOpeningSceneAsync(campaign);

        StringAssert.Contains(narration, "deterministic fallback");
    }

    [TestMethod]
    public async Task DescribeOpeningSceneAsync_WhenPrimaryHasUnexpectedFailure_DoesNotHideBug()
    {
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);
        var narrator = new FallbackSceneNarrator(
            new ThrowingNarrator(new InvalidOperationException("programmer bug")),
            new DeterministicSceneNarrator());

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => narrator.DescribeOpeningSceneAsync(campaign));
    }

    private sealed class ThrowingNarrator(Exception exceptionToThrow) : ISceneNarrator
    {
        public Task<string> DescribeOpeningSceneAsync(CampaignState campaign, CancellationToken cancellationToken = default)
        {
            throw exceptionToThrow;
        }

        public Task<string> DescribeQuestUpdateAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
        {
            throw exceptionToThrow;
        }

        public Task<string> DescribeCombatResolutionAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
        {
            throw exceptionToThrow;
        }

        public Task<string> DescribeRecapAsync(CampaignState campaign, string recap, CancellationToken cancellationToken = default)
        {
            throw exceptionToThrow;
        }

        public Task<string> DescribeJournalAsync(CampaignState campaign, IReadOnlyList<JournalEntry> journalEntries, CancellationToken cancellationToken = default)
        {
            throw exceptionToThrow;
        }

        public Task<string> DescribeNpcDialogueAsync(CampaignState campaign, string npcName, string npcContext, CancellationToken cancellationToken = default)
        {
            throw exceptionToThrow;
        }
    }
}