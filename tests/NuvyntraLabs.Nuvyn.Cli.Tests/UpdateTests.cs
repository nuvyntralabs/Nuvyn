using NuvyntraLabs.Nuvyn.Cli.Commands;
using NuvyntraLabs.Nuvyn.Cli.Infrastructure;
using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using Xunit;

namespace NuvyntraLabs.Nuvyn.Cli.Tests;

public sealed class UpdateTests
{
    [Fact]
    public void Update_refreshes_skills_and_reference_without_touching_host_or_specs()
    {
        var root = NewTemp();
        var before = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(root);
        try
        {
            Assert.Equal(0, InitCommand.Run("HarborDesk", "cursor", skipHost: true));
            var project = Path.Combine(root, "HarborDesk");

            var constitution = Path.Combine(project, ".nuvyn", "constitution.md");
            File.WriteAllText(constitution, "# customized constitution\n");

            var spec = Path.Combine(project, "specs", "001-desk", "spec.md");
            Directory.CreateDirectory(Path.GetDirectoryName(spec)!);
            File.WriteAllText(spec, "# user spec\n");

            var hostPage = Path.Combine(project, "HarborDesk", "Pages", "MainPage.xaml");
            Directory.CreateDirectory(Path.GetDirectoryName(hostPage)!);
            File.WriteAllText(hostPage, "<!-- host code -->");

            var constraints = Path.Combine(project, ".nuvyn", "reference", "constraints.md");
            File.WriteAllText(constraints, "stale reference\n");

            var skill = Path.Combine(project, ".cursor", "skills", "nuvyn-implement", "SKILL.md");
            File.WriteAllText(skill, "stale skill\n");

            Assert.Equal(0, UpdateCommand.Run(null, project));

            Assert.Equal("# customized constitution\n", File.ReadAllText(constitution));
            Assert.Equal("# user spec\n", File.ReadAllText(spec));
            Assert.Equal("<!-- host code -->", File.ReadAllText(hostPage));
            Assert.Contains("domain-agnostic", File.ReadAllText(constraints));
            Assert.Contains("Build is the gate", File.ReadAllText(skill));
            Assert.DoesNotContain("stale skill", File.ReadAllText(skill));

            var options = InitOptions.TryRead(project);
            Assert.NotNull(options);
            Assert.Equal("cursor", options.Agent);
            Assert.Equal(InitOptions.CliId, options.Cli);
            Assert.False(string.IsNullOrWhiteSpace(options.CliVersion));
        }
        finally
        {
            Directory.SetCurrentDirectory(before);
            TryDelete(root);
        }
    }

    [Fact]
    public void Update_can_switch_agent_without_removing_the_previous_skills()
    {
        var root = NewTemp();
        var before = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(root);
        try
        {
            Assert.Equal(0, InitCommand.Run("HarborDesk", "cursor", skipHost: true));
            var project = Path.Combine(root, "HarborDesk");

            Assert.Equal(0, UpdateCommand.Run("claude", project));

            Assert.True(File.Exists(Path.Combine(project, ".cursor", "skills", "nuvyn-specify", "SKILL.md")));
            Assert.True(File.Exists(Path.Combine(project, ".claude", "commands", "nuvyn.specify.md")));
            Assert.Equal("claude", InitOptions.TryRead(project)!.Agent);
        }
        finally
        {
            Directory.SetCurrentDirectory(before);
            TryDelete(root);
        }
    }

    [Fact]
    public void Update_rejects_a_folder_that_is_not_a_nuvyn_project()
    {
        var root = NewTemp();
        try
        {
            Assert.Equal(1, UpdateCommand.Run(null, root));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void Project_root_walks_up_to_the_nuvyn_folder()
    {
        var root = NewTemp();
        var before = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(root);
        try
        {
            Assert.Equal(0, InitCommand.Run("HarborDesk", "cursor", skipHost: true));
            var nested = Path.Combine(root, "HarborDesk", "HarborDesk", "Pages");
            Directory.CreateDirectory(nested);
            Assert.Equal(Path.Combine(root, "HarborDesk"), ProjectRoot.TryFind(nested));
        }
        finally
        {
            Directory.SetCurrentDirectory(before);
            TryDelete(root);
        }
    }

    [Fact]
    public void Refresh_workflow_skips_user_owned_root_files()
    {
        Assert.True(PayloadInstaller.IsUserOwnedWorkflowFile("constitution.md"));
        Assert.True(PayloadInstaller.IsUserOwnedWorkflowFile("feature.json"));
        Assert.True(PayloadInstaller.IsUserOwnedWorkflowFile("init-options.json"));
        Assert.True(PayloadInstaller.IsUserOwnedWorkflowFile("adopt-report.md"));
        Assert.False(PayloadInstaller.IsUserOwnedWorkflowFile(Path.Combine("reference", "constraints.md")));
        Assert.False(PayloadInstaller.IsUserOwnedWorkflowFile(Path.Combine("templates", "plan.md")));
    }

    private static string NewTemp()
    {
        var dir = Path.Combine(Path.GetTempPath(), "nuvyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void TryDelete(string dir)
    {
        try { Directory.Delete(dir, recursive: true); } catch { /* ignore locked temp files */ }
    }
}
