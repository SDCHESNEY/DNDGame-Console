using DNDGame.Core.Models;
using DNDGame.Core.Services;

namespace DNDGame.Console;

public sealed class ConsoleGameApplication
{
    private readonly ICampaignStorage _storage;
    private readonly ISceneNarrator _narrator;
    private readonly ConsoleScreenRenderer _screenRenderer;
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
        _screenRenderer = new ConsoleScreenRenderer(standardOutput);
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

        await _screenRenderer.WriteNarratedBlockAsync("New Campaign", campaign, narration);
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

        var recap = CampaignRecapBuilder.Build(campaign);
        var narration = await _narrator.DescribeRecapAsync(campaign, recap, cancellationToken);

        await _screenRenderer.WriteNarratedBlockAsync("Loaded Campaign", campaign, narration);
        return 0;
    }

    private async Task<int> WriteHelpAsync(CancellationToken cancellationToken)
    {
        var saveSlotMetadata = await _storage.ListSaveSlotMetadataAsync(cancellationToken);

        await _standardOutput.WriteLineAsync("DNDGame.Console");
        await _standardOutput.WriteLineAsync("Commands:");
        await _standardOutput.WriteLineAsync("  menu");
        await _standardOutput.WriteLineAsync("  new  --slot <name> --name <hero> --class <fighter|ranger|mage>");
        await _standardOutput.WriteLineAsync("  load --slot <name>");
        await _standardOutput.WriteLineAsync("  help");
        await _standardOutput.WriteLineAsync(string.Empty);
        await _standardOutput.WriteLineAsync($"Save directory: {_storage.SaveDirectory}");

        if (saveSlotMetadata.Count > 0)
        {
            await WriteSaveSlotMetadataAsync(saveSlotMetadata, cancellationToken);
        }

        return 0;
    }

    private async Task<int> RunMenuAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _screenRenderer.WriteMenuAsync(
                "Main Menu",
                [
                    "1. New game",
                    "2. Load game",
                    "3. Help",
                    "4. Quit",
                ]);

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
                    await _screenRenderer.WriteNarratedBlockAsync("New Campaign", campaign, await _narrator.DescribeOpeningSceneAsync(campaign, cancellationToken));
                    await RunCampaignLoopAsync(campaign, cancellationToken);
                    break;
                }
                case "2":
                {
                    var saveSlotMetadata = await _storage.ListSaveSlotMetadataAsync(cancellationToken);
                    if (saveSlotMetadata.Count == 0)
                    {
                        await _standardOutput.WriteLineAsync("No save slots are available yet.");
                        break;
                    }

                    await WriteSaveSlotMetadataAsync(saveSlotMetadata, cancellationToken);
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

                    await _screenRenderer.WriteHeadingAsync("Loaded Campaign");
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
            var menuOptions = new List<string>
            {
                "1. Status",
                "2. Journal",
                "3. Advance quest",
            };

            if (currentCampaign.CurrentEncounter is not null)
            {
                menuOptions.Add("4. Fight current encounter");
            }

            menuOptions.Add("5. Save");
            menuOptions.Add("6. Quit to main menu");

            await _screenRenderer.WriteMenuAsync("Campaign Menu", menuOptions, leadingBlankLine: true);

            var selection = await PromptAsync("Choose an option", cancellationToken);
            switch (selection)
            {
                case "1":
                    await _standardOutput.WriteLineAsync(CampaignRecapBuilder.Build(currentCampaign));
                    break;
                case "2":
                    if (currentCampaign.RecapSnapshots.Count > 0)
                    {
                        await _standardOutput.WriteLineAsync("Recap snapshots:");
                        foreach (var snapshot in currentCampaign.RecapSnapshots.OrderBy(snapshot => snapshot.CreatedUtc))
                        {
                            await _standardOutput.WriteLineAsync($"[{snapshot.CreatedUtc:u}] {snapshot.Summary}");
                        }

                        await _standardOutput.WriteLineAsync(string.Empty);
                    }

                    foreach (var entry in currentCampaign.Journal.OrderBy(entry => entry.TimestampUtc))
                    {
                        await _standardOutput.WriteLineAsync($"[{entry.TimestampUtc:u}] {entry.Category}: {entry.Text}");
                    }

                    await _standardOutput.WriteLineAsync(string.Empty);
                    await _standardOutput.WriteLineAsync(await _narrator.DescribeJournalAsync(currentCampaign, currentCampaign.Journal, cancellationToken));

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
                    await _screenRenderer.WriteNarratedBlockAsync("Quest Update", currentCampaign, narration);

                    if (currentCampaign.ActiveQuest.Stage == QuestStage.ReturnedToCaptain)
                    {
                        await _standardOutput.WriteLineAsync(string.Empty);
                        await _standardOutput.WriteLineAsync(await _narrator.DescribeNpcDialogueAsync(
                            currentCampaign,
                            "Captain Elira",
                            "You have returned with proof that the watchtower is secure.",
                            cancellationToken));
                    }

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
            await _screenRenderer.WriteEncounterScreenAsync(currentCampaign);

            var hasCombatItems = GetCombatUsableItems(currentCampaign).Count > 0;

            var selection = await PromptAsync("Choose a combat action", cancellationToken);
            if ((!hasCombatItems && selection == "4") || (hasCombatItems && selection == "5") || selection is null)
            {
                return currentCampaign;
            }

            if (hasCombatItems && selection == "4")
            {
                var itemResult = await ResolveCombatItemTurnAsync(currentCampaign, cancellationToken);
                currentCampaign = itemResult.Campaign;
                await _storage.SaveAsync(currentCampaign, cancellationToken);
                await _screenRenderer.WriteNarratedBlockAsync("Combat", currentCampaign, await _narrator.DescribeCombatResolutionAsync(currentCampaign, itemResult.Summary, cancellationToken));
                continue;
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
                await _standardOutput.WriteLineAsync(hasCombatItems ? "Enter 1, 2, 3, 4, or 5." : "Enter 1, 2, 3, or 4.");
                continue;
            }

            var result = CombatResolutionService.ResolveTurn(currentCampaign, action.Value);
            currentCampaign = result.Campaign;
            await _storage.SaveAsync(currentCampaign, cancellationToken);

            var narration = await _narrator.DescribeCombatResolutionAsync(currentCampaign, result.Summary, cancellationToken);
            await _screenRenderer.WriteNarratedBlockAsync("Combat", currentCampaign, narration);
        }

        return currentCampaign;
    }

    private async Task<CampaignUpdateResult> ResolveCombatItemTurnAsync(CampaignState campaign, CancellationToken cancellationToken)
    {
        var combatItems = GetCombatUsableItems(campaign);
        if (combatItems.Count == 0)
        {
            return new CampaignUpdateResult(campaign, "No combat-safe items are available.", false);
        }

        await _standardOutput.WriteLineAsync("Items:");
        for (var index = 0; index < combatItems.Count; index++)
        {
            var item = combatItems[index];
            var suffix = item.Quantity > 1 ? $" x{item.Quantity}" : string.Empty;
            await _standardOutput.WriteLineAsync($"{index + 1}. {item.Name}{suffix}");
        }

        var selection = await PromptAsync("Choose an item", cancellationToken);
        if (!int.TryParse(selection, out var selectedIndex) || selectedIndex < 1 || selectedIndex > combatItems.Count)
        {
            return new CampaignUpdateResult(campaign, "Choose a valid combat item.", false);
        }

        return CombatResolutionService.UseCombatItem(campaign, combatItems[selectedIndex - 1].ItemId);
    }

    private static IReadOnlyList<InventoryItem> GetCombatUsableItems(CampaignState campaign)
    {
        return campaign.Inventory
            .Where(static item => string.Equals(item.ItemId, "minor-potion", StringComparison.Ordinal)
                && string.Equals(item.Category, "consumable", StringComparison.OrdinalIgnoreCase)
                && item.Quantity > 0)
            .ToArray();
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

    private async Task WriteSaveSlotMetadataAsync(IReadOnlyList<SaveSlotMetadata> saveSlotMetadata, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _standardOutput.WriteLineAsync("Available saves:");

        foreach (var metadata in saveSlotMetadata)
        {
            var heroFragment = metadata.HeroClass is null
                ? metadata.HeroName
                : $"{metadata.HeroName} the {metadata.HeroClass}";
            var questFragment = metadata.QuestStage is null
                ? "stage unavailable"
                : $"stage {metadata.QuestStage}";
            var lastPlayedFragment = metadata.LastPlayedUtc is null
                ? "last played unavailable"
                : $"last played {metadata.LastPlayedUtc:yyyy-MM-dd HH:mm:ss}Z";

            await _standardOutput.WriteLineAsync($"- {metadata.SaveSlot}: {heroFragment}, {questFragment}, {metadata.Summary}, {lastPlayedFragment}");
        }
    }

    private async Task<string?> PromptAsync(string label, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _standardOutput.WriteAsync($"{label}: ");
        return await _standardInput.ReadLineAsync();
    }
}