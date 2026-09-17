using Spectre.Console;

namespace NuvyntraLabs.Nuvyn.Cli;

internal static class ConsoleUi
{
    public static void Banner()
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddRow(new FigletText("Nuvyn").Color(Color.DeepSkyBlue1));
        grid.AddRow("[grey]Spec-driven .NET MAUI on the Nuvyntra stack[/]");
        AnsiConsole.Write(new Panel(grid).BorderColor(Color.DeepSkyBlue1).Padding(1, 0));
        AnsiConsole.WriteLine();
    }

    public static void Info(string message) => AnsiConsole.MarkupLine($"[grey]{Escape(message)}[/]");

    public static void Ok(string message) => AnsiConsole.MarkupLine($"[green]✓[/] {Escape(message)}");

    public static void Warn(string message) => AnsiConsole.MarkupLine($"[yellow]![/] {Escape(message)}");

    public static void Error(string message) => AnsiConsole.MarkupLine($"[red]Error:[/] {Escape(message)}");

    public static void Step(int n, string message) =>
        AnsiConsole.MarkupLine($"[deepskyblue1]{n}.[/] {Escape(message)}");

    public static string Escape(string value) => Markup.Escape(value);
}
