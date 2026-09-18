namespace NuvyntraLabs.Nuvyn.Cli.Infrastructure;

/// <summary>
/// Finds the app folder created by <c>nuvyn init</c> by walking up for <c>.nuvyn/</c>.
/// </summary>
public static class ProjectRoot
{
    public static string? TryFind(string? startDirectory = null)
    {
        var dir = new DirectoryInfo(string.IsNullOrWhiteSpace(startDirectory)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(startDirectory));

        while (dir is not null)
        {
            if (IsNuvynProject(dir.FullName))
                return dir.FullName;
            dir = dir.Parent;
        }

        return null;
    }

    public static bool IsNuvynProject(string directory)
    {
        var nuvyn = Path.Combine(directory, ".nuvyn");
        return Directory.Exists(nuvyn)
            && (File.Exists(Path.Combine(nuvyn, "constitution.md"))
                || File.Exists(Path.Combine(nuvyn, "init-options.json")));
    }
}
