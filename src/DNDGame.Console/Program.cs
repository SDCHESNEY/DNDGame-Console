using DNDGame.Console;
using DNDGame.Core.Services;
using DNDGame.Infrastructure.Narration;
using DNDGame.Infrastructure.Persistence;

try
{
	var settings = AppSettingsLoader.Load();
	var storage = new JsonCampaignStorage(settings.SaveDirectory);
	using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

	var deterministicNarrator = new DeterministicSceneNarrator(settings.LocalLlm);
	ISceneNarrator narrator = settings.EnableLocalLlmNarration
		? new FallbackSceneNarrator(new LocalLlmHttpSceneNarrator(httpClient, settings.LocalLlm), deterministicNarrator)
		: deterministicNarrator;

	var application = new ConsoleGameApplication(storage, narrator, System.Console.In, System.Console.Out, System.Console.Error);

	return await application.RunAsync(args);
}
catch (ConfigurationValidationException exception)
{
	await System.Console.Error.WriteLineAsync(exception.Message);
	return 1;
}
