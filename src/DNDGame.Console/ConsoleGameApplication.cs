using DNDGame.Core.Services;

namespace DNDGame.Console;

public sealed class ConsoleGameApplication
{
    private readonly ICampaignStorage _storage;
    private readonly ISceneNarrator _narrator;
    private readonly TextWriter _standardOutput;
    private readonly TextWriter _standardError;

    public ConsoleGameApplication(
        ICampaignStorage storage,
        ISceneNarrator narrator,
        TextWriter standardOutput,
        TextWriter standardError)
    {
        _storage = storage;
        _narrator = narrator;
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
            GameCommand.Help => await WriteHelpAsync(cancellationToken),
            GameCommand.New => await StartNewGameAsync(options, cancellationToken),
            GameCommand.Load => await LoadGameAsync(options, cancellationToken),
            _ => 2,
        };
    }

    private async Task<int> StartNewGameAsync(CommandLineOptions options, CancellationToken cancellationToken)
    {
        var campaign = DNDGame.Core.Services.NewCampaignFactory.Create(options.SaveSlot, options.HeroName, options.CharacterClass);
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
}