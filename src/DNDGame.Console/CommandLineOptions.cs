using DNDGame.Core.Models;

namespace DNDGame.Console;

public enum GameCommand
{
    Help,
    New,
    Load,
}

public sealed record CommandLineOptions(
    GameCommand Command,
    string SaveSlot,
    string HeroName,
    CharacterClass CharacterClass)
{
    public static bool TryParse(string[] args, out CommandLineOptions options, out string? error)
    {
        options = new CommandLineOptions(GameCommand.Help, "default", "Aria", CharacterClass.Fighter);
        error = null;

        if (args.Length == 0)
        {
            return true;
        }

        var command = args[0].ToLowerInvariant() switch
        {
            "new" => GameCommand.New,
            "load" => GameCommand.Load,
            "help" or "--help" or "-h" => GameCommand.Help,
            _ => (GameCommand?)null,
        };

        if (command is null)
        {
            error = $"Unknown command '{args[0]}'.";
            return false;
        }

        var saveSlot = "default";
        var heroName = "Aria";
        var characterClass = CharacterClass.Fighter;

        for (var index = 1; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length)
            {
                error = $"Missing value for option '{args[index]}'.";
                return false;
            }

            var value = args[index + 1];
            switch (args[index])
            {
                case "--slot":
                    saveSlot = value;
                    break;
                case "--name":
                    heroName = value;
                    break;
                case "--class":
                    if (!Enum.TryParse<CharacterClass>(value, true, out characterClass))
                    {
                        error = $"Unsupported class '{value}'. Supported values: fighter, ranger, mage.";
                        return false;
                    }

                    break;
                default:
                    error = $"Unknown option '{args[index]}'.";
                    return false;
            }
        }

        options = new CommandLineOptions(command.Value, saveSlot, heroName, characterClass);
        return true;
    }
}