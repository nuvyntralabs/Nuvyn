using NuvyntraLabs.Nuvyn.Cli.Scaffolding;
using Xunit;

namespace NuvyntraLabs.Nuvyn.Cli.Tests;

public sealed class MauiDevCompanionTests
{
    [Fact]
    public void Inspect_is_missing_when_the_command_is_absent()
    {
        var result = MauiDevCompanion.Inspect(new MauiDevCompanionOptions
        {
            FindCommand = () => null,
        });
        Assert.Equal(MauiDevCompanionStatus.Missing, result.Status);
        Assert.Contains("not on PATH", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Inspect_warns_when_below_the_floor()
    {
        var result = MauiDevCompanion.Inspect(new MauiDevCompanionOptions
        {
            FindCommand = () => "/tmp/maui-dev",
            ListGlobalTools = () => "Plugin.Maui.MauiDev.Cli    1.1.0    maui-dev",
        });
        Assert.Equal(MauiDevCompanionStatus.BelowFloor, result.Status);
        Assert.Equal("1.1.0", result.Version);
    }

    [Fact]
    public void Inspect_accepts_1_2_0_and_newer()
    {
        var result = MauiDevCompanion.Inspect(new MauiDevCompanionOptions
        {
            FindCommand = () => "/tmp/maui-dev",
            ListGlobalTools = () => "plugin.maui.maudev.cli   1.2.2   maui-dev",
        });
        Assert.Equal(MauiDevCompanionStatus.Found, result.Status);
        Assert.Equal("1.2.2", result.Version);
    }

    [Fact]
    public void TryReadListedVersion_matches_package_id_or_command()
    {
        Assert.True(MauiDevCompanion.TryReadListedVersion(
            """
            Package Id                         Version      Commands
            -----------------------------------------------------------
            nuvyntralabs.nuvyn.cli             1.0.1        nuvyn
            Plugin.Maui.MauiDev.Cli            1.2.1        maui-dev
            """,
            out var version));
        Assert.Equal(new Version(1, 2, 1), version);
    }

    [Fact]
    public void RunDoctor_does_not_start_when_missing()
    {
        var ran = false;
        var result = MauiDevCompanion.RunDoctor("/tmp/app", new MauiDevCompanionOptions
        {
            FindCommand = () => null,
            Run = (_, _) =>
            {
                ran = true;
                return 0;
            },
        });
        Assert.Equal(MauiDevCompanionStatus.Missing, result.Status);
        Assert.False(ran);
    }

    [Fact]
    public void RunDoctor_passes_path_and_skips_the_nested_update_prompt()
    {
        string[] received = [];
        var result = MauiDevCompanion.RunDoctor("/tmp/ClinicApp", new MauiDevCompanionOptions
        {
            FindCommand = () => "maui-dev",
            ListGlobalTools = () => "Plugin.Maui.MauiDev.Cli 1.2.2 maui-dev",
            Run = (_, arguments) =>
            {
                received = [.. arguments];
                return 0;
            },
        });
        Assert.Equal(MauiDevCompanionStatus.DoctorPassed, result.Status);
        Assert.Equal(0, result.DoctorExitCode);
        Assert.Equal(["doctor", "--path", "/tmp/ClinicApp"], received);
    }

    [Fact]
    public void RunDoctor_issues_do_not_become_a_nuvyn_failure()
    {
        var result = MauiDevCompanion.RunDoctor("/tmp/app", new MauiDevCompanionOptions
        {
            FindCommand = () => "maui-dev",
            ListGlobalTools = () => "Plugin.Maui.MauiDev.Cli 1.2.0 maui-dev",
            Run = (_, _) => 1,
        });
        Assert.Equal(MauiDevCompanionStatus.DoctorIssues, result.Status);
        Assert.Equal(1, result.DoctorExitCode);
        Assert.Contains("found issues", result.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("exited", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RunDoctor_still_runs_when_below_the_floor()
    {
        var ran = false;
        var result = MauiDevCompanion.RunDoctor("/tmp/app", new MauiDevCompanionOptions
        {
            FindCommand = () => "maui-dev",
            ListGlobalTools = () => "Plugin.Maui.MauiDev.Cli 1.0.1 maui-dev",
            Run = (_, _) =>
            {
                ran = true;
                return 0;
            },
        });
        Assert.True(ran);
        Assert.Equal(MauiDevCompanionStatus.BelowFloor, result.Status);
    }
}
