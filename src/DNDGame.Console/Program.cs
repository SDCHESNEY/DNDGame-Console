using DNDGame.Console;
using DNDGame.Infrastructure.Narration;
using DNDGame.Infrastructure.Persistence;

var settings = AppSettingsLoader.Load();
var storage = new JsonCampaignStorage(settings.SaveDirectory);
var narrator = new DeterministicSceneNarrator(settings.LocalLlm);
var application = new ConsoleGameApplication(storage, narrator, Console.Out, Console.Error);

return await application.RunAsync(args);
