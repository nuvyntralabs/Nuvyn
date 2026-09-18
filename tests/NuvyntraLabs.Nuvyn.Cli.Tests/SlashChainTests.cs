using NuvyntraLabs.Nuvyn.Cli.Workflow;
using Xunit;

namespace NuvyntraLabs.Nuvyn.Cli.Tests;

public sealed class SlashChainTests
{
    [Fact]
    public void Slash_chain_is_complete_and_locked_to_the_default_host()
    {
        Assert.Equal(
            [
                "constitution", "specify", "clarify", "plan", "checklist",
                "task", "analysis", "implement", "converge",
            ],
            NuvynCommands.Ids);

        var commands = Path.Combine(FindPayload(), "commands");
        foreach (var id in NuvynCommands.Ids)
        {
            var path = Path.Combine(commands, $"{id}.md");
            Assert.True(File.Exists(path), path);
            var text = File.ReadAllText(path);
            Assert.Contains($"/nuvyn.{id}", text);
            Assert.DoesNotContain("dump the full catalog", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("dotnet add package Plugin.Maui.LocalStore", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Plan_and_implement_keep_the_smallest_set_and_require_a_build()
    {
        var commands = Path.Combine(FindPayload(), "commands");
        var plan = File.ReadAllText(Path.Combine(commands, "plan.md"));
        var implement = File.ReadAllText(Path.Combine(commands, "implement.md"));
        var constraints = File.ReadAllText(Path.Combine(FindPayload(), "nuvyn", "reference", "constraints.md"));

        foreach (var package in new[]
                 {
                     "MVVMExpress", "UIKit", "HttpForge", "FormValidation", "KeyboardManager",
                 })
        {
            Assert.Contains(package, plan);
            Assert.Contains(package, constraints);
        }

        Assert.Contains("Do not add any other package unless the user explicitly asked", plan);
        Assert.Contains("Build is the gate", implement);
        Assert.Contains("dotnet build", implement);
        Assert.Contains("domain-agnostic", constraints);
        Assert.DoesNotContain("clinic default", constraints, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Plan_template_lists_only_the_default_host_rows()
    {
        var plan = File.ReadAllText(Path.Combine(FindPayload(), "nuvyn", "templates", "plan.md"));
        Assert.Contains("Plugin.Maui.MVVMExpress", plan);
        Assert.Contains("NuvyntraLabs.UIKit", plan);
        Assert.Contains("Plugin.Maui.HttpForge", plan);
        Assert.Contains("Plugin.Maui.FormValidation", plan);
        Assert.Contains("Plugin.Maui.KeyboardManager", plan);
        Assert.Contains("None unless the user asked", plan);
        Assert.DoesNotContain("Plugin.Maui.LocalStore", plan.Split("Package map")[1].Split("One extra row")[0]);
    }

    private static string FindPayload()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var payload = Path.Combine(dir.FullName, "payload");
            if (Directory.Exists(Path.Combine(payload, "commands")))
                return payload;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("payload/");
    }
}
