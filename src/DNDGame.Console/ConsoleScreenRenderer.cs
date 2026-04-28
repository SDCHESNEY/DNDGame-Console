using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Console;

public sealed class ConsoleScreenRenderer
{
    private readonly TextWriter _output;

    public ConsoleScreenRenderer(TextWriter output)
    {
        _output = output;
    }

    public Task WriteHeadingAsync(string title)
    {
        return _output.WriteLineAsync($"== {title} ==");
    }

    public async Task WriteMenuAsync(string title, IReadOnlyList<string> options, bool leadingBlankLine = false)
    {
        if (leadingBlankLine)
        {
            await _output.WriteLineAsync(string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            await WriteHeadingAsync(title);
        }

        foreach (var option in options)
        {
            await _output.WriteLineAsync(option);
        }
    }

    public async Task WriteNarratedBlockAsync(string title, CampaignState campaign, string narration)
    {
        await WriteHeadingAsync(title);
        await _output.WriteLineAsync(CampaignRecapBuilder.Build(campaign));
        await _output.WriteLineAsync(string.Empty);
        await _output.WriteLineAsync(narration);
    }

    public Task WriteStatLineAsync(string label, string name, int currentValue, int maxValue)
    {
        return _output.WriteLineAsync($"{label}: {name} {currentValue}/{maxValue}");
    }

    public async Task WriteEncounterScreenAsync(CampaignState campaign)
    {
        var encounter = campaign.CurrentEncounter;
        if (encounter is null)
        {
            throw new InvalidOperationException("Encounter rendering requires an active encounter.");
        }

        await WriteHeadingAsync("Encounter");
        await _output.WriteLineAsync($"{encounter.Title}: {encounter.Description}");
        await WriteStatLineAsync("Enemy", encounter.Enemy.Name, encounter.Enemy.CurrentHealth, encounter.Enemy.MaxHealth);
        await WriteStatLineAsync("Hero", campaign.Hero.Name, campaign.Hero.CurrentHealth, campaign.Hero.MaxHealth);
        await WriteMenuAsync(
            "",
            [
                "1. Attack",
                "2. Defend",
                "3. Special",
                "4. Retreat to campaign menu",
            ]);
    }
}