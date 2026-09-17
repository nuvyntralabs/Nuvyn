using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public sealed record HostScaffoldResult(bool Created, string Method, string? Warning);

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
            AddDefaultPackages(projectDir);
            MauiProgramPatcher.TryPatch(projectDir);
            return new(true, "uikit", null);
        }

        TryDotnet(["new", "install", "Plugin.Maui.MVVMExpress.Templates"], projectDir);

        if (TryDotnet(["new", "mvvmexpress", "-n", projectName, "-o", projectDir, "--force"], projectDir))
        {
            OverlayUiKitPages(payloadRoot, projectDir, projectName);
            AddDefaultPackages(projectDir);
            MauiProgramPatcher.TryPatch(projectDir);
            return new(true, "mvvmexpress",
                "Embedded Nuvyn host was missing. Used dotnet new mvvmexpress and overlaid UIKit pages.");
        }

        if (TryDotnet(["new", "maui", "-n", projectName, "-o", projectDir, "--force"], projectDir))
        {
            AddDefaultPackages(projectDir);
            MauiProgramPatcher.TryPatch(projectDir);
            return new(true, "maui",
                "Embedded Nuvyn host was missing. Used dotnet new maui and wired MVVMExpress + UIKit + HttpForge + FormValidation + KeyboardManager.");
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

    internal static void AddDefaultPackages(string projectDir)
    {
        var projects = Directory.Exists(projectDir)
            ? Directory.EnumerateFiles(projectDir, "*.csproj", SearchOption.AllDirectories).ToList()
            : [];

        if (projects.Count == 0)
        {
            foreach (var package in DefaultHostPackages.AfterHost)
                AddLatestPackage(projectDir, project: null, package);
            return;
        }

        foreach (var project in projects)
        {
            foreach (var package in DefaultHostPackages.ForProject(project))
                AddLatestPackage(projectDir, project, package);

            foreach (var package in ExistingNuvyntraPackages(project))
                AddLatestPackage(projectDir, project, package);
        }
    }

    /// <summary>No <c>--version</c> — nuget.org latest stable.</summary>
    internal static void AddLatestPackage(string workingDirectory, string? project, string package)
    {
        if (project is null)
            TryDotnet(["add", "package", package], workingDirectory);
        else
            TryDotnet(["add", project, "package", package], workingDirectory);
    }

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

    internal static bool TryDotnet(IEnumerable<string> args, string workingDirectory)
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

            if (!process.WaitForExit(180_000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
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
