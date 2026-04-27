using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Infrastructure.Narration;

public sealed class LocalLlmHttpSceneNarrator : ISceneNarrator
{
    private readonly HttpClient _httpClient;
    private readonly LocalLlmSettings _settings;

    public LocalLlmHttpSceneNarrator(HttpClient httpClient, LocalLlmSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public Task<string> DescribeOpeningSceneAsync(CampaignState campaign, CancellationToken cancellationToken = default)
    {
        return GenerateNarrationAsync("opening scene", campaign, null, cancellationToken);
    }

    public Task<string> DescribeQuestUpdateAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
    {
        return GenerateNarrationAsync("quest update", campaign, summary, cancellationToken);
    }

    public Task<string> DescribeCombatResolutionAsync(CampaignState campaign, string summary, CancellationToken cancellationToken = default)
    {
        return GenerateNarrationAsync("combat resolution", campaign, summary, cancellationToken);
    }

    public Task<string> DescribeRecapAsync(CampaignState campaign, string recap, CancellationToken cancellationToken = default)
    {
        return GenerateNarrationAsync("campaign recap", campaign, recap, cancellationToken);
    }

    public Task<string> DescribeJournalAsync(CampaignState campaign, IReadOnlyList<JournalEntry> journalEntries, CancellationToken cancellationToken = default)
    {
        var journalContext = string.Join(" | ", journalEntries.OrderBy(entry => entry.TimestampUtc).TakeLast(3).Select(entry => entry.Text));
        return GenerateNarrationAsync("journal summary", campaign, journalContext, cancellationToken);
    }

    public Task<string> DescribeNpcDialogueAsync(CampaignState campaign, string npcName, string npcContext, CancellationToken cancellationToken = default)
    {
        return GenerateNarrationAsync($"npc dialogue from {npcName}", campaign, npcContext, cancellationToken);
    }

    private async Task<string> GenerateNarrationAsync(string mode, CampaignState campaign, string? summary, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        var request = new OllamaGenerateRequest(
            _settings.ModelName,
            BuildPrompt(mode, campaign, summary),
            false);

        using var response = await _httpClient.PostAsJsonAsync(GetGenerateUri(), request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Response))
        {
            throw new InvalidOperationException("Local LLM returned an empty narration response.");
        }

        return ValidateAndExtractNarration(payload.Response);
    }

    private Uri GetGenerateUri()
    {
        var baseUri = _settings.EndpointUrl.TrimEnd('/') + "/";
        return new Uri(new Uri(baseUri, UriKind.Absolute), "api/generate");
    }

    private static string BuildPrompt(string mode, CampaignState campaign, string? summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are the narrator for a cross-platform console RPG.");
        builder.AppendLine("Stay grounded in the provided state. Do not invent items, quests, victories, damage, or rules changes.");
        builder.AppendLine("Use 2 to 4 sentences. Keep the tone practical, vivid, and readable in a terminal.");
        builder.AppendLine("Return valid JSON only in the shape {\"text\":\"...\"}.");
        builder.AppendLine($"Mode: {mode}");
        builder.AppendLine($"Hero: {campaign.Hero.Name} the level {campaign.Hero.Level} {campaign.Hero.Class}");
        builder.AppendLine($"Location: {campaign.LocationName}");
        builder.AppendLine($"Quest: {campaign.ActiveQuest.Title}");
        builder.AppendLine($"Quest Stage: {campaign.ActiveQuest.Stage}");
        builder.AppendLine($"Objective: {campaign.ActiveQuest.Objective}");
        builder.AppendLine($"Health: {campaign.Hero.CurrentHealth}/{campaign.Hero.MaxHealth}");
        builder.AppendLine($"Inventory: {string.Join(", ", campaign.Inventory.Select(item => item.Quantity > 1 ? $"{item.Name} x{item.Quantity}" : item.Name))}");

        if (campaign.CurrentEncounter is not null)
        {
            builder.AppendLine($"Encounter: {campaign.CurrentEncounter.Title}");
            builder.AppendLine($"Enemy: {campaign.CurrentEncounter.Enemy.Name} {campaign.CurrentEncounter.Enemy.CurrentHealth}/{campaign.CurrentEncounter.Enemy.MaxHealth}");
        }

        if (!string.IsNullOrWhiteSpace(summary))
        {
            builder.AppendLine($"Deterministic Summary: {summary}");
        }

        builder.Append("Narration:");
        return builder.ToString();
    }

    private static string ValidateAndExtractNarration(string rawResponse)
    {
        try
        {
            var trimmedResponse = rawResponse.Trim();
            using var document = JsonDocument.Parse(trimmedResponse);

            if (document.RootElement.ValueKind != JsonValueKind.Object || !document.RootElement.TryGetProperty("text", out var textProperty))
            {
                throw new InvalidOperationException("Local LLM narration response did not match the required JSON schema.");
            }

            var text = textProperty.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("Local LLM narration response contained empty text.");
            }

            if (text.Contains("```") || text.Length > 600)
            {
                throw new InvalidOperationException("Local LLM narration response failed validation checks.");
            }

            var sentenceCount = text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
            if (sentenceCount is < 1 or > 6)
            {
                throw new InvalidOperationException("Local LLM narration response used an unexpected sentence count.");
            }

            return text;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Local LLM narration response could not be parsed as JSON.", exception);
        }
    }

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string Response);
}