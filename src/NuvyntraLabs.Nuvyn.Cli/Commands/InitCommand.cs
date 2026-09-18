using System.CommandLine;
using System.Text.RegularExpressions;
using NuvyntraLabs.Nuvyn.Cli.Agents;
using NuvyntraLabs.Nuvyn.Cli.Infrastructure;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using NuvyntraLabs.Nuvyn.Cli.Workflow;
using Spectre.Console;

namespace NuvyntraLabs.Nuvyn.Cli.Commands;

internal static class InitCommand
{
    public static Command Create()
    {
        var nameArg = new Argument<string?>("project_name")
        {
            Description = "New project folder name. Always creates a new directory.",
            Arity = ArgumentArity.ZeroOrOne,
        };
        var agentOpt = new Option<string>("--agent", "-a")
        {
            Description = "AI coding agent key (cursor, copilot, claude, gemini, codex, windsurf, …). Prompted when omitted.",
        };
        var skipHostOpt = new Option<bool>("--skip-host")
        {
            Description = "Install workflow files only (no dotnet new).",
            Hidden = true,
        };

        var command = new Command("init", "Create a new Nuvyntra MAUI app and install the spec-driven agent workflow.")
        {
            nameArg,
            agentOpt,
            skipHostOpt,
        };

        command.SetAction(parse =>
        {
            var projectName = parse.GetValue(nameArg);
            var agentId = parse.GetValue(agentOpt);
            var skipHost = parse.GetValue(skipHostOpt);
            return Run(projectName, agentId, skipHost);
        });

        return command;
    }

    internal static int Run(string? projectName, string? agentId, bool skipHost)
    {
        ConsoleUi.Banner();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            if (!AnsiConsole.Profile.Capabilities.Interactive)
            {
                ConsoleUi.Error("Pass a new project name, for example: nuvyn init ClinicApp");
                return 1;
            }

            projectName = AnsiConsole.Ask<string>("New project name:");
        }

        projectName = projectName.Trim();

        if (!Regex.IsMatch(projectName, @"^[A-Za-z][A-Za-z0-9._-]{0,63}$"))
        {
            ConsoleUi.Error("Project name must start with a letter and use letters, digits, '.', '_' or '-'.");
            return 1;
        }

        var agent = ResolveAgent(agentId);
        if (agent is null)
            return 1;

        var projectDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), projectName));
        if (Directory.Exists(projectDir) || File.Exists(projectDir))
        {
            ConsoleUi.Error($"{projectDir} already exists. nuvyn init only creates a new project. Pick another name.");
            return 1;
        }

        Directory.CreateDirectory(projectDir);
        ConsoleUi.Step(1, $"Project: {projectDir}");
        ConsoleUi.Step(2, $"Agent: {agent.DisplayName}");

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

        ConsoleUi.Step(3, "Scaffolding MVVMExpress host with UIKit pages…");
        var host = HostScaffolder.Scaffold(projectDir, projectName, skipHost, payload);
        if (host.Created)
            ConsoleUi.Ok($"Host: {host.Method}");
        if (host.Warning is not null)
            ConsoleUi.Warn(host.Warning);

        ConsoleUi.Step(4, "Installing .nuvyn templates…");
        PayloadInstaller.InstallNuvynTree(payload, projectDir, projectName);
        ConsoleUi.Ok(".nuvyn/constitution.md");

        ConsoleUi.Step(5, $"Writing {agent.DisplayName} commands…");
        var files = AgentInstaller.Install(payload, projectDir, agent);
        foreach (var file in files)
            ConsoleUi.Ok(file);

        ProjectReadme.Write(projectDir, projectName, agent);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Next[/]");
        AnsiConsole.MarkupLine($"  cd {ConsoleUi.Escape(projectName)}");
        AnsiConsole.MarkupLine($"  Open this folder in {ConsoleUi.Escape(agent.DisplayName)} and run:");
        foreach (var slash in NuvynCommands.Slash)
            AnsiConsole.MarkupLine($"    [cyan]{slash}[/]");

        return 0;
    }

    internal static AiAgent? ResolveAgent(string? agentId)
    {
        var known = AiAgent.Find(agentId);
        if (known is not null)
            return known;

        if (!string.IsNullOrWhiteSpace(agentId))
        {
            ConsoleUi.Error($"Unknown agent '{agentId}'. Choose: {string.Join(", ", AiAgent.Ids)}.");
            return null;
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            ConsoleUi.Warn("Non-interactive session — defaulting to Cursor. Pass --agent to choose.");
            return AiAgent.Cursor;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<AiAgent>()
                .Title("Select your AI coding agent")
                .PageSize(15)
                .EnableSearch()
                .UseConverter(a => a.PickerLabel)
                .AddChoices(AiAgent.All));
    }
}
