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

    /// <summary>
    /// Overwrites shipped templates and reference files. Leaves constitution,
    /// feature.json, init-options.json, specs/, and host code alone.
    /// </summary>
    public static IReadOnlyList<string> RefreshWorkflow(string payloadRoot, string projectDir)
    {
        var source = Path.Combine(payloadRoot, "nuvyn");
        var dest = Path.Combine(projectDir, ".nuvyn");
        Directory.CreateDirectory(dest);

        var written = new List<string>();
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            if (IsUserOwnedWorkflowFile(relative))
                continue;

            var target = Path.Combine(dest, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
            written.Add(Path.Combine(".nuvyn", relative).Replace('\\', '/'));
        }

        Directory.CreateDirectory(Path.Combine(projectDir, "specs"));
        return written;
    }

    internal static bool IsUserOwnedWorkflowFile(string relative)
    {
        var name = Path.GetFileName(relative);
        var folder = Path.GetDirectoryName(relative);
        if (!string.IsNullOrEmpty(folder) && folder != ".")
            return false;

        return name.Equals("constitution.md", StringComparison.OrdinalIgnoreCase)
            || name.Equals("feature.json", StringComparison.OrdinalIgnoreCase)
            || name.Equals("init-options.json", StringComparison.OrdinalIgnoreCase)
            || name.Equals("adopt-report.md", StringComparison.OrdinalIgnoreCase);
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
