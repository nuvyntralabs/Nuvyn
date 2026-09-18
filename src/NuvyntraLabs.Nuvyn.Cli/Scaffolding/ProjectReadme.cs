using NuvyntraLabs.Nuvyn.Cli.Agents;
using NuvyntraLabs.Nuvyn.Cli.Workflow;

namespace NuvyntraLabs.Nuvyn.Cli.Scaffolding;

public static class ProjectReadme
{
    public static void Write(string projectDir, string projectName, AiAgent agent)
    {
        var chain = string.Join(" → ", NuvynCommands.Slash);
        var text =
            $"""
            # {projectName}

            Cross-platform **.NET MAUI** app (Android, iOS, Windows, Mac Catalyst) started with **Nuvyn**. Domain comes from your spec. Stack: MVVMExpress, Lumina UIKit, HttpForge, FormValidation, KeyboardManager. Do not add another package until the user explicitly asks.

            AI agent: **{agent.DisplayName}**

            ## Workflow (Nuvyntra-locked)

            In {agent.DisplayName}, run these in order:

            {chain}

            1. `/nuvyn.constitution` — project principles (already seeded in `.nuvyn/constitution.md`)
            2. `/nuvyn.specify` — what / why (mobile product spec)
            3. `/nuvyn.clarify` — resolve up to 3 ambiguities
            4. `/nuvyn.plan` — Nuvyntra packages + Lumina screens
            5. `/nuvyn.checklist` — reviewer quality checklist (optional gate)
            6. `/nuvyn.task` — dependency-ordered tasks
            7. `/nuvyn.analysis` — spec / plan / tasks consistency
            8. `/nuvyn.implement` — build against this host
            9. `/nuvyn.converge` — append remaining work, then implement again if needed

            Artifacts land in `specs/<nnn-slug>/`.

            ## Run

            ```bash
            dotnet build
            ```

            Diagnose the tree with `dotnet tool install -g Plugin.Maui.MauiDev.Cli --source https://api.nuget.org/v3/index.json` then `maui-dev doctor`.
            """;

        var path = Path.Combine(projectDir, "README.md");
        if (!File.Exists(path))
            File.WriteAllText(path, text);
        else
            File.WriteAllText(Path.Combine(projectDir, "NUVYN.md"), text);
    }
}
