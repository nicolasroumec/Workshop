using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workshop.Configuration;
using Workshop.Menu;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Services
    .AddConsoleUi()
    .AddProjects();

using var host = builder.Build();

host.Services.GetRequiredService<MainMenu>().Run();
