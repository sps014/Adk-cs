using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Sessions;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using GoogleAdk.Core.Abstractions.Artifacts;
using GoogleAdk.Core.Abstractions.Memory;
using GoogleAdk.Core.Artifacts;
using GoogleAdk.Core.Telemetry;
using OpenTelemetry.Trace;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using GoogleAdk.Core.A2a;

namespace GoogleAdk.ApiServer;

/// <summary>
/// Simple static entry point for hosting an agent in the ADK dev server.
/// <code>
/// await AdkServer.RunAsync(myAgent);
/// </code>
/// </summary>
public static class AdkServer
{
    /// <summary>
    /// Starts the ADK dev server with the UI, serving the specified root agent with default options.
    /// </summary>
    public static Task RunAsync(BaseAgent rootAgent) 
        => RunAsync(rootAgent, new AdkServerOptions());

    /// <summary>
    /// Starts the ADK dev server with the UI, serving the specified root agent with configured options.
    /// </summary>
    public static Task RunAsync(BaseAgent rootAgent, Action<AdkServerOptions> configureOptions)
    {
        var options = new AdkServerOptions();
        configureOptions?.Invoke(options);
        return RunAsync(rootAgent, options);
    }

    /// <summary>
    /// Starts the ADK dev server with the UI, serving the specified root agent with explicitly provided options.
    /// </summary>
    public static async Task RunAsync(BaseAgent rootAgent, AdkServerOptions options)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddAdk(rootAgent, opt =>
        {
            opt.ArtifactService = options.ArtifactService;
            opt.MemoryService = options.MemoryService;
            opt.Port = options.Port;
            opt.Host = options.Host;
            opt.ShowAdkWebUI = options.ShowAdkWebUI;
            opt.ShowSwaggerUI = options.ShowSwaggerUI;
            opt.EnableA2a = options.EnableA2a;
            opt.InitialState = options.InitialState;
            opt.EnableCloudTracing = options.EnableCloudTracing;
            opt.ConfigureCors = options.ConfigureCors;
            opt.ConfigureServices = options.ConfigureServices;
            opt.ConfigureApp = options.ConfigureApp;
        });

        options.ConfigureServices?.Invoke(builder.Services);

        var app = builder.Build();

        app.UseAdk();

        options.ConfigureApp?.Invoke(app);

        app.MapAdk();

        var url = $"http://{options.Host}:{options.Port}";
        app.Urls.Add(url);

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        grid.AddRow("[bold cyan]Server[/]", $"[link={url}]{url}[/]");


        if (options.ShowSwaggerUI)
            grid.AddRow("[bold yellow]Swagger UI[/]", $"[link={url}/swagger]{url}/swagger[/]");
        grid.AddRow("[bold magenta]Agent[/]", rootAgent.Name);

        if (options.EnableA2a)
            grid.AddRow("[bold blue]A2A[/]", $"[link={url}/a2a/{rootAgent.Name}/{AgentCardConstants.AgentCardPath}]{url}/a2a/{rootAgent.Name}/{AgentCardConstants.AgentCardPath}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Panel(grid)
                .Header("[bold white]ADK Dev Server[/]")
                .Border(BoxBorder.Rounded)
                .Expand()
        );
        AnsiConsole.MarkupLine("[dim italic]Press Ctrl+C to stop.[/]");
        AnsiConsole.WriteLine();

        var shutdownToken = DevServerLifetime.Register(app);
        await app.RunAsync(shutdownToken);
    }
}
