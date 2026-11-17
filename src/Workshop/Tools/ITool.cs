using Workshop.Projects;

namespace Workshop.Tools;

internal interface ITool
{
    string Name { get; }

    string Icon { get; }

    string Description { get; }

    Task RunAsync(Project project, CancellationToken cancellationToken);
}
