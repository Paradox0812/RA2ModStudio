namespace RA2IniEditor.Tests;

internal static class TestRepositoryRoot
{
    public static string Find()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RA2IniEditor.sln")) ||
                File.Exists(Path.Combine(directory.FullName, "RA2IniEditor.IDE.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root not found.");
    }
}
