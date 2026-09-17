namespace NuvyntraLabs.Nuvyn.Cli.Agents;

public sealed record AiAgent(
    string Id,
    string DisplayName,
    string Folder,
    string SlashStyle)
{
    public static readonly AiAgent Cursor = new("cursor", "Cursor", ".cursor", "skills");
    public static readonly AiAgent Copilot = new("copilot", "GitHub Copilot", ".github", "skills");
    public static readonly AiAgent Claude = new("claude", "Claude Code", ".claude", "commands");
    public static readonly AiAgent Gemini = new("gemini", "Gemini CLI", ".gemini", "commands");

    public static readonly IReadOnlyList<AiAgent> All = [Cursor, Copilot, Claude, Gemini];

    public static AiAgent? Find(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return All.FirstOrDefault(a =>
            a.Id.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase) ||
            a.DisplayName.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
