using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Workshop.Menu;
using Workshop.Projects;
using Workshop.Tools;

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

    public static IServiceCollection AddTools(this IServiceCollection services)
    {
        var toolTypes = typeof(ITool).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ITool).IsAssignableFrom(t));

        foreach (var toolType in toolTypes)
        {
            services.AddSingleton(typeof(ITool), toolType);
        }

        return services;
    }
}
