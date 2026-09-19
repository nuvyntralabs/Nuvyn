using System.Diagnostics;

namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public enum MauiDevCompanionStatus
{
    Missing,
    BelowFloor,
    Found,
    DoctorPassed,
    DoctorIssues,
    DoctorFailedToStart,
}

public sealed record MauiDevCompanionResult(
    MauiDevCompanionStatus Status,
    string? Version,
    string Message,
    int? DoctorExitCode = null,
    string? Detail = null);

public sealed class MauiDevCompanionOptions
{
    public Func<string?>? FindCommand { get; init; }
    public Func<string?>? ListGlobalTools { get; init; }
    public Func<string, IReadOnlyList<string>, int>? Run { get; init; }
}

/// <summary>
/// PATH hand-off to <c>maui-dev doctor</c>. No ProjectReference. Missing tool never fails Nuvyn.
/// </summary>
public static class MauiDevCompanion
{
    public const string CommandName = "maui-dev";
    public const string PackageId = "Plugin.Maui.MauiDev.Cli";
    public static readonly Version MinVersion = new(1, 2, 0);
    public const string InstallHint =
        "dotnet tool install -g Plugin.Maui.MauiDev.Cli --source https://api.nuget.org/v3/index.json";
    public const string UpdateHint =
        "dotnet tool update -g Plugin.Maui.MauiDev.Cli --source https://api.nuget.org/v3/index.json";

    public static MauiDevCompanionResult Inspect(MauiDevCompanionOptions? options = null)
    {
        try
        {
            return InspectCore(options ?? new());
        }
        catch
        {
            return new(MauiDevCompanionStatus.Missing, null,
                "maui-dev was not found. Nuvyn still created the app. Install MauiDev to diagnose the tree.");
        }
    }

    public static MauiDevCompanionResult RunDoctor(string projectDir, MauiDevCompanionOptions? options = null)
    {
        try
        {
            return RunDoctorCore(projectDir, options ?? new());
        }
        catch
        {
            return new(MauiDevCompanionStatus.DoctorFailedToStart, null,
                "maui-dev doctor could not start. Run it yourself after install.");
        }
    }

    public static void Write(MauiDevCompanionResult result)
    {
        switch (result.Status)
        {
            case MauiDevCompanionStatus.Found:
            case MauiDevCompanionStatus.DoctorPassed:
                ConsoleUi.Ok(result.Message);
                break;
            case MauiDevCompanionStatus.Missing:
            case MauiDevCompanionStatus.BelowFloor:
            case MauiDevCompanionStatus.DoctorIssues:
            case MauiDevCompanionStatus.DoctorFailedToStart:
                ConsoleUi.Warn(result.Message);
                if (result.Status == MauiDevCompanionStatus.Missing)
                    ConsoleUi.Info(InstallHint);
                if (result.Status == MauiDevCompanionStatus.BelowFloor)
                    ConsoleUi.Info(UpdateHint);
                break;
        }

        if (!string.IsNullOrWhiteSpace(result.Detail))
            Console.Out.WriteLine(result.Detail);
    }

    public static bool TryReadListedVersion(string toolListOutput, out Version version)
    {
        version = new Version(0, 0);
        foreach (var raw in toolListOutput.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                continue;

            var id = parts[0];
            var isMauiDev = id.Equals(PackageId, StringComparison.OrdinalIgnoreCase)
                || parts.Any(part => part.Equals(CommandName, StringComparison.OrdinalIgnoreCase));
            if (!isMauiDev)
                continue;

            var normalized = ToolUpdateCheck.NormalizeVersion(parts[1]);
            if (Version.TryParse(normalized, out var parsed))
            {
                version = parsed;
                return true;
            }
        }

        return false;
    }

    static MauiDevCompanionResult InspectCore(MauiDevCompanionOptions options)
    {
        var command = (options.FindCommand ?? FindOnPath)();
        if (string.IsNullOrWhiteSpace(command))
        {
            return new(MauiDevCompanionStatus.Missing, null,
                "maui-dev is not on PATH. Nuvyn created the host; MauiDev diagnoses it.");
        }

        var listed = (options.ListGlobalTools ?? ListGlobalTools)();
        if (!string.IsNullOrWhiteSpace(listed) && TryReadListedVersion(listed, out var version))
        {
            if (version < MinVersion)
            {
                return new(MauiDevCompanionStatus.BelowFloor, version.ToString(),
                    $"nuvyn expects maui-dev {MinVersion}+ (found {version}). Doctor will still run.");
            }

            return new(MauiDevCompanionStatus.Found, version.ToString(),
                $"maui-dev {version} is installed.");
        }

        return new(MauiDevCompanionStatus.Found, null, "maui-dev is on PATH.");
    }

    static MauiDevCompanionResult RunDoctorCore(string projectDir, MauiDevCompanionOptions options)
    {
        var inspect = InspectCore(options);
        if (inspect.Status == MauiDevCompanionStatus.Missing)
            return inspect;

        var command = (options.FindCommand ?? FindOnPath)() ?? CommandName;
        var args = (IReadOnlyList<string>)["doctor", "--path", projectDir];
        int exit;
        var output = "";
        try
        {
            if (options.Run is { } custom)
            {
                exit = custom(command, args);
            }
            else
            {
                exit = RunProcess(command, args, out output);
            }
        }
        catch
        {
            return new(MauiDevCompanionStatus.DoctorFailedToStart, inspect.Version,
                "maui-dev doctor could not start. Run: maui-dev doctor");
        }

        if (exit == 0)
        {
            if (inspect.Status == MauiDevCompanionStatus.BelowFloor)
            {
                return new(MauiDevCompanionStatus.BelowFloor, inspect.Version,
                    $"{inspect.Message} maui-dev doctor passed.", 0);
            }

            var version = inspect.Version is null ? "maui-dev doctor" : $"maui-dev {inspect.Version} doctor";
            return new(MauiDevCompanionStatus.DoctorPassed, inspect.Version, $"{version} passed.", 0);
        }

        var detail = TrimDoctorOutput(output);
        if (inspect.Status == MauiDevCompanionStatus.BelowFloor)
        {
            return new(MauiDevCompanionStatus.BelowFloor, inspect.Version,
                $"{inspect.Message} maui-dev doctor exited {exit}.", exit, detail);
        }

        return new(MauiDevCompanionStatus.DoctorIssues, inspect.Version,
            $"maui-dev doctor exited {exit}. Fix those findings, then re-run maui-dev doctor.", exit, detail);
    }

    internal static string? FindOnPath()
    {
        var file = OperatingSystem.IsWindows() ? CommandName + ".exe" : CommandName;
        var path = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(path))
        {
            foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = Path.Combine(dir, file);
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        var homeTools = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotnet", "tools", file);
        return File.Exists(homeTools) ? homeTools : null;
    }

    static string? ListGlobalTools()
    {
        try
        {
            var start = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            start.ArgumentList.Add("tool");
            start.ArgumentList.Add("list");
            start.ArgumentList.Add("-g");
            using var process = Process.Start(start);
            if (process is null)
                return null;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return output;
        }
        catch
        {
            return null;
        }
    }

    static int RunProcess(string fileName, IReadOnlyList<string> arguments, out string output)
    {
        output = "";
        var start = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start);
        if (process is null)
            return 2;

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        if (!process.WaitForExit((int)TimeSpan.FromMinutes(2).TotalMilliseconds))
        {
            try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return 2;
        }

        output = string.IsNullOrWhiteSpace(stderr) ? stdout : stdout + Environment.NewLine + stderr;
        return process.ExitCode;
    }

    static string? TrimDoctorOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return null;

        var lines = output.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 24)
            return string.Join(Environment.NewLine, lines);

        return string.Join(Environment.NewLine, lines.TakeLast(24));
    }
}
