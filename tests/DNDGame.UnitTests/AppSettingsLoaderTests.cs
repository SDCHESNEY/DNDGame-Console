using DNDGame.Console;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class AppSettingsLoaderTests
{
    [TestMethod]
    public void Load_LocalNarrationEnabledWithInvalidVerbosity_ThrowsConfigurationValidationException()
    {
        var settingsFilePath = CreateSettingsFile("""
            {
              "enableLocalLlmNarration": true,
              "localLlm": {
                "endpointUrl": "http://localhost:11434",
                "modelName": "local-dm",
                "narrativeVerbosity": "loud"
              }
            }
            """);

        try
        {
            var exception = Assert.ThrowsException<ConfigurationValidationException>(() => AppSettingsLoader.Load(settingsFilePath));

            StringAssert.Contains(exception.Message, "localLlm.narrativeVerbosity");
        }
        finally
        {
            File.Delete(settingsFilePath);
        }
    }

    private static string CreateSettingsFile(string json)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"dndgame-settings-{Guid.NewGuid():N}.json");
        File.WriteAllText(filePath, json);
        return filePath;
    }
}