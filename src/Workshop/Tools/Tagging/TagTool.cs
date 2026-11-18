using Spectre.Console;
using Workshop.Projects;

namespace Workshop.Tools.Tagging;

internal class TagTool(IAnsiConsole console) : ITool
{
    private const int MaxCommitsShown = 15;
    private const int ShortCommitLength = 7;

    public string Name => "Crear tag";

    public string Icon => "🏷️";

    public string Description => "Publica una versión nueva en GitHub";

    public async Task RunAsync(Project project, CancellationToken cancellationToken)
    {
        var (plan, problem) = await console.Status()
            .StartAsync("Revisando el repositorio...", _ => GitTags.PrepareAsync(project.Path, cancellationToken));

        if (plan is null)
        {
            console.MarkupLine($"[{Theme.Danger}]{Markup.Escape(problem ?? "No se pudo revisar el repositorio.")}[/]");
            return;
        }

        ShowSummary(plan);

        var tag = TagVersion.ToTag(AskVersion(plan.Latest));
        var message = console.Prompt(new TextPrompt<string>("Mensaje del tag:").DefaultValue(tag));

        var target = $"{plan.Commit[..ShortCommitLength]} ({Markup.Escape(plan.Branch)})";
        if (!console.Confirm($"¿Crear y subir [bold]{tag}[/] sobre {target}?", defaultValue: false))
        {
            console.MarkupLine($"[{Theme.Muted}]No se creó ningún tag.[/]");
            return;
        }

        var result = await console.Status()
            .StartAsync($"Subiendo {tag}...", _ => GitTags.PublishAsync(project.Path, tag, message, plan.Commit, cancellationToken));

        if (!result.Succeeded)
        {
            console.MarkupLine($"[{Theme.Danger}]No se pudo publicar {tag}.[/] {Markup.Escape(result.Error.Trim())}");
            return;
        }

        var remote = await GitTags.RunAsync(project.Path, CancellationToken.None, "remote", "get-url", "origin");
        var url = GitTags.TagUrl(remote.Output.Trim(), tag);
        var label = url is null ? tag : $"[link={url}]{tag}[/]";
        console.MarkupLine($"[{Theme.Success}]✔ Tag publicado:[/] {label}");
    }

    private void ShowSummary(TagPlan plan)
    {
        var latest = plan.Latest is null ? "ninguno" : TagVersion.ToTag(plan.Latest);
        console.MarkupLine($"Último tag: [bold]{latest}[/] · {plan.Commits.Count} commits nuevos");

        foreach (var commit in plan.Commits.Take(MaxCommitsShown))
        {
            console.MarkupLine($"  [{Theme.Muted}]{Markup.Escape(commit)}[/]");
        }

        if (plan.Commits.Count > MaxCommitsShown)
        {
            console.MarkupLine($"  [{Theme.Muted}]y {plan.Commits.Count - MaxCommitsShown} más[/]");
        }

        console.WriteLine();
    }

    private Version AskVersion(Version? latest)
    {
        if (latest is null)
        {
            return TagVersion.First;
        }

        var bump = console.Prompt(new SelectionPrompt<VersionBump>()
            .Title("Nueva versión:")
            .HighlightStyle(Style.Parse(Theme.Accent))
            .UseConverter(b => $"{TagVersion.ToTag(TagVersion.Next(latest, b))} [{Theme.Muted}]({b.ToString().ToLowerInvariant()})[/]")
            .AddChoices(Enum.GetValues<VersionBump>()));

        return TagVersion.Next(latest, bump);
    }
}
