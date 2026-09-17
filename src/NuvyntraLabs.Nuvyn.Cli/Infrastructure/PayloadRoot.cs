namespace NuvyntraLabs.Nuvyn.Cli.Infrastructure;

public static class PayloadRoot
{
    public static string Resolve()
    {
        foreach (var candidate in Candidates())
        {
            if (Directory.Exists(Path.Combine(candidate, "nuvyn")) &&
                Directory.Exists(Path.Combine(candidate, "commands")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            "Nuvyn payload was not found next to the CLI. Reinstall NuvyntraLabs.Nuvyn.Cli.");
    }

    private static IEnumerable<string> Candidates()
    {
        var baseDir = AppContext.BaseDirectory;
        yield return Path.Combine(baseDir, "payload");

        var dir = new DirectoryInfo(baseDir);
        for (var i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
            yield return Path.Combine(dir.FullName, "payload");
    }
}
