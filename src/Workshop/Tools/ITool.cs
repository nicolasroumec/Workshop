using Workshop.Projects;

namespace Workshop.Tools;

internal interface ITool
{
    string Name { get; }

    Task RunAsync(Project project, CancellationToken cancellationToken);
}
