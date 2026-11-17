using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Menu;

internal class MainMenu
{
    private const string ChangeProjectOption = "Cambiar proyecto";
    private const string AddProjectOption = "Agregar proyecto";
    private const string ExitOption = "Salir";

    private readonly IAnsiConsole _console;
    private readonly ProjectStore _store;
    private readonly List<Project> _projects;
    private Project? _current;

    public MainMenu(IAnsiConsole console, ProjectStore store)
    {
        _console = console;
        _store = store;
        _projects = store.Load();
        _current = _projects.FirstOrDefault();
    }

    public void Run()
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
        if (_projects.Count > 1)
        {
            options.Add(ChangeProjectOption);
        }
        options.AddRange([AddProjectOption, ExitOption]);

        return _console.Prompt(new SelectionPrompt<string>()
            .Title("¿Qué hacemos?")
            .AddChoices(options));
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
