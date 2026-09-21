using System.Text;

namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public sealed record AdoptLayer(string Area, string Found, string Keep, string DoNotAddUnlessAsked);

public sealed record AdoptInventoryResult(
    string HostCsproj,
    IReadOnlyList<string> MauiProjects,
    IReadOnlyList<AdoptLayer> Layers,
    IReadOnlyList<string> CatalogPackages,
    IReadOnlyList<string> Summary,
    bool HasUiKit,
    bool HasMvvmExpress,
    bool HasHttpForge);

/// <summary>
/// Read-only inventory of an existing MAUI tree. Warnings only — never HostProof.
/// </summary>
public static class AdoptInventory
{
    public static AdoptInventoryResult? TryInspect(string projectDir)
    {
        var hosts = FindMauiProjects(projectDir);
        if (hosts.Count == 0)
            return null;

        var host = hosts[0];
        var hostDir = Path.GetDirectoryName(host)!;
        var csprojText = File.ReadAllText(host);
        var ids = HostProof.PackageIds(csprojText).ToList();
        var program = ReadFirst(hostDir, "MauiProgram.cs") ?? ReadFirst(projectDir, "MauiProgram.cs") ?? "";
        var treeText = program + "\n" + ConcatMatching(projectDir, "*.xaml", 12) + "\n" +
                       ConcatMatching(projectDir, "*.cs", 20);

        var hasMvvmExpress = HasPackage(ids, "Plugin.Maui.MVVMExpress");
        var hasUiKit = HasPackage(ids, "NuvyntraLabs.UIKit");
        var hasHttpForge = HasPackage(ids, "Plugin.Maui.HttpForge");
        var catalog = ids.Where(DefaultHostPackages.IsNuvyntraPackage)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var architecture = DescribeArchitecture(ids, treeText, hasMvvmExpress);
        var chrome = DescribeChrome(projectDir, program);
        var ui = DescribeUi(ids, hasUiKit);
        var http = DescribeHttp(ids, treeText, hasHttpForge);

        var layers = new List<AdoptLayer>
        {
            new("Architecture", architecture.Found, architecture.Keep, architecture.DoNotAdd),
            new("Chrome", chrome.Found, chrome.Keep, chrome.DoNotAdd),
            new("UI", ui.Found, ui.Keep, ui.DoNotAdd),
            new("HTTP", http.Found, http.Keep, http.DoNotAdd),
        };

        var summary = new List<string>
        {
            $"MAUI host: {Path.GetRelativePath(projectDir, host).Replace('\\', '/')}",
            $"Architecture: {architecture.Found}",
            $"Chrome: {chrome.Found}",
            $"UI: {ui.Found}",
            $"HTTP: {http.Found}",
        };
        if (hosts.Count > 1)
            summary.Add($"{hosts.Count} UseMaui projects — inventoried the shallowest.");

        return new AdoptInventoryResult(
            host,
            hosts,
            layers,
            catalog,
            summary,
            hasUiKit,
            hasMvvmExpress,
            hasHttpForge);
    }

    public static string WriteReport(string projectDir, string appName, AdoptInventoryResult result)
    {
        var dest = Path.Combine(projectDir, ".nuvyn", "adopt-report.md");
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        var hostRel = Path.GetRelativePath(projectDir, result.HostCsproj).Replace('\\', '/');
        var catalog = result.CatalogPackages.Count == 0
            ? "_None._"
            : string.Join("\n", result.CatalogPackages.Select(id => $"- `{id}`"));

        var sb = new StringBuilder();
        sb.AppendLine($"# Adopt report — {appName}");
        sb.AppendLine();
        sb.AppendLine($"**Date:** {DateTime.UtcNow:yyyy-MM-dd}");
        sb.AppendLine($"**Host:** `{hostRel}`");
        sb.AppendLine();
        sb.AppendLine("Mode: **adopt**. Do not apply greenfield HostProof. `nuvyn adopt` did not change host code.");
        sb.AppendLine();
        sb.AppendLine("| Layer | Found | Keep (do not rewrite) | Do not add unless asked |");
        sb.AppendLine("| --- | --- | --- | --- |");
        foreach (var layer in result.Layers)
        {
            sb.AppendLine($"| {layer.Area} | {layer.Found} | {layer.Keep} | {layer.DoNotAddUnlessAsked} |");
        }

        sb.AppendLine();
        sb.AppendLine("## Catalog already present");
        sb.AppendLine();
        sb.AppendLine(catalog);
        sb.AppendLine();
        sb.AppendLine("## Standing law");
        sb.AppendLine();
        sb.AppendLine("- New work uses the **Keep** column (same MVVM, same UI kit, same HTTP).");
        sb.AppendLine("- Do not add MVVMExpress, Lumina / `NuvyntraLabs.UIKit`, or HttpForge unless that row already lists them or the user asks in this conversation.");
        sb.AppendLine("- If UIKit is already referenced, `NV*` is allowed on **new** screens only. Do not restyle existing pages.");
        sb.AppendLine("- Catalog plugins (`Plugin.Maui.*`) are additive only when the user names the capability.");
        sb.AppendLine("- `/nuvyn.plan` and `/nuvyn.implement` must read this file first.");
        sb.AppendLine();
        sb.AppendLine("Next: `/nuvyn.constitution` then `/nuvyn.specify` for the feature you want.");
        File.WriteAllText(dest, sb.ToString());
        return dest;
    }

    public static void AppendAdoptedConstitution(string projectDir)
    {
        var path = Path.Combine(projectDir, ".nuvyn", "constitution.md");
        if (!File.Exists(path))
            return;

        var text = File.ReadAllText(path);
        if (text.Contains("## Adopted host", StringComparison.Ordinal))
            return;

        var block =
            """

            ## Adopted host

            This app was attached with `nuvyn adopt`. Stack items **Default host**, **NavigationPage chrome**, and **Sleek Lumina UI** above do **not** apply.

            1. **Keep the existing architecture.** Do not add or migrate to MVVMExpress. Do not split into `.Core` or replace Shell / NavigationPage unless the user asks.
            2. **Keep the existing UI.** Do not introduce Lumina / `NV*` unless `NuvyntraLabs.UIKit` is already a PackageReference. If UIKit is present, new screens may use `NV*`; do not restyle existing pages.
            3. **Keep existing HTTP.** Do not add HttpForge or rewrite `HttpClient` / Refit calls unless the user asks.
            4. **Read `.nuvyn/adopt-report.md` first.** New work uses the Keep column. Catalog plugins are additive only when the user names them.

            """;

        var marker = "## App-specific principles";
        var at = text.IndexOf(marker, StringComparison.Ordinal);
        if (at >= 0)
            text = text.Insert(at, block + Environment.NewLine);
        else
            text += Environment.NewLine + block;

        File.WriteAllText(path, text);
    }

    internal static IReadOnlyList<string> FindMauiProjects(string projectDir)
    {
        if (!Directory.Exists(projectDir))
            return [];

        return Directory.EnumerateFiles(projectDir, "*.csproj", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("UseMaui", StringComparison.Ordinal))
            .OrderBy(path => path.Count(c => c is '/' or '\\'))
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static (string Found, string Keep, string DoNotAdd) DescribeArchitecture(
        IReadOnlyList<string> ids,
        string treeText,
        bool hasMvvmExpress)
    {
        if (hasMvvmExpress)
            return ("Plugin.Maui.MVVMExpress", "MVVMExpress (already present)", "A second MVVM framework");
        if (HasPackage(ids, "CommunityToolkit.Mvvm") || treeText.Contains("CommunityToolkit.Mvvm", StringComparison.Ordinal))
            return ("CommunityToolkit.Mvvm", "CommunityToolkit.Mvvm", "Plugin.Maui.MVVMExpress");
        if (ids.Any(id => id.StartsWith("Prism.", StringComparison.OrdinalIgnoreCase)))
            return ("Prism", "Prism", "Plugin.Maui.MVVMExpress");
        if (ids.Any(id => id.Contains("FreshMvvm", StringComparison.OrdinalIgnoreCase)))
            return ("FreshMvvm", "FreshMvvm", "Plugin.Maui.MVVMExpress");
        return ("Unknown / code-behind", "Current ViewModel / code-behind style", "Plugin.Maui.MVVMExpress");
    }

    private static (string Found, string Keep, string DoNotAdd) DescribeChrome(string projectDir, string program)
    {
        var hasShellFile = Directory.Exists(projectDir) &&
                           Directory.EnumerateFiles(projectDir, "AppShell.xaml", SearchOption.AllDirectories).Any();
        var usesShell = hasShellFile
                        || program.Contains("UseShell", StringComparison.Ordinal)
                        || program.Contains("AppShell", StringComparison.Ordinal);
        if (usesShell)
            return ("AppShell", "AppShell / existing chrome", "UseNavigationPage + Map (unless the user asks)");
        if (program.Contains("UseNavigationPage", StringComparison.Ordinal))
            return ("NavigationPage + Map", "NavigationPage + Map", "AppShell / .UseShell()");
        return ("Unknown", "Existing navigation", "Do not invent AppShell or UseNavigationPage");
    }

    private static (string Found, string Keep, string DoNotAdd) DescribeUi(IReadOnlyList<string> ids, bool hasUiKit)
    {
        if (hasUiKit)
            return ("NuvyntraLabs.UIKit", "Lumina on new screens only; existing pages stay", "Restyle existing pages to NV*");
        if (HasPackage(ids, "CommunityToolkit.Maui"))
            return ("CommunityToolkit.Maui", "Current CommunityToolkit / stock controls", "NuvyntraLabs.UIKit / Lumina");
        return ("Stock MAUI controls", "Current controls (Button / Label / Entry as used)", "NuvyntraLabs.UIKit / Lumina");
    }

    private static (string Found, string Keep, string DoNotAdd) DescribeHttp(
        IReadOnlyList<string> ids,
        string treeText,
        bool hasHttpForge)
    {
        if (hasHttpForge)
            return ("Plugin.Maui.HttpForge", "HttpForge (already present)", "Rewrite existing HttpClient call sites");
        if (HasPackage(ids, "Refit") || treeText.Contains("Refit.", StringComparison.Ordinal))
            return ("Refit", "Existing Refit clients", "Plugin.Maui.HttpForge");
        if (treeText.Contains("HttpClient", StringComparison.Ordinal) ||
            treeText.Contains("IHttpClientFactory", StringComparison.Ordinal))
            return ("HttpClient", "Existing HttpClient / IHttpClientFactory", "Plugin.Maui.HttpForge");
        return ("None detected", "Current HTTP style (do not invent HttpForge)", "Plugin.Maui.HttpForge");
    }

    private static bool HasPackage(IReadOnlyList<string> ids, string id) =>
        ids.Any(x => x.Equals(id, StringComparison.OrdinalIgnoreCase) ||
                     x.StartsWith(id + ".", StringComparison.OrdinalIgnoreCase));

    private static string? ReadFirst(string root, string fileName)
    {
        if (!Directory.Exists(root))
            return null;
        var path = Directory.EnumerateFiles(root, fileName, SearchOption.AllDirectories).FirstOrDefault();
        return path is null ? null : File.ReadAllText(path);
    }

    private static string ConcatMatching(string root, string pattern, int take)
    {
        if (!Directory.Exists(root))
            return "";
        var sb = new StringBuilder();
        foreach (var file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories).Take(take))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                continue;
            sb.AppendLine(File.ReadAllText(file));
        }

        return sb.ToString();
    }
}
