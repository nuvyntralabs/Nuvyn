using System.CommandLine;
using NuvyntraLabs.Nuvyn.Cli.Agents;
using NuvyntraLabs.Nuvyn.Cli.Infrastructure;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using Spectre.Console;

namespace NuvyntraLabs.Nuvyn.Cli.Commands;

internal static class UpdateCommand
{
    public static Command Create()
    {
        var agentOpt = new Option<string>("--agent", "-a")
        {
            Description = "Refresh this agent's slash files (default: the agent recorded at init).",
        };

        var command = new Command(
            "update",
            "Refresh .nuvyn templates, reference, and agent skills. Does not overlay host code or specs/.")
        {
            agentOpt,
        };

        command.SetAction(parse => Run(parse.GetValue(agentOpt)));
        return command;
    }

    internal static int Run(string? agentId, string? startDirectory = null)
    {
        ConsoleUi.Banner();

        var projectDir = ProjectRoot.TryFind(startDirectory);
        if (projectDir is null)
        {
            ConsoleUi.Error("Not a Nuvyn project. Run nuvyn update from a folder created by nuvyn init or nuvyn adopt.");
            return 1;
        }

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

        var options = InitOptions.TryRead(projectDir) ?? new InitOptions();
        var agent = ResolveAgent(agentId, options.Agent, projectDir);
        if (agent is null)
            return 1;

        ConsoleUi.Step(1, $"Project: {projectDir}");
        ConsoleUi.Step(2, "Refreshing .nuvyn/templates and .nuvyn/reference…");
        var workflow = PayloadInstaller.RefreshWorkflow(payload, projectDir);
        foreach (var file in workflow.Take(8))
            ConsoleUi.Ok(file);
        if (workflow.Count > 8)
            ConsoleUi.Ok($"+ {workflow.Count - 8} more workflow files");

        ConsoleUi.Step(3, $"Writing {agent.DisplayName} commands…");
        var files = AgentInstaller.Install(payload, projectDir, agent);
        foreach (var file in files)
            ConsoleUi.Ok(file);

        options.Agent = agent.Id;
        options.Cli = InitOptions.CliId;
        options.CliVersion = VersionCommand.GetVersion();
        options.Updated = DateTime.UtcNow.ToString("yyyy-MM-dd");
        InitOptions.Write(projectDir, options);

        AnsiConsole.WriteLine();
        ConsoleUi.Ok("Host code, specs/, .nuvyn/constitution.md, and .nuvyn/adopt-report.md were left untouched.");
        ConsoleUi.Info("Existing PackageReference versions were not changed.");
        return 0;
    }

    internal static AiAgent? ResolveAgent(string? agentId, string? recordedId, string projectDir)
    {
        var requested = AiAgent.Find(agentId);
        if (requested is not null)
            return requested;

        if (!string.IsNullOrWhiteSpace(agentId))
        {
            ConsoleUi.Error($"Unknown agent '{agentId}'. Choose: {string.Join(", ", AiAgent.Ids)}.");
            return null;
        }

        var recorded = AiAgent.Find(recordedId);
        if (recorded is not null)
            return recorded;

        var detected = AgentInstaller.DetectInstalled(projectDir);
        if (detected is not null)
            return detected;

        ConsoleUi.Error("Could not determine the AI agent. Re-run with --agent (for example --agent cursor).");
        return null;
    }
}
