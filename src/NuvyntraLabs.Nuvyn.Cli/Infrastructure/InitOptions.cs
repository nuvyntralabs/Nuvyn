using System.Text.Json;
using System.Text.Json.Serialization;
using NuvyntraLabs.Nuvyn.Cli.Commands;

namespace NuvyntraLabs.Nuvyn.Cli.Infrastructure;

public sealed class InitOptions
{
    public const string CliId = "NuvyntraLabs.Nuvyn.Cli";
    public const string ModeInit = "init";
    public const string ModeAdopt = "adopt";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string Agent { get; set; } = "cursor";
    public string Cli { get; set; } = CliId;
    public string Mode { get; set; } = ModeInit;
    public string? CliVersion { get; set; }
    public string? Updated { get; set; }

    [JsonIgnore]
    public bool IsAdopt => string.Equals(Mode, ModeAdopt, StringComparison.OrdinalIgnoreCase);

    public static string FilePath(string projectDir) =>
        Path.Combine(projectDir, ".nuvyn", "init-options.json");

    public static InitOptions? TryRead(string projectDir)
    {
        var path = FilePath(projectDir);
        if (!File.Exists(path))
            return null;

        try
        {
            return JsonSerializer.Deserialize<InitOptions>(File.ReadAllText(path), Json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static InitOptions Create(string agentId) => new()
    {
        Agent = agentId,
        Cli = CliId,
        CliVersion = VersionCommand.GetVersion(),
        Updated = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    };

    public static void Write(string projectDir, InitOptions options)
    {
        var path = FilePath(projectDir);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(options, Json) + Environment.NewLine);
    }
}
