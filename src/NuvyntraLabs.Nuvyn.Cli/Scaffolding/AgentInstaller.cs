using System.Text;
using NuvyntraLabs.Nuvyn.Cli.Agents;
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

    internal static string Destination(string projectDir, AiAgent agent, string id) =>
        agent.Id switch
        {
            "cursor" or "copilot" => Path.Combine(projectDir, agent.Folder, "skills", $"nuvyn-{id}", "SKILL.md"),
            "claude" => Path.Combine(projectDir, ".claude", "commands", $"nuvyn.{id}.md"),
            "gemini" => Path.Combine(projectDir, ".gemini", "commands", $"nuvyn.{id}.toml"),
            _ => Path.Combine(projectDir, ".agents", "skills", $"nuvyn-{id}", "SKILL.md"),
        };

    internal static string Wrap(AiAgent agent, string id, string body)
    {
        var slash = $"/nuvyn.{id}";
        var trimmed = body.Trim();
        var description = Description(id, slash);

        if (agent.Id == "gemini")
        {
            var escaped = trimmed.Replace("\\", "\\\\");
            return $"description = \"{description.Replace("\"", "'")}\"{Environment.NewLine}prompt = \"\"\"{Environment.NewLine}{escaped}{Environment.NewLine}\"\"\"{Environment.NewLine}";
        }

        var yaml = new StringBuilder();
        yaml.AppendLine("---");
        if (agent.Id is "cursor" or "copilot")
            yaml.AppendLine($"name: nuvyn-{id}");
        yaml.AppendLine("description: >-");
        yaml.AppendLine($"  {description}");
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

        yaml.AppendLine("---");
        yaml.AppendLine();
        yaml.AppendLine(trimmed);
        return yaml.ToString();
    }

    private static string Description(string id, string slash) =>
        $"Nuvyntra {id} for .NET MAUI (Android, iOS, Windows, Mac Catalyst). " +
        $"Use when the user invokes {slash} or /nuvyn-{id}.";

    private static void WriteInitOptions(string projectDir, AiAgent agent)
    {
        var nuvyn = Path.Combine(projectDir, ".nuvyn");
        Directory.CreateDirectory(nuvyn);
        File.WriteAllText(Path.Combine(nuvyn, "init-options.json"),
            $$"""
            {
              "agent": "{{agent.Id}}",
              "cli": "NuvyntraLabs.Nuvyn.Cli"
            }
            """);
    }
}
