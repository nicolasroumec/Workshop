using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Workshop.Menu;
using Workshop.Projects;

namespace Workshop.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsoleUi(this IServiceCollection services)
    {
        services.AddSingleton(AnsiConsole.Console);
        services.AddSingleton<MainMenu>();
        return services;
    }

    public static IServiceCollection AddProjects(this IServiceCollection services)
    {
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Workshop");
        services.AddSingleton(new ProjectStore(dataFolder));
        return services;
    }
}
