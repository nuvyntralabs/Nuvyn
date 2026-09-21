using System.CommandLine;
using NuvyntraLabs.Nuvyn.Cli.Infrastructure;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using NuvyntraLabs.Nuvyn.Cli.Workflow;
using Spectre.Console;

namespace NuvyntraLabs.Nuvyn.Cli.Commands;

internal static class AdoptCommand
{
    public static Command Create()
    {
        var agentOpt = new Option<string>("--agent", "-a")
        {
            Description = "AI coding agent key (cursor, copilot, claude, gemini, codex, windsurf, …). Prompted when omitted.",
        };
        var pathOpt = new Option<string>("--path", "-p")
        {
            Description = "Existing MAUI app folder. Defaults to the current directory.",
        };

        var command = new Command(
            "adopt",
            "Attach the Nuvyn slash chain to an existing MAUI app. Does not change host code, MVVM, UI, or HTTP.")
        {
            agentOpt,
            pathOpt,
        };

        command.SetAction(parse => Run(parse.GetValue(agentOpt), parse.GetValue(pathOpt)));
        return command;
    }

    internal static int Run(string? agentId, string? path = null, string? startDirectory = null)
    {
        ConsoleUi.Banner();

        var projectDir = ResolveRoot(path, startDirectory);
        if (projectDir is null)
            return 1;

        var existing = ProjectRoot.TryFind(projectDir);
        if (existing is not null)
        {
            ConsoleUi.Error(
                $"{existing} is already a Nuvyn project. Use nuvyn update to refresh skills. Do not re-run nuvyn adopt or nuvyn init.");
            return 1;
        }

        var inventory = AdoptInventory.TryInspect(projectDir);
        if (inventory is null)
        {
            ConsoleUi.Error($"{projectDir} is not a MAUI app (no UseMaui csproj). nuvyn adopt only attaches an existing MAUI tree.");
            return 1;
        }

        var agent = InitCommand.ResolveAgent(agentId);
        if (agent is null)
            return 1;

        string payload;
        try
        {
            payload = PayloadRoot.Resolve();
        }
        catch (Exception ex)
        {
            ConsoleUi.Error(ex.Message);
            return 1;
        }

        var appName = new DirectoryInfo(projectDir).Name;
        var hostBefore = SnapshotHost(projectDir);

        ConsoleUi.Step(1, $"Project: {projectDir}");
        ConsoleUi.Step(2, $"Agent: {agent.DisplayName}");
        ConsoleUi.Step(3, "Inventory (read-only)…");
        foreach (var line in inventory.Summary)
            ConsoleUi.Ok(line);

        ConsoleUi.Step(4, "Installing .nuvyn templates…");
        PayloadInstaller.InstallNuvynTree(payload, projectDir, appName);
        AdoptInventory.AppendAdoptedConstitution(projectDir);
        AdoptInventory.WriteReport(projectDir, appName, inventory);
        ConsoleUi.Ok(".nuvyn/constitution.md (adopted host)");
        ConsoleUi.Ok(".nuvyn/adopt-report.md");

        ConsoleUi.Step(5, $"Writing {agent.DisplayName} commands…");
        var files = AgentInstaller.Install(payload, projectDir, agent);
        foreach (var file in files)
            ConsoleUi.Ok(file);

        var options = InitOptions.TryRead(projectDir) ?? InitOptions.Create(agent.Id);
        options.Agent = agent.Id;
        options.Mode = InitOptions.ModeAdopt;
        options.Cli = InitOptions.CliId;
        options.CliVersion = VersionCommand.GetVersion();
        options.Updated = DateTime.UtcNow.ToString("yyyy-MM-dd");
        InitOptions.Write(projectDir, options);

        if (!HostUnchanged(hostBefore, SnapshotHost(projectDir)))
        {
            ConsoleUi.Error("Adopt wrote host files. That is a bug — host code must stay unchanged.");
            return 1;
        }

        ConsoleUi.Step(6, "MauiDev doctor…");
        var doctor = MauiDevCompanion.RunDoctor(projectDir);
        MauiDevCompanion.Write(doctor);

        AnsiConsole.WriteLine();
        ConsoleUi.Ok("Host code, csproj, pages, and HTTP call sites were left untouched.");
        AnsiConsole.MarkupLine("[bold]Next[/]");
        AnsiConsole.MarkupLine("  Open this folder and read [cyan].nuvyn/adopt-report.md[/]");
        AnsiConsole.MarkupLine($"  In {ConsoleUi.Escape(agent.DisplayName)} run:");
        foreach (var slash in NuvynCommands.Slash)
            AnsiConsole.MarkupLine($"    [cyan]{slash}[/]");
        AnsiConsole.MarkupLine("  New work keeps the existing MVVM, UI kit, and HttpClient.");
        AnsiConsole.MarkupLine("  Later: [cyan]nuvyn update[/] refreshes skills without overlaying host code.");
        if (doctor.Status == MauiDevCompanionStatus.Missing)
            AnsiConsole.MarkupLine("  Diagnose the tree: [cyan]maui-dev doctor[/] after installing Plugin.Maui.MauiDev.Cli.");

        return 0;
    }

    internal static string? ResolveRoot(string? path, string? startDirectory)
    {
        var start = string.IsNullOrWhiteSpace(path)
            ? (string.IsNullOrWhiteSpace(startDirectory) ? Directory.GetCurrentDirectory() : startDirectory)
            : path;

        var full = Path.GetFullPath(start);
        if (File.Exists(full) && !Directory.Exists(full))
        {
            ConsoleUi.Error($"{full} is a file. Pass a MAUI app folder to nuvyn adopt --path.");
            return null;
        }

        if (!Directory.Exists(full))
        {
            ConsoleUi.Error($"{full} does not exist.");
            return null;
        }

        return full;
    }

    internal static Dictionary<string, string> SnapshotHost(string projectDir)
    {
        var snapshot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(projectDir))
            return snapshot;

        foreach (var file in Directory.EnumerateFiles(projectDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(projectDir, file).Replace('\\', '/');
            if (relative.StartsWith('.') ||
                relative.StartsWith("specs/", StringComparison.OrdinalIgnoreCase) ||
                relative.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
                relative.Contains("/bin/", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                snapshot[relative] = File.ReadAllText(file);
            }
            catch
            {
                /* skip locked / binary */
            }
        }

        return snapshot;
    }

    internal static bool HostUnchanged(Dictionary<string, string> before, Dictionary<string, string> after)
    {
        if (before.Count != after.Count)
            return false;

        foreach (var (key, value) in before)
        {
            if (!after.TryGetValue(key, out var next) || next != value)
                return false;
        }

        return true;
    }
}
