using System.Diagnostics;

namespace DNDGame.UnitTests;

[TestClass]
public sealed class ConsoleProcessIntegrationTests
{
    [TestMethod]
    public async Task NewCommand_InvalidLocalLlmEndpoint_FailsFastWithClearError()
    {
        var saveDirectory = CreateTempDirectory();

        try
        {
            var result = await RunConsoleProcessAsync(
                ["new", "--slot", "invalid-llm", "--name", "Mira", "--class", "mage"],
                string.Empty,
                saveDirectory,
                enableLocalLlmNarration: true,
                llmEndpointUrl: "not-a-url");

            Assert.AreEqual(1, result.ExitCode, $"stdout:{Environment.NewLine}{result.StandardOutput}{Environment.NewLine}stderr:{Environment.NewLine}{result.StandardError}");
            StringAssert.Contains(result.StandardError, "localLlm.endpointUrl");
            Assert.IsFalse(File.Exists(Path.Combine(saveDirectory, "invalid-llm.json")));
        }
        finally
        {
            DeleteDirectory(saveDirectory);
        }
    }

    [TestMethod]
    public async Task Menu_NewGame_SaveAndQuit_CreatesSaveFile()
    {
        var saveDirectory = CreateTempDirectory();

        try
        {
            var result = await RunConsoleProcessAsync(
                ["menu"],
                "1\nmenu-save\nMira\nmage\n5\n6\n4\n",
                saveDirectory,
                enableLocalLlmNarration: false);

            Assert.AreEqual(0, result.ExitCode, result.StandardError);
            StringAssert.Contains(result.StandardOutput, "== Main Menu ==");
            StringAssert.Contains(result.StandardOutput, "== New Campaign ==");
            StringAssert.Contains(result.StandardOutput, "Campaign saved.");
            Assert.IsTrue(File.Exists(Path.Combine(saveDirectory, "menu-save.json")));
        }
        finally
        {
            DeleteDirectory(saveDirectory);
        }
    }

    [TestMethod]
    public async Task Menu_FullCombatLoop_CompletesQuestAndReturnsToCaptain()
    {
        var saveDirectory = CreateTempDirectory();

        try
        {
            var result = await RunConsoleProcessAsync(
                ["menu"],
                "1\nfull-loop\nMira\nfighter\n3\n4\n3\n3\n3\n3\n4\n3\n3\n3\n3\n3\n3\n6\n4\n",
                saveDirectory,
                enableLocalLlmNarration: false);

            Assert.AreEqual(0, result.ExitCode, result.StandardError);
            StringAssert.Contains(result.StandardOutput, "Hobgoblin Raider");
            StringAssert.Contains(result.StandardOutput, "Quest Stage: ReturnedToCaptain");
            StringAssert.Contains(result.StandardOutput, "Captain Elira says");
            Assert.IsTrue(File.Exists(Path.Combine(saveDirectory, "full-loop.json")));
        }
        finally
        {
            DeleteDirectory(saveDirectory);
        }
    }

    private static async Task<ProcessRunResult> RunConsoleProcessAsync(
        string[] args,
        string standardInput,
        string saveDirectory,
        bool enableLocalLlmNarration,
        string? llmEndpointUrl = null,
        string? llmModel = null,
        string? llmVerbosity = null)
    {
        var consoleAssemblyPath = typeof(DNDGame.Console.CommandLineOptions).Assembly.Location;
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory,
        };

        startInfo.ArgumentList.Add(consoleAssemblyPath);
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        startInfo.Environment["DNDGAME_SAVE_DIRECTORY"] = saveDirectory;
        startInfo.Environment["DNDGAME_ENABLE_LOCAL_LLM_NARRATION"] = enableLocalLlmNarration.ToString().ToLowerInvariant();

        if (llmEndpointUrl is not null)
        {
            startInfo.Environment["DNDGAME_LLM_ENDPOINT_URL"] = llmEndpointUrl;
        }

        if (llmModel is not null)
        {
            startInfo.Environment["DNDGAME_LLM_MODEL"] = llmModel;
        }

        if (llmVerbosity is not null)
        {
            startInfo.Environment["DNDGAME_LLM_VERBOSITY"] = llmVerbosity;
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start console process.");

        await process.StandardInput.WriteAsync(standardInput);
        process.StandardInput.Close();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessRunResult(
            process.ExitCode,
            await standardOutputTask,
            await standardErrorTask);
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"dndgame-integration-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError);
}