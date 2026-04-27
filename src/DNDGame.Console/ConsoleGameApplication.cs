using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Console;

public sealed class ConsoleGameApplication
{
    private readonly ICampaignStorage _storage;
    private readonly ISceneNarrator _narrator;
    private readonly TextReader _standardInput;
    private readonly TextWriter _standardOutput;
    private readonly TextWriter _standardError;

    public ConsoleGameApplication(
        ICampaignStorage storage,
        ISceneNarrator narrator,
        TextReader standardInput,
        TextWriter standardOutput,
        TextWriter standardError)
    {
        _storage = storage;
        _narrator = narrator;
        _standardInput = standardInput;
        _standardOutput = standardOutput;
        _standardError = standardError;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (!CommandLineOptions.TryParse(args, out var options, out var error))
        {
            await _standardError.WriteLineAsync(error);
            await WriteHelpAsync(cancellationToken);
            return 2;
        }

        return options.Command switch
        {
            GameCommand.Menu => await RunMenuAsync(cancellationToken),
            GameCommand.Help => await WriteHelpAsync(cancellationToken),
            GameCommand.New => await StartNewGameAsync(options, cancellationToken),
            GameCommand.Load => await LoadGameAsync(options, cancellationToken),
            _ => 2,
        };
    }

    private async Task<int> StartNewGameAsync(CommandLineOptions options, CancellationToken cancellationToken)
    {
        var campaign = NewCampaignFactory.Create(options.SaveSlot, options.HeroName, options.CharacterClass);
        await _storage.SaveAsync(campaign, cancellationToken);
        var narration = await _narrator.DescribeOpeningSceneAsync(campaign, cancellationToken);

        await _standardOutput.WriteLineAsync("== New Campaign ==");
        await _standardOutput.WriteLineAsync(CampaignRecapBuilder.Build(campaign));
        await _standardOutput.WriteLineAsync(string.Empty);
        await _standardOutput.WriteLineAsync(narration);
        await _standardOutput.WriteLineAsync(string.Empty);
        await _standardOutput.WriteLineAsync($"Save directory: {_storage.SaveDirectory}");

        return 0;
    }

    private async Task<int> LoadGameAsync(CommandLineOptions options, CancellationToken cancellationToken)
    {
        var campaign = await _storage.LoadAsync(options.SaveSlot, cancellationToken);
        if (campaign is null)
        {
            await _standardError.WriteLineAsync($"No save slot named '{options.SaveSlot}' was found.");
            return 1;
        }

        await _standardOutput.WriteLineAsync("== Loaded Campaign ==");
        await _standardOutput.WriteLineAsync(CampaignRecapBuilder.Build(campaign));
        return 0;
    }

    private async Task<int> WriteHelpAsync(CancellationToken cancellationToken)
    {
        var saveSlots = await _storage.ListSaveSlotsAsync(cancellationToken);

        await _standardOutput.WriteLineAsync("DNDGame.Console");
        await _standardOutput.WriteLineAsync("Commands:");
        await _standardOutput.WriteLineAsync("  menu");
        await _standardOutput.WriteLineAsync("  new  --slot <name> --name <hero> --class <fighter|ranger|mage>");
        await _standardOutput.WriteLineAsync("  load --slot <name>");
        await _standardOutput.WriteLineAsync("  help");
        await _standardOutput.WriteLineAsync(string.Empty);
        await _standardOutput.WriteLineAsync($"Save directory: {_storage.SaveDirectory}");

        if (saveSlots.Count > 0)
        {
            await _standardOutput.WriteLineAsync($"Existing saves: {string.Join(", ", saveSlots)}");
        }

        return 0;
    }

    private async Task<int> RunMenuAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _standardOutput.WriteLineAsync("== Main Menu ==");
            await _standardOutput.WriteLineAsync("1. New game");
            await _standardOutput.WriteLineAsync("2. Load game");
            await _standardOutput.WriteLineAsync("3. Help");
            await _standardOutput.WriteLineAsync("4. Quit");

            var selection = await PromptAsync("Choose an option", cancellationToken);
            switch (selection)
            {
                case "1":
                {
                    var options = await PromptForNewGameOptionsAsync(cancellationToken);
                    if (options is null)
                    {
                        continue;
                    }

                    var campaign = NewCampaignFactory.Create(options.SaveSlot, options.HeroName, options.CharacterClass);
                    await _storage.SaveAsync(campaign, cancellationToken);
                    await RenderNarratedBlockAsync("== New Campaign ==", campaign, await _narrator.DescribeOpeningSceneAsync(campaign, cancellationToken));
                    await RunCampaignLoopAsync(campaign, cancellationToken);
                    break;
                }
                case "2":
                {
                    var saveSlots = await _storage.ListSaveSlotsAsync(cancellationToken);
                    if (saveSlots.Count == 0)
                    {
                        await _standardOutput.WriteLineAsync("No save slots are available yet.");
                        break;
                    }

                    await _standardOutput.WriteLineAsync($"Available saves: {string.Join(", ", saveSlots)}");
                    var slot = await PromptAsync("Enter save slot", cancellationToken);
                    if (string.IsNullOrWhiteSpace(slot))
                    {
                        break;
                    }

                    var campaign = await _storage.LoadAsync(slot, cancellationToken);
                    if (campaign is null)
                    {
                        await _standardError.WriteLineAsync($"No save slot named '{slot}' was found.");
                        break;
                    }

                    await _standardOutput.WriteLineAsync("== Loaded Campaign ==");
                    await _standardOutput.WriteLineAsync(CampaignRecapBuilder.Build(campaign));
                    await RunCampaignLoopAsync(campaign, cancellationToken);
                    break;
                }
                case "3":
                    await WriteHelpAsync(cancellationToken);
                    break;
                case "4":
                case null:
                    return 0;
                default:
                    await _standardOutput.WriteLineAsync("Enter 1, 2, 3, or 4.");
                    break;
            }
        }

        return 0;
    }

    private async Task RunCampaignLoopAsync(CampaignState campaign, CancellationToken cancellationToken)
    {
        var currentCampaign = campaign;

        while (!cancellationToken.IsCancellationRequested)
        {
            await _standardOutput.WriteLineAsync(string.Empty);
            await _standardOutput.WriteLineAsync("== Campaign Menu ==");
            await _standardOutput.WriteLineAsync("1. Status");
            await _standardOutput.WriteLineAsync("2. Journal");
            await _standardOutput.WriteLineAsync("3. Advance quest");

            if (currentCampaign.CurrentEncounter is not null)
            {
                await _standardOutput.WriteLineAsync("4. Fight current encounter");
            }

            await _standardOutput.WriteLineAsync("5. Save");
            await _standardOutput.WriteLineAsync("6. Quit to main menu");

            var selection = await PromptAsync("Choose an option", cancellationToken);
            switch (selection)
            {
                case "1":
                    await _standardOutput.WriteLineAsync(CampaignRecapBuilder.Build(currentCampaign));
                    break;
                case "2":
                    foreach (var entry in currentCampaign.Journal.OrderBy(entry => entry.TimestampUtc))
                    {
                        await _standardOutput.WriteLineAsync($"[{entry.TimestampUtc:u}] {entry.Category}: {entry.Text}");
                    }

                    break;
                case "3":
                {
                    var result = CampaignProgressionService.Advance(currentCampaign);
                    currentCampaign = result.Campaign;
                    if (result.ShouldSave)
                    {
                        await _storage.SaveAsync(currentCampaign, cancellationToken);
                    }

                    var narration = await _narrator.DescribeQuestUpdateAsync(currentCampaign, result.Summary, cancellationToken);
                    await RenderNarratedBlockAsync("== Quest Update ==", currentCampaign, narration);
                    break;
                }
                case "4" when currentCampaign.CurrentEncounter is not null:
                    currentCampaign = await RunEncounterLoopAsync(currentCampaign, cancellationToken);
                    break;
                case "5":
                    await _storage.SaveAsync(currentCampaign, cancellationToken);
                    await _standardOutput.WriteLineAsync("Campaign saved.");
                    break;
                case "6":
                case null:
                    await _storage.SaveAsync(currentCampaign, cancellationToken);
                    return;
                default:
                    await _standardOutput.WriteLineAsync("That option is not available right now.");
                    break;
            }
        }
    }

    private async Task<CampaignState> RunEncounterLoopAsync(CampaignState campaign, CancellationToken cancellationToken)
    {
        var currentCampaign = campaign;

        while (currentCampaign.CurrentEncounter is not null && !cancellationToken.IsCancellationRequested)
        {
            var encounter = currentCampaign.CurrentEncounter;
            await _standardOutput.WriteLineAsync("== Encounter ==");
            await _standardOutput.WriteLineAsync($"{encounter.Title}: {encounter.Description}");
            await _standardOutput.WriteLineAsync($"Enemy: {encounter.Enemy.Name} {encounter.Enemy.CurrentHealth}/{encounter.Enemy.MaxHealth}");
            await _standardOutput.WriteLineAsync($"Hero: {currentCampaign.Hero.Name} {currentCampaign.Hero.CurrentHealth}/{currentCampaign.Hero.MaxHealth}");
            await _standardOutput.WriteLineAsync("1. Attack");
            await _standardOutput.WriteLineAsync("2. Defend");
            await _standardOutput.WriteLineAsync("3. Special");
            await _standardOutput.WriteLineAsync("4. Retreat to campaign menu");

            var selection = await PromptAsync("Choose a combat action", cancellationToken);
            if (selection == "4" || selection is null)
            {
                return currentCampaign;
            }

            var action = selection switch
            {
                "1" => CombatAction.Attack,
                "2" => CombatAction.Defend,
                "3" => CombatAction.Special,
                _ => (CombatAction?)null,
            };

            if (action is null)
            {
                await _standardOutput.WriteLineAsync("Enter 1, 2, 3, or 4.");
                continue;
            }

            var result = CombatResolutionService.ResolveTurn(currentCampaign, action.Value);
            currentCampaign = result.Campaign;
            await _storage.SaveAsync(currentCampaign, cancellationToken);

            var narration = await _narrator.DescribeCombatResolutionAsync(currentCampaign, result.Summary, cancellationToken);
            await RenderNarratedBlockAsync("== Combat ==", currentCampaign, narration);
        }

        return currentCampaign;
    }

    private async Task<CommandLineOptions?> PromptForNewGameOptionsAsync(CancellationToken cancellationToken)
    {
        var saveSlot = await PromptAsync("Save slot", cancellationToken);
        if (string.IsNullOrWhiteSpace(saveSlot))
        {
            await _standardError.WriteLineAsync("Save slot is required.");
            return null;
        }

        var heroName = await PromptAsync("Hero name", cancellationToken);
        if (string.IsNullOrWhiteSpace(heroName))
        {
            await _standardError.WriteLineAsync("Hero name is required.");
            return null;
        }

        await _standardOutput.WriteLineAsync("Classes:");
        foreach (var definition in DNDGame.Core.Content.StarterGameContent.Classes)
        {
            await _standardOutput.WriteLineAsync($"- {definition.Title}: {definition.Description}");
        }

        var classValue = await PromptAsync("Class", cancellationToken);
        if (!Enum.TryParse<CharacterClass>(classValue, true, out var characterClass))
        {
            await _standardError.WriteLineAsync("Supported classes: fighter, ranger, mage.");
            return null;
        }

        return new CommandLineOptions(GameCommand.New, saveSlot, heroName, characterClass);
    }

    private async Task RenderNarratedBlockAsync(string title, CampaignState campaign, string narration)
    {
        await _standardOutput.WriteLineAsync(title);
        await _standardOutput.WriteLineAsync(CampaignRecapBuilder.Build(campaign));
        await _standardOutput.WriteLineAsync(string.Empty);
        await _standardOutput.WriteLineAsync(narration);
    }

    private async Task<string?> PromptAsync(string label, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _standardOutput.WriteAsync($"{label}: ");
        return await _standardInput.ReadLineAsync();
    }
}