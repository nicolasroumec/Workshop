using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Tools.DeadCss;

internal class DeadCssTool(IAnsiConsole console) : ITool
{
    public string Name => "Auditoría CSS";

    public string Icon => "🔍";

    public string Description => "Clases y variables CSS que sobran o faltan";

    public async Task RunAsync(Project project, CancellationToken cancellationToken)
    {
        var audit = await console.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn { CompletedStyle = Style.Parse(Theme.Accent) },
                new PercentageColumn())
            .StartAsync(context =>
            {
                var task = context.AddTask("Leyendo archivos");
                return DeadCssAnalyzer.AnalyzeAsync(project.Path, (done, total) =>
                {
                    task.MaxValue = total;
                    task.Value = done;
                    task.Description = $"Leyendo archivos {done}/{total}";
                }, cancellationToken);
            });

        console.WriteLine();

        if (audit.Files.Count == 0)
        {
            console.MarkupLine($"[{Theme.Warning}]No se encontraron archivos CSS globales.[/]");
            console.WriteLine();
        }
        else
        {
            ShowTree(project, audit.Files);
            ShowSummary(project, audit.Files);
            console.WriteLine();
        }

        ShowVariables(project, audit.Variables);
    }

    private void ShowVariables(Project project, CssVariableReport report)
    {
        console.Write(new Rule("[bold]Variables[/]").LeftJustified());
        console.WriteLine();

        if (report.Undeclared.Count + report.UndeclaredWithFallback.Count + report.Unused.Count == 0)
        {
            console.MarkupLine($"[{Theme.Success}]No hay variables inventadas ni sin uso.[/]");
            return;
        }

        console.MarkupLine(
            $"{Highlight(report.Undeclared.Count, Theme.Danger)} inventadas · " +
            $"{Highlight(report.UndeclaredWithFallback.Count, Theme.Warning)} con respaldo · " +
            $"{Highlight(report.Unused.Count, Theme.Warning)} sin uso");
        console.WriteLine();

        ShowVariableGroup(project, "Inventadas", "Usos", Theme.Danger, report.Undeclared);
        ShowVariableGroup(project, "Con respaldo", "Usos", Theme.Warning, report.UndeclaredWithFallback);
        ShowVariableGroup(project, "Sin uso", "Declaraciones", Theme.Warning, report.Unused);
    }

    private void ShowVariableGroup(Project project, string title, string countHeader, string color, IReadOnlyList<CssVariableIssue> issues)
    {
        if (issues.Count == 0)
        {
            return;
        }

        console.MarkupLine($"[bold {color}]{title}[/]");

        var table = new Table()
            .AddColumn("Variable")
            .AddColumn(new TableColumn(countHeader).RightAligned())
            .AddColumn("Dónde");

        foreach (var issue in issues)
        {
            var first = issue.Locations[0];
            var where = Theme.FileLink(Path.Combine(project.Path, first.RelativePath), $"{first.RelativePath}:{first.Line}");
            if (issue.Locations.Count > 1)
            {
                where += $" [{Theme.Muted}](+{issue.Locations.Count - 1})[/]";
            }

            table.AddRow(Markup.Escape(issue.Name), issue.Locations.Count.ToString(), where);
        }

        console.Write(table);
        console.WriteLine();
    }

    private void ShowTree(Project project, IReadOnlyList<CssFileResult> results)
    {
        var withIssues = results.Where(r => r.DeadClasses.Count > 0 || r.DynamicClasses.Count > 0).ToList();
        if (withIssues.Count == 0)
        {
            console.MarkupLine($"[{Theme.Success}]No hay clases muertas.[/]");
            console.WriteLine();
            return;
        }

        var tree = new Tree($"[bold]{Markup.Escape(project.Name)}[/]");

        foreach (var folder in withIssues.GroupBy(r => Path.GetDirectoryName(r.RelativePath) ?? ""))
        {
            IHasTreeNodes parent = folder.Key.Length == 0
                ? tree
                : tree.AddNode($"📁 [bold]{Markup.Escape(folder.Key)}[/]");

            foreach (var file in folder)
            {
                var fileNode = parent.AddNode(
                    $"{Theme.FileLink(Path.Combine(project.Path, file.RelativePath), Path.GetFileName(file.RelativePath))}  " +
                    $"[{Theme.Muted}]{file.DeadClasses.Count} muertas · {file.DynamicClasses.Count} a revisar[/]");

                foreach (var cssClass in file.DeadClasses)
                {
                    fileNode.AddNode($"[{Theme.Danger}]muerta [/]  .{Markup.Escape(cssClass)}");
                }

                foreach (var dynamic in file.DynamicClasses)
                {
                    fileNode.AddNode($"[{Theme.Warning}]revisar[/]  .{Markup.Escape(dynamic.Name)}  [{Theme.Muted}](prefijo {Markup.Escape(dynamic.Prefix)})[/]");
                }
            }
        }

        console.Write(tree);
        console.WriteLine();
    }

    private void ShowSummary(Project project, IReadOnlyList<CssFileResult> results)
    {
        var table = new Table().AddColumns("Archivo", "Vivas", "Muertas", "Dinámicas");

        foreach (var file in results)
        {
            table.AddRow(
                Theme.FileLink(Path.Combine(project.Path, file.RelativePath), file.RelativePath),
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
