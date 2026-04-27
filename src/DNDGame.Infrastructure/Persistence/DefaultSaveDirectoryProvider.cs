namespace DNDGame.Infrastructure.Persistence;

public static class DefaultSaveDirectoryProvider
{
    public static string GetSaveDirectory()
    {
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var baseDirectory = string.IsNullOrWhiteSpace(localApplicationData)
            ? AppContext.BaseDirectory
            : localApplicationData;

        return Path.Combine(baseDirectory, "DNDGame.Console", "saves");
    }
}