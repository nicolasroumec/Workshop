using Spectre.Console;
using Workshop.Projects;
using Workshop.Tools;

namespace Workshop.Menu;

internal class MainMenu
{
    private const string ChangeProjectOption = "Cambiar proyecto";
    private const string AddProjectOption = "Agregar proyecto";
    private const string ExitOption = "Salir";

    private readonly IAnsiConsole _console;
    private readonly ProjectStore _store;
    private readonly List<ITool> _tools;
    private readonly List<Project> _projects;
    private Project? _current;

    public MainMenu(IAnsiConsole console, ProjectStore store, IEnumerable<ITool> tools)
    {
        _console = console;
        _store = store;
        _tools = tools.OrderBy(t => t.Name).ToList();
        _projects = store.Load();
        _current = _projects.FirstOrDefault();
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ShowHeader();

            switch (AskOption())
            {
                case ChangeProjectOption:
                    ChangeProject();
                    break;

                case AddProjectOption:
                    AddProject();
                    break;

                case ExitOption:
                    return;

                case var toolName when _current is not null:
                    await RunToolAsync(_tools.First(t => t.Name == toolName), _current);
                    break;
            }
        }
    }

    private void ShowHeader()
    {
        _console.Clear();
        _console.Write(new Rule("[bold]Workshop[/]").LeftJustified());
        _console.MarkupLine(_current is null
            ? "[yellow]Sin proyecto. Agregá uno para empezar.[/]"
            : $"Proyecto: [bold]{Markup.Escape(_current.Name)}[/]  [grey]{Markup.Escape(_current.Path)}[/]");
        _console.WriteLine();
    }

    private string AskOption()
    {
        var options = new List<string>();
        if (_current is not null)
        {
            options.AddRange(_tools.Select(t => t.Name));
        }
        if (_projects.Count > 1)
        {
            options.Add(ChangeProjectOption);
        }
        options.AddRange([AddProjectOption, ExitOption]);

        return _console.Prompt(new SelectionPrompt<string>()
            .Title("¿Qué hacemos?")
            .UseConverter(Markup.Escape)
            .AddChoices(options));
    }

    private async Task RunToolAsync(ITool tool, Project project)
    {
        _console.Clear();
        _console.Write(new Rule($"[bold]{Markup.Escape(tool.Name)}[/] [grey]· {Markup.Escape(project.Name)}[/]").LeftJustified());
        _console.WriteLine();

        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler onCancel = (_, e) =>
        {
            e.Cancel = true;
            cancellation.Cancel();
        };
        Console.CancelKeyPress += onCancel;

        try
        {
            await tool.RunAsync(project, cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            _console.MarkupLine("[yellow]Cancelado.[/]");
        }
        catch (Exception ex)
        {
            _console.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
        finally
        {
            Console.CancelKeyPress -= onCancel;
        }

        _console.WriteLine();
        _console.Prompt(new TextPrompt<string>("[grey]Enter para volver al menú[/]").AllowEmpty());
    }

    private void ChangeProject()
    {
        var selected = _console.Prompt(new SelectionPrompt<Project>()
            .Title("¿Qué proyecto?")
            .UseConverter(p => $"{Markup.Escape(p.Name)}  [grey]{Markup.Escape(p.Path)}[/]")
            .AddChoices(_projects));

        SetCurrent(selected);
    }

    private void AddProject()
    {
        var path = _console.Prompt(new TextPrompt<string>("Ruta de la carpeta [grey](vacío para volver)[/]:")
            .AllowEmpty()
            .Validate(p => p.Length == 0 || Directory.Exists(p.Trim('"')), "Esa carpeta no existe."))
            .Trim().Trim('"');

        if (path.Length == 0)
        {
            return;
        }

        var fullPath = Path.GetFullPath(path);

        var name = _console.Prompt(new TextPrompt<string>("Nombre:")
            .DefaultValue(Path.GetFileName(Path.TrimEndingDirectorySeparator(fullPath))));

        _projects.RemoveAll(p => p.Name == name);
        SetCurrent(new Project(name, fullPath));
    }

    private void SetCurrent(Project project)
    {
        _current = project;
        _store.MarkAsLast(_projects, project);
    }
}
