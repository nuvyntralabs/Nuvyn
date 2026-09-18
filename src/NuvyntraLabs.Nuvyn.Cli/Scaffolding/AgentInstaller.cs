using System.Text;
using NuvyntraLabs.Nuvyn.Cli.Agents;
using NuvyntraLabs.Nuvyn.Cli.Infrastructure;
using NuvyntraLabs.Nuvyn.Cli.Workflow;

namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public static class AgentInstaller
{
    public static IReadOnlyList<string> Install(string payloadRoot, string projectDir, AiAgent agent)
    {
        var written = new List<string>();
        var commandsDir = Path.Combine(payloadRoot, "commands");

        foreach (var id in NuvynCommands.Ids)
        {
            var bodyPath = Path.Combine(commandsDir, $"{id}.md");
            if (!File.Exists(bodyPath))
                throw new FileNotFoundException($"Missing command payload: {id}.md", bodyPath);

            var body = File.ReadAllText(bodyPath);
            var dest = Destination(projectDir, agent, id);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.WriteAllText(dest, Wrap(agent, id, body));
            written.Add(Path.GetRelativePath(projectDir, dest));
        }

        WriteInitOptions(projectDir, agent);
        return written;
    }

    public static AiAgent? DetectInstalled(string projectDir)
    {
        foreach (var agent in AiAgent.All)
        {
            if (File.Exists(Destination(projectDir, agent, "specify")))
                return agent;
        }

        return null;
    }

    internal static string Destination(string projectDir, AiAgent agent, string id)
    {
        var folder = Path.Combine(projectDir, agent.CommandsDir.Replace('/', Path.DirectorySeparatorChar));
        return agent.Format switch
        {
            AgentFormat.Skill => Path.Combine(folder, $"nuvyn-{id}", "SKILL.md"),
            AgentFormat.Markdown => Path.Combine(folder, $"nuvyn.{id}.md"),
            AgentFormat.Toml => Path.Combine(folder, $"nuvyn.{id}.toml"),
            AgentFormat.Yaml => Path.Combine(folder, $"nuvyn.{id}.yaml"),
            _ => throw new ArgumentOutOfRangeException(nameof(agent), agent.Format, "Unknown agent format."),
        };
    }

    internal static string Wrap(AiAgent agent, string id, string body)
    {
        var slash = $"/nuvyn.{id}";
        var trimmed = body.Trim();
        var description = Description(id, slash);

        return agent.Format switch
        {
            AgentFormat.Toml => WrapToml(description, trimmed),
            AgentFormat.Yaml => WrapYaml(id, description, trimmed),
            _ => WrapMarkdown(agent, id, description, trimmed),
        };
    }

    private static string WrapToml(string description, string body)
    {
        var escaped = body.Replace("\\", "\\\\");
        return $"description = \"{description.Replace("\"", "'")}\"{Environment.NewLine}prompt = \"\"\"{Environment.NewLine}{escaped}{Environment.NewLine}\"\"\"{Environment.NewLine}";
    }

    private static string WrapYaml(string id, string description, string body)
    {
        var indented = string.Join(Environment.NewLine, body.Split('\n').Select(line => "  " + line.TrimEnd('\r')));
        return
            $"""
            version: 1.0.0
            title: nuvyn.{id}
            description: "{description.Replace("\"", "'")}"
            author:
              contact: nuvyn
            parameters:
              - key: args
                input_type: string
                requirement: optional
                default: ""
                description: User input passed to the command.
            prompt: |2
            {indented}

            """;
    }

    private static string WrapMarkdown(AiAgent agent, string id, string description, string body)
    {
        var yaml = new StringBuilder();
        yaml.AppendLine("---");
        if (agent.IncludeName)
            yaml.AppendLine($"name: nuvyn-{id}");
        yaml.AppendLine("description: >-");
        yaml.AppendLine($"  {description}");
        if (agent.IncludeHandoffs)
        {
            var handoffs = NuvynCommands.Handoffs(id);
            if (handoffs.Count > 0)
            {
                yaml.AppendLine("handoffs:");
                foreach (var (label, prompt) in handoffs)
                {
                    yaml.AppendLine($"  - label: {label}");
                    yaml.AppendLine($"    prompt: {prompt}");
                }
            }
        }

        yaml.AppendLine("---");
        yaml.AppendLine();
        yaml.AppendLine(body);
        return yaml.ToString();
    }

    private static string Description(string id, string slash) =>
        $"Nuvyntra {id} for .NET MAUI (Android, iOS, Windows, Mac Catalyst). " +
        $"Use when the user invokes {slash} or /nuvyn-{id}.";

    private static void WriteInitOptions(string projectDir, AiAgent agent)
    {
        var existing = InitOptions.TryRead(projectDir);
        var options = existing ?? InitOptions.Create(agent.Id);
        options.Agent = agent.Id;
        options.Cli = InitOptions.CliId;
        options.CliVersion = Commands.VersionCommand.GetVersion();
        options.Updated ??= DateTime.UtcNow.ToString("yyyy-MM-dd");
        InitOptions.Write(projectDir, options);
    }
}
