using System.Net;
using System.Text;
using DNDGame.Core.Models;
using DNDGame.Core.Services;
using DNDGame.Infrastructure.Narration;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class LocalLlmHttpSceneNarratorTests
{
    [TestMethod]
    public async Task DescribeOpeningSceneAsync_ValidJsonPayload_ReturnsNarrationText()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler("{\"response\":\"{\\\"text\\\":\\\"The watchtower looms over the road.\\\"}\"}"));
        var narrator = new LocalLlmHttpSceneNarrator(httpClient, LocalLlmSettings.Default);
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);

        var narration = await narrator.DescribeOpeningSceneAsync(campaign);

        Assert.AreEqual("The watchtower looms over the road.", narration);
    }

    [TestMethod]
    public async Task DescribeOpeningSceneAsync_InvalidPayload_ThrowsInvalidOperationException()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler("{\"response\":\"plain text\"}"));
        var narrator = new LocalLlmHttpSceneNarrator(httpClient, LocalLlmSettings.Default);
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);

        await Assert.ThrowsExceptionAsync<NarrationGuardrailException>(() => narrator.DescribeOpeningSceneAsync(campaign));
    }

    [TestMethod]
    public async Task DescribeOpeningSceneAsync_HttpFailure_ThrowsNarrationTransportException()
    {
        using var httpClient = new HttpClient(new ThrowingHttpMessageHandler(new HttpRequestException("offline")));
        var narrator = new LocalLlmHttpSceneNarrator(httpClient, LocalLlmSettings.Default);
        var campaign = NewCampaignFactory.Create("slot", "Mira", CharacterClass.Mage);

        await Assert.ThrowsExceptionAsync<NarrationTransportException>(() => narrator.DescribeOpeningSceneAsync(campaign));
    }

    private sealed class FakeHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };

            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHttpMessageHandler(Exception exceptionToThrow) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw exceptionToThrow;
        }
    }
}