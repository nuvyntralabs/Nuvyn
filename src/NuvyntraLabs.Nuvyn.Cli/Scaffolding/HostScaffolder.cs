using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public sealed record HostScaffoldResult(
    bool Created,
    string Method,
    string? Warning,
    IReadOnlyList<string>? FailedPackages = null);

public static class HostScaffolder
{
    public static HostScaffoldResult Scaffold(string projectDir, string projectName, bool skipHost, string? payloadRoot = null)
    {
        if (skipHost)
            return new(false, "skipped", "Host scaffold skipped (--skip-host).");

        Directory.CreateDirectory(projectDir);

        if (!string.IsNullOrWhiteSpace(payloadRoot) &&
            HostTemplate.TryInstall(payloadRoot, projectDir, projectName))
        {
            var failed = AddDefaultPackages(projectDir);
            ClearRestoreArtifacts(projectDir);
            MauiProgramPatcher.TryPatch(projectDir);
            return new(true, "uikit", PackageWarning(failed), failed);
        }

        TryDotnet(["new", "install", "Plugin.Maui.MVVMExpress.Templates"], projectDir);

        if (TryDotnet(["new", "mvvmexpress", "-n", projectName, "-o", projectDir, "--force"], projectDir))
        {
            OverlayUiKitPages(payloadRoot, projectDir, projectName);
            var failed = AddDefaultPackages(projectDir);
            ClearRestoreArtifacts(projectDir);
            MauiProgramPatcher.TryPatch(projectDir);
            return new(true, "mvvmexpress",
                FirstMessage(
                    "Embedded Nuvyn host was missing. Used dotnet new mvvmexpress and overlaid UIKit pages.",
                    PackageWarning(failed)),
                failed);
        }

        if (TryDotnet(["new", "maui", "-n", projectName, "-o", projectDir, "--force"], projectDir))
        {
            var failed = AddDefaultPackages(projectDir);
            ClearRestoreArtifacts(projectDir);
            MauiProgramPatcher.TryPatch(projectDir);
            return new(true, "maui",
                FirstMessage(
                    "Embedded Nuvyn host was missing. Used dotnet new maui and wired MVVMExpress + UIKit + HttpForge + FormValidation + KeyboardManager.",
                    PackageWarning(failed)),
                failed);
        }

        WriteFallbackReadme(projectDir, projectName);
        return new(false, "none",
            "Could not copy the embedded UIKit host or run 'dotnet new'. Workflow files were still installed. See HOST.md.");
    }

    internal static void OverlayUiKitPages(string? payloadRoot, string projectDir, string projectName)
    {
        if (string.IsNullOrWhiteSpace(payloadRoot))
            return;

        var sourcePages = Path.Combine(payloadRoot, "host", HostTemplate.Token, "Pages");
        if (!Directory.Exists(sourcePages))
            return;

        var destPages = Directory.EnumerateDirectories(projectDir, "Pages", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (destPages is null)
            return;

        var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.GetFiles(sourcePages, "*.xaml"))
        {
            var name = Path.GetFileName(file);
            keep.Add(name);
            keep.Add(name + ".cs");
            var dest = Path.Combine(destPages, name);
            File.WriteAllText(dest, HostTemplate.RewriteToken(File.ReadAllText(file), projectName));
        }

        foreach (var extra in Directory.GetFiles(destPages))
        {
            if (!keep.Contains(Path.GetFileName(extra)))
                File.Delete(extra);
        }
    }

    internal static IReadOnlyList<string> AddDefaultPackages(string projectDir)
    {
        var failed = new List<string>();
        var projects = Directory.Exists(projectDir)
            ? Directory.EnumerateFiles(projectDir, "*.csproj", SearchOption.AllDirectories).ToList()
            : [];

        if (projects.Count == 0)
        {
            foreach (var package in DefaultHostPackages.AfterHost)
            {
                if (!AddLatestPackage(projectDir, project: null, package))
                    failed.Add(package);
            }

            return failed;
        }

        foreach (var project in projects)
        {
            var planned = DefaultHostPackages.ForProject(project);
            var existing = ExistingNuvyntraPackages(project)
                .Where(id => !planned.Contains(id, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var package in planned.Concat(existing))
            {
                if (!AddLatestPackage(projectDir, project, package))
                    failed.Add($"{package} ({Path.GetFileName(project)})");
            }
        }

        return failed;
    }

    /// <summary>
    /// Writes nuget.org's latest stable version into the csproj without
    /// <c>dotnet add</c>. That command writes a partial <c>project.assets.json</c>
    /// when restore is skipped, and a full restore pulls iOS / Mac Catalyst TFMs
    /// on Linux CI (android workload only).
    /// </summary>
    internal static bool AddLatestPackage(string workingDirectory, string? project, string package)
    {
        if (!TryGetLatestStableVersion(package, out var version))
            return false;

        var csproj = project;
        if (string.IsNullOrWhiteSpace(csproj))
        {
            csproj = Directory.Exists(workingDirectory)
                ? Directory.EnumerateFiles(workingDirectory, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault()
                : null;
        }

        return csproj is not null && TryWritePackageReference(csproj, package, version);
    }

    internal static bool TryWritePackageReference(string projectPath, string package, string version)
    {
        if (!File.Exists(projectPath) || string.IsNullOrWhiteSpace(package) || string.IsNullOrWhiteSpace(version))
            return false;

        var text = File.ReadAllText(projectPath);
        var replacement = $@"<PackageReference Include=""{package}"" Version=""{version}"" />";
        var existing = new Regex(
            $@"<PackageReference\s+Include=""{Regex.Escape(package)}""(?:\s+Version=""[^""]*"")?\s*(?:/>|>\s*(?:<Version>[^<]*</Version>\s*)?</PackageReference>)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (existing.IsMatch(text))
        {
            text = existing.Replace(text, replacement, 1);
        }
        else
        {
            var itemGroup = new Regex(@"(<ItemGroup>\s*)(<PackageReference\s)", RegexOptions.IgnoreCase);
            if (itemGroup.IsMatch(text))
            {
                text = itemGroup.Replace(text, $"$1{replacement}\n    $2", 1);
            }
            else
            {
                var close = text.LastIndexOf("</Project>", StringComparison.OrdinalIgnoreCase);
                if (close < 0)
                    return false;
                text = text.Insert(close, $"  <ItemGroup>\n    {replacement}\n  </ItemGroup>\n");
            }
        }

        File.WriteAllText(projectPath, text);
        return true;
    }

    internal static void ClearRestoreArtifacts(string projectDir)
    {
        if (!Directory.Exists(projectDir))
            return;

        foreach (var name in new[] { "obj", "bin" })
        {
            foreach (var dir in Directory.EnumerateDirectories(projectDir, name, SearchOption.AllDirectories).ToList())
                try { Directory.Delete(dir, recursive: true); } catch { /* locked obj */ }
        }
    }

    internal static bool TryGetLatestStableVersion(string packageId, out string version)
    {
        version = "";
        try
        {
            var json = NugetHttp.GetStringAsync(FlatContainerIndex(packageId)).GetAwaiter().GetResult();
            return TryReadLatestStableVersion(json, out version);
        }
        catch
        {
            return false;
        }
    }

    internal static bool TryReadLatestStableVersion(string flatContainerJson, out string version)
    {
        version = "";
        using var doc = JsonDocument.Parse(flatContainerJson);
        if (!doc.RootElement.TryGetProperty("versions", out var versions))
            return false;

        string? latest = null;
        foreach (var item in versions.EnumerateArray())
        {
            var value = item.GetString();
            if (string.IsNullOrEmpty(value) || value.Contains('-', StringComparison.Ordinal))
                continue;
            latest = value;
        }

        if (latest is null)
            return false;

        version = latest;
        return true;
    }

    private static readonly HttpClient NugetHttp = new()
    {
        BaseAddress = new Uri("https://api.nuget.org/"),
        Timeout = TimeSpan.FromSeconds(30),
    };

    private static string FlatContainerIndex(string packageId) =>
        $"v3-flatcontainer/{packageId.ToLowerInvariant()}/index.json";

    private static string? PackageWarning(IReadOnlyList<string> failed) =>
        failed.Count == 0
            ? null
            : "Could not add from nuget.org: " + string.Join(", ", failed);

    private static string? FirstMessage(string primary, string? extra) =>
        extra is null ? primary : primary + " " + extra;

    internal static IEnumerable<string> ExistingNuvyntraPackages(string projectPath)
    {
        var text = File.ReadAllText(projectPath);
        foreach (Match match in Regex.Matches(text, @"<PackageReference\s+Include=""([^""]+)""", RegexOptions.IgnoreCase))
        {
            var id = match.Groups[1].Value;
            if (DefaultHostPackages.IsNuvyntraPackage(id))
                yield return id;
        }
    }

    internal static string? FindMauiProject(string projectDir)
    {
        return Directory.EnumerateFiles(projectDir, "*.csproj", SearchOption.AllDirectories)
            .FirstOrDefault(path =>
            {
                var text = File.ReadAllText(path);
                return text.Contains("UseMaui", StringComparison.Ordinal);
            });
    }

    internal static bool TryDotnet(IEnumerable<string> args, string workingDirectory) =>
        TryDotnet(args, workingDirectory, TimeSpan.FromMinutes(3));

    internal static bool TryDotnet(IEnumerable<string> args, string workingDirectory, TimeSpan timeout)
    {
        try
        {
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = Directory.Exists(workingDirectory)
                    ? workingDirectory
                    : Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var arg in args)
                start.ArgumentList.Add(arg);

            using var process = Process.Start(start);
            if (process is null)
                return false;

            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                return false;
            }

            Task.WaitAll(stdout, stderr);
            if (process.ExitCode == 0)
                return true;

            WriteDotnetFailure(args, stdout.Result, stderr.Result);
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static void WriteDotnetFailure(IEnumerable<string> args, string stdout, string stderr)
    {
        var text = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
        text = text.Trim();
        if (text.Length == 0)
            return;

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var snippet = string.Join(" | ", lines.TakeLast(4));
        if (snippet.Length > 400)
            snippet = snippet[^400..];
        Console.Error.WriteLine($"dotnet {string.Join(' ', args)}: {snippet}");
    }

    private static void WriteFallbackReadme(string projectDir, string projectName)
    {
        var packages = string.Join('\n', DefaultHostPackages.AfterHost.Select(p => $"dotnet add package {p}"));
        File.WriteAllText(
            Path.Combine(projectDir, "HOST.md"),
            $"""
            # Host not scaffolded

            Re-run `nuvyn init {projectName}` from a machine that has the Nuvyn payload, or:

            ```bash
            dotnet new install Plugin.Maui.MVVMExpress.Templates
            dotnet new mvvmexpress -n {projectName} -o . --force
            {packages}
            ```

            Then replace Pages/*.xaml with Lumina `NV*` recipes.
            """);
    }
}
