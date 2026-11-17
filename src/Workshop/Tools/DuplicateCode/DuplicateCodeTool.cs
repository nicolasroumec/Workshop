using System.Diagnostics;
using System.Globalization;
using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Tools.DuplicateCode;

internal class DuplicateCodeTool(IAnsiConsole console, ProjectStore store) : ITool
{
    private const int TopDuplicates = 20;

    public string Name => "Código duplicado";

    public string Icon => "📋";

    public string Description => "Bloques repetidos detectados con jscpd";

    public async Task RunAsync(Project project, CancellationToken cancellationToken)
    {
        var minTokens = console.Prompt(new TextPrompt<int>("Tamaño mínimo del bloque (tokens):")
            .DefaultValue(70)
            .Validate(t => t > 0, "Tiene que ser mayor a cero."));

        var outputFolder = Path.Combine(store.DataFolder, "reports", SafeFolderName(project.Name), "duplicate-code");
        if (Directory.Exists(outputFolder))
        {
            Directory.Delete(outputFolder, recursive: true);
        }

        var result = await console.Status()
            .StartAsync("Buscando duplicados con jscpd...", _ =>
                JscpdRunner.RunAsync(project.Path, minTokens, outputFolder, cancellationToken));

        var reportPath = Path.Combine(outputFolder, "jscpd-report.json");
        if (result.ExitCode != 0 || !File.Exists(reportPath))
        {
            console.MarkupLine($"[{Theme.Danger}]jscpd no generó el reporte.[/] ¿Está instalado Node.js?");
            console.WriteLine(string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error);
            return;
        }

        var report = JscpdReport.Parse(await File.ReadAllTextAsync(reportPath, cancellationToken));

        ShowSummary(report.Statistics.Total);
        ShowTopDuplicates(project, report.Duplicates);
        OfferHtmlReport(outputFolder);
    }

    private void ShowSummary(JscpdReport.Totals total)
    {
        console.MarkupLine(
            $"[bold]{total.Clones}[/] bloques duplicados · " +
            $"[bold]{total.DuplicatedLines}[/] de {total.Lines} líneas " +
            $"([bold]{total.Percentage.ToString("0.##", CultureInfo.CurrentCulture)} %[/]) · " +
            $"{total.Sources} archivos analizados");
        console.WriteLine();
    }

    private void ShowTopDuplicates(Project project, IReadOnlyList<JscpdReport.Duplicate> duplicates)
    {
        if (duplicates.Count == 0)
        {
            console.MarkupLine($"[{Theme.Success}]No hay código duplicado.[/]");
            return;
        }

        var table = new Table()
            .Title($"Los {Math.Min(TopDuplicates, duplicates.Count)} más grandes")
            .AddColumns("Líneas", "Tipo", "Archivo A", "Archivo B");

        foreach (var duplicate in duplicates.OrderByDescending(d => d.Lines).Take(TopDuplicates))
        {
            table.AddRow(
                duplicate.Lines.ToString(CultureInfo.CurrentCulture),
                duplicate.Format,
                FormatLocation(project, duplicate.FirstFile),
                FormatLocation(project, duplicate.SecondFile));
        }

        console.Write(table);
    }

    private void OfferHtmlReport(string outputFolder)
    {
        var htmlReport = Path.Combine(outputFolder, "jscpd-report.html");
        if (File.Exists(htmlReport) && console.Confirm("¿Abrir el reporte HTML con el detalle?"))
        {
            Process.Start(new ProcessStartInfo(htmlReport) { UseShellExecute = true });
        }
    }

    private static string FormatLocation(Project project, JscpdReport.Location location) =>
        $"{Theme.FileLink(Path.Combine(project.Path, location.Name), location.Name)} [{Theme.Muted}]{location.Start}-{location.End}[/]";

    private static string SafeFolderName(string name) =>
        string.Join('_', name.Split(Path.GetInvalidFileNameChars()));
}
