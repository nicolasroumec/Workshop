using System.Reflection;
using Spectre.Console;
using Workshop.Install;
using Workshop.Projects;
using Workshop.Tools;

namespace Workshop.Menu;

internal class MainMenu
{
    private const string ChangeProjectOption = "Cambiar proyecto";
    private const string AddProjectOption = "Agregar proyecto";
    private const string InstallOption = "Instalar en esta PC";
    private const string ExitOption = "Salir";
    private const string ToolsGroup = "Herramientas";
    private const string ProjectGroup = "Proyecto";
    private const string WorkshopGroup = "Workshop";

    private static readonly Dictionary<string, string> OptionIcons = new()
    {
        [ChangeProjectOption] = "🔁",
        [AddProjectOption] = "➕",
        [InstallOption] = "📥",
        [ExitOption] = "🚪",
    };

    private static readonly string Version =
        typeof(MainMenu).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0]
        ?? "?";

    private readonly IAnsiConsole _console;
    private readonly ProjectStore _store;
    private readonly List<ITool> _tools;
    private readonly List<Project> _projects;
    private readonly string? _repository =
        OperatingSystem.IsWindows() ? SelfInstaller.FindRepository(AppContext.BaseDirectory) : null;
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

                case InstallOption:
                    await RunScreenAsync($"{OptionIcons[InstallOption]} [bold]{InstallOption}[/]", InstallAsync);
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
        Console.Title = _current is null ? "Workshop" : $"Workshop · {_current.Name}";

        _console.Clear();
        _console.Write(new FigletText("Workshop") { Color = Style.Parse(Theme.Accent).Foreground });
        _console.MarkupLine($"[{Theme.Muted}]v{Version}[/]");
        _console.WriteLine();

        var content = _current is null
            ? $"[{Theme.Warning}]Sin proyecto. Agregá uno para empezar.[/]"
            : $"[bold]{Markup.Escape(_current.Name)}[/]\n[{Theme.Muted}]{Markup.Escape(_current.Path)}[/]{FormatGit(GitInfo.Read(_current.Path))}";

        _console.Write(new Panel(new Markup(content))
            .Header($"[{Theme.Accent}] Proyecto [/]")
            .BorderStyle(Style.Parse(Theme.Accent))
            .RoundedBorder()
            .Expand());
        _console.WriteLine();
    }

    private static string FormatGit(GitInfo? git)
    {
        if (git is null)
        {
            return "";
        }

        var line = $"\n[{Theme.Accent}]rama {Markup.Escape(git.Branch)}[/]";
        return git.Ahead switch
        {
            null => $"{line}  [{Theme.Muted}]sin remoto[/]",
            0 => line,
            1 => $"{line}  [{Theme.Warning}]1 commit sin subir[/]",
            var ahead => $"{line}  [{Theme.Warning}]{ahead} commits sin subir[/]",
        };
    }

    private string AskOption()
    {
        var prompt = new SelectionPrompt<string>()
            .Title("¿Qué hacemos?")
            .Mode(SelectionMode.Leaf)
            .HighlightStyle(Style.Parse(Theme.Accent))
            .PageSize(15)
            .UseConverter(FormatOption);

        if (_current is not null)
        {
            prompt.AddChoiceGroup(ToolsGroup, _tools.Select(t => t.Name));
        }

        var projectOptions = new List<string>();
        if (_projects.Count > 1)
        {
            projectOptions.Add(ChangeProjectOption);
        }
        projectOptions.Add(AddProjectOption);

        prompt.AddChoiceGroup(ProjectGroup, projectOptions);

        if (_repository is not null)
        {
            prompt.AddChoiceGroup(WorkshopGroup, InstallOption);
        }

        prompt.AddChoices(ExitOption);

        return _console.Prompt(prompt);
    }

    private string FormatOption(string option)
    {
        var tool = _tools.FirstOrDefault(t => t.Name == option);
        if (tool is not null)
        {
            return $"{tool.Icon} {Markup.Escape(tool.Name)}  [{Theme.Muted}]{Markup.Escape(tool.Description)}[/]";
        }

        return OptionIcons.TryGetValue(option, out var icon)
            ? $"{icon} {Markup.Escape(option)}"
            : $"[bold]{Markup.Escape(option)}[/]";
    }

    private Task RunToolAsync(ITool tool, Project project) =>
        RunScreenAsync(
            $"{tool.Icon} [bold]{Markup.Escape(tool.Name)}[/] [{Theme.Muted}]· {Markup.Escape(project.Name)}[/]",
            cancellationToken => tool.RunAsync(project, cancellationToken));

    private async Task InstallAsync(CancellationToken cancellationToken)
    {
        var repository = _repository;
        if (!OperatingSystem.IsWindows() || repository is null)
        {
            return;
        }

        var folder = SelfInstaller.InstallFolder;
        if (!_console.Confirm($"¿Instalar Workshop v{Version} en [bold]{Markup.Escape(folder)}[/]?"))
        {
            return;
        }

        _console.MarkupLine($"[{Theme.Muted}]Compilando e instalando...[/]");
        var error = await SelfInstaller.PublishAsync(repository, folder, cancellationToken);
        if (error is not null)
        {
            _console.MarkupLine($"[{Theme.Danger}]No se pudo instalar.[/] Si [bold]workshop[/] está abierto en otra ventana, cerralo y reintentá.");
            _console.WriteLine(error);
            return;
        }

        var addedToPath = SelfInstaller.AddToUserPath(folder);
        _console.MarkupLine($"[{Theme.Success}]✔ Workshop instalado.[/]");
        _console.MarkupLine(addedToPath
            ? "Abrí una terminal nueva y escribí [bold]workshop[/]."
            : "Ya podés abrirlo escribiendo [bold]workshop[/].");
    }

    private async Task RunScreenAsync(string title, Func<CancellationToken, Task> action)
    {
        _console.Clear();
        _console.Write(new Rule(title).LeftJustified());
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
            await action(cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            _console.MarkupLine($"[{Theme.Warning}]Cancelado.[/]");
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
        _console.Prompt(new TextPrompt<string>($"[{Theme.Muted}]Enter para volver al menú[/]").AllowEmpty());
    }

    private void ChangeProject()
    {
        var selected = _console.Prompt(new SelectionPrompt<Project>()
            .Title("¿Qué proyecto?")
            .UseConverter(p => $"{Markup.Escape(p.Name)}  [{Theme.Muted}]{Markup.Escape(p.Path)}[/]")
            .AddChoices(_projects));

        SetCurrent(selected);
    }

    private void AddProject()
    {
        var path = _console.Prompt(new TextPrompt<string>($"Ruta de la carpeta [{Theme.Muted}](vacío para volver)[/]:")
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
