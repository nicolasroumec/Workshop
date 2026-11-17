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
            console.MarkupLine($"[{Theme.Warning}]No se encontraron archivos CSS globales.[/]");
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
            console.MarkupLine($"    [{Theme.Danger}]muerta [/]  .{Markup.Escape(cssClass)}");
        }

        foreach (var dynamic in file.DynamicClasses)
        {
            console.MarkupLine($"    [{Theme.Warning}]revisar[/]  .{Markup.Escape(dynamic.Name)}  [{Theme.Muted}](prefijo {Markup.Escape(dynamic.Prefix)})[/]");
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
                Highlight(file.DeadClasses.Count, Theme.Danger),
                Highlight(file.DynamicClasses.Count, Theme.Warning));
        }

        table.AddRow(
            "[bold]TOTAL[/]",
            $"[bold]{results.Sum(r => r.AliveCount)}[/]",
            $"[bold]{Highlight(results.Sum(r => r.DeadClasses.Count), Theme.Danger)}[/]",
            $"[bold]{Highlight(results.Sum(r => r.DynamicClasses.Count), Theme.Warning)}[/]");

        console.Write(table);
    }

    private static string Highlight(int count, string color) => count == 0 ? "0" : $"[{color}]{count}[/]";
}
