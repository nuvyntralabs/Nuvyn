namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public static class PayloadInstaller
{
    public static void InstallNuvynTree(string payloadRoot, string projectDir, string projectName)
    {
        var source = Path.Combine(payloadRoot, "nuvyn");
        var dest = Path.Combine(projectDir, ".nuvyn");
        CopyDirectory(source, dest);

        var constitution = Path.Combine(dest, "constitution.md");
        if (File.Exists(constitution))
        {
            var text = File.ReadAllText(constitution)
                .Replace("[NAME]", projectName, StringComparison.Ordinal)
                .Replace("[DATE]", DateTime.UtcNow.ToString("yyyy-MM-dd"), StringComparison.Ordinal);
            File.WriteAllText(constitution, text);
        }

        Directory.CreateDirectory(Path.Combine(projectDir, "specs"));
    }

    public static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            var target = Path.Combine(dest, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }
}
