using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Menu;

internal class MainMenu
{
    private const string ChangeProjectOption = "Cambiar proyecto";
    private const string AddProjectOption = "Agregar proyecto";
    private const string ExitOption = "Salir";

    private readonly List<Project> _projects = ProjectStore.Load();
    private Project? _current;

    public MainMenu()
    {
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
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold]Workshop[/]").LeftJustified());
        AnsiConsole.MarkupLine(_current is null
            ? "[yellow]Sin proyecto. Agregá uno para empezar.[/]"
            : $"Proyecto: [bold]{Markup.Escape(_current.Name)}[/]  [grey]{Markup.Escape(_current.Path)}[/]");
        AnsiConsole.WriteLine();
    }

    private string AskOption()
    {
        var options = new List<string>();
        if (_projects.Count > 1)
        {
            options.Add(ChangeProjectOption);
        }
        options.AddRange([AddProjectOption, ExitOption]);

        return AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("¿Qué hacemos?")
            .AddChoices(options));
    }

    private void ChangeProject()
    {
        var selected = AnsiConsole.Prompt(new SelectionPrompt<Project>()
            .Title("¿Qué proyecto?")
            .UseConverter(p => $"{Markup.Escape(p.Name)}  [grey]{Markup.Escape(p.Path)}[/]")
            .AddChoices(_projects));

        SetCurrent(selected);
    }

    private void AddProject()
    {
        var path = AnsiConsole.Prompt(new TextPrompt<string>("Ruta de la carpeta:")
            .Validate(p => Directory.Exists(p.Trim('"')), "Esa carpeta no existe."))
            .Trim('"');

        var fullPath = Path.GetFullPath(path);

        var name = AnsiConsole.Prompt(new TextPrompt<string>("Nombre:")
            .DefaultValue(Path.GetFileName(Path.TrimEndingDirectorySeparator(fullPath))));

        _projects.RemoveAll(p => p.Name == name);
        SetCurrent(new Project(name, fullPath));
    }

    private void SetCurrent(Project project)
    {
        _current = project;
        ProjectStore.MarkAsLast(_projects, project);
    }
}
