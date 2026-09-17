namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

/// <summary>
/// Copies the embedded MVVMExpress host (UIKit pages) and rewrites <c>MauiApp1</c> to the project name.
/// </summary>
internal static class HostTemplate
{
    internal const string Token = "MauiApp1";

    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".xaml", ".csproj", ".sln", ".json", ".props", ".md", ".plist",
        ".xml", ".manifest", ".txt", ".config", ".xcprivacy",
    };

    public static string? SourceDirectory(string payloadRoot)
    {
        var host = Path.Combine(payloadRoot, "host");
        return Directory.Exists(Path.Combine(host, Token)) ? host : null;
    }

    public static bool TryInstall(string payloadRoot, string projectDir, string projectName)
    {
        var source = SourceDirectory(payloadRoot);
        if (source is null)
            return false;

        Directory.CreateDirectory(projectDir);

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            if (relative.Contains(".template.config", StringComparison.Ordinal))
                continue;

            var destRelative = RewriteToken(relative, projectName);
            var dest = Path.Combine(projectDir, destRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

            if (IsText(file))
            {
                var text = File.ReadAllText(file);
                File.WriteAllText(dest, RewriteToken(text, projectName));
            }
            else
            {
                File.Copy(file, dest, overwrite: true);
            }
        }

        return Directory.Exists(Path.Combine(projectDir, projectName));
    }

    internal static string RewriteToken(string value, string projectName)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace(Token, projectName, StringComparison.Ordinal)
            .Replace(Token.ToLowerInvariant(), projectName.ToLowerInvariant(), StringComparison.Ordinal);
    }

    private static bool IsText(string path)
        => TextExtensions.Contains(Path.GetExtension(path));
}
