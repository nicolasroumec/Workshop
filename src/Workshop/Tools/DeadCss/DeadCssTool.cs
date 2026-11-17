using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Tools.DeadCss;

internal class DeadCssTool(IAnsiConsole console) : ITool
{
    public string Name => "CSS muerto";

    public async Task RunAsync(Project project, CancellationToken cancellationToken)
    {
        var results = await console.Status()
            .StartAsync("Analizando...", _ => DeadCssAnalyzer.AnalyzeAsync(project.Path, cancellationToken));

        if (results.Count == 0)
        {
            console.MarkupLine("[yellow]No se encontraron archivos CSS globales.[/]");
            return;
        }

        foreach (var file in results.Where(r => r.DeadClasses.Count > 0 || r.DynamicClasses.Count > 0))
        {
            ShowDetail(file);
        }

        ShowSummary(results);
    }

    private void ShowDetail(CssFileResult file)
    {
        console.MarkupLine($"[bold]{Markup.Escape(file.RelativePath)}[/]");

        foreach (var cssClass in file.DeadClasses)
        {
            console.MarkupLine($"    [red]muerta [/]  .{Markup.Escape(cssClass)}");
        }

        foreach (var dynamic in file.DynamicClasses)
        {
            console.MarkupLine($"    [yellow]revisar[/]  .{Markup.Escape(dynamic.Name)}  [grey](prefijo {Markup.Escape(dynamic.Prefix)})[/]");
        }

        console.WriteLine();
    }

    private void ShowSummary(IReadOnlyList<CssFileResult> results)
    {
        var table = new Table().AddColumns("Archivo", "Vivas", "Muertas", "Dinámicas");

        foreach (var file in results)
        {
            table.AddRow(
                Markup.Escape(file.RelativePath),
                file.AliveCount.ToString(),
                Highlight(file.DeadClasses.Count, "red"),
                Highlight(file.DynamicClasses.Count, "yellow"));
        }

        table.AddRow(
            "[bold]TOTAL[/]",
            $"[bold]{results.Sum(r => r.AliveCount)}[/]",
            $"[bold]{Highlight(results.Sum(r => r.DeadClasses.Count), "red")}[/]",
            $"[bold]{Highlight(results.Sum(r => r.DynamicClasses.Count), "yellow")}[/]");

        console.Write(table);
    }

    private static string Highlight(int count, string color) => count == 0 ? "0" : $"[{color}]{count}[/]";
}
