namespace NuvyntraLabs.Nuvyn.Cli.Workflow;

public static class NuvynCommands
{
    public static readonly IReadOnlyList<string> Slash =
    [
        "/nuvyn.constitution",
        "/nuvyn.specify",
        "/nuvyn.clarify",
        "/nuvyn.plan",
        "/nuvyn.checklist",
        "/nuvyn.task",
        "/nuvyn.analysis",
        "/nuvyn.implement",
        "/nuvyn.converge",
    ];

    public static readonly IReadOnlyList<string> Ids =
    [
        "constitution",
        "specify",
        "clarify",
        "plan",
        "checklist",
        "task",
        "analysis",
        "implement",
        "converge",
    ];

    public static IReadOnlyList<(string Label, string Prompt)> Handoffs(string id) => id switch
    {
        "constitution" => [("Specify feature", "/nuvyn.specify")],
        "specify" => [("Clarify spec", "/nuvyn.clarify"), ("Build plan", "/nuvyn.plan")],
        "clarify" => [("Build plan", "/nuvyn.plan")],
        "plan" => [("Create checklist", "/nuvyn.checklist"), ("Create tasks", "/nuvyn.task")],
        "checklist" => [("Create tasks", "/nuvyn.task")],
        "task" => [("Analyze consistency", "/nuvyn.analysis"), ("Implement", "/nuvyn.implement")],
        "analysis" => [("Implement", "/nuvyn.implement")],
        "implement" => [("Converge", "/nuvyn.converge")],
        "converge" => [("Implement remaining", "/nuvyn.implement")],
        _ => [],
    };
}
