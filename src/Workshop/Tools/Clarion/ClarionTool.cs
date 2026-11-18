using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Tools.Clarion;

internal class ClarionTool(IAnsiConsole console) : ITool
{
    public string Name => "Conversor Clarion";

    public string Icon => "🕰️";

    public string Description => "Fechas y horas Clarion, en ambos sentidos";

    public async Task RunAsync(Project project, CancellationToken cancellationToken)
    {
        var mode = await new SelectionPrompt<ClarionMode>()
            .Title("¿Qué convertimos?")
            .HighlightStyle(Style.Parse(Theme.Accent))
            .UseConverter(Label)
            .AddChoices(Enum.GetValues<ClarionMode>())
            .ShowAsync(console, cancellationToken);

        var prompt = $"{Label(mode)} [{Theme.Muted}]({Example(mode)}, vacío para volver)[/]:";

        while (true)
        {
            var input = (await new TextPrompt<string>(prompt).AllowEmpty().ShowAsync(console, cancellationToken)).Trim();
            if (input.Length == 0)
            {
                return;
            }

            var result = Convert(mode, input);
            console.MarkupLine(result is null
                ? $"[{Theme.Danger}]Valor no válido.[/]"
                : $"[{Theme.Success}]→[/] [bold]{Markup.Escape(result)}[/]");
        }
    }

    private static string? Convert(ClarionMode mode, string input) => mode switch
    {
        ClarionMode.Date => ClarionConverter.ConvertDate(input),
        ClarionMode.Time => ClarionConverter.ConvertTime(input),
        _ => ClarionConverter.ConvertDateTime(input),
    };

    private static string Label(ClarionMode mode) => mode switch
    {
        ClarionMode.Date => "Fecha",
        ClarionMode.Time => "Hora",
        _ => "Fecha y hora",
    };

    private static string Example(ClarionMode mode) => mode switch
    {
        ClarionMode.Date => "82455 o 29/09/2026",
        ClarionMode.Time => "4599263 o 12:46:32.62",
        _ => "82455 4599263 o 29/09/2026 12:46:32.62",
    };
}
