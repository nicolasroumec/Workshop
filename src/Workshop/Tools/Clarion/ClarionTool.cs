using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Tools.Clarion;

internal class ClarionTool(IAnsiConsole console) : ITool
{
    public string Name => "Conversor Clarion";

    public string Icon => "🕰️";

    public string Description => "Fechas Clarion, en ambos sentidos";

    public async Task RunAsync(Project project, CancellationToken cancellationToken)
    {
        var prompt = $"Fecha [{Theme.Muted}](82455 o 29/09/2026, vacío para volver)[/]:";

        while (true)
        {
            var input = (await new TextPrompt<string>(prompt).AllowEmpty().ShowAsync(console, cancellationToken)).Trim();
            if (input.Length == 0)
            {
                return;
            }

            var result = ClarionConverter.ConvertDate(input);
            console.MarkupLine(result is null
                ? $"[{Theme.Danger}]Valor no válido.[/]"
                : $"[{Theme.Success}]→[/] [bold]{Markup.Escape(result)}[/]");
        }
    }
}
