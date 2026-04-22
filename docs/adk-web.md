# ADK Web

The ADK ships with a powerful visual dashboard, **ADK Web**, which automatically mounts an ASP.NET Core server and a Blazor front-end over your agent. It visualizes the execution graph, multi-agent orchestration, tool calls, and streaming responses in real-time.

It is highly recommended to use `AdkServer` when debugging complex workflows like `LoopAgent` or `ParallelAgent`.

## Launching ADK Web

You can launch the dashboard using a single line of code via `AdkServer.RunAsync`.

```csharp
using GoogleAdk.ApiServer;
using GoogleAdk.Core.Agents;

// Ensure your agent is fully configured
var myComplexAgent = new SequentialAgent(new SequentialAgentConfig { /* ... */ });

// Launch the interactive dashboard application with default options
// The browser will automatically open to http://localhost:8080/dev-ui
await AdkServer.RunAsync(myComplexAgent);
```

## Configuring ADK Server Options

You can easily configure server options such as port, host, UI toggles, tracing, and A2A endpoints using the action overload. You can also hook into the underlying ASP.NET Core pipeline using `ConfigureServices` and `ConfigureApp`:

```csharp
using GoogleAdk.ApiServer;
using Microsoft.Extensions.DependencyInjection;

await AdkServer.RunAsync(myComplexAgent, options => 
{
    options.Port = 9000;
    options.ShowSwaggerUI = true;
    options.EnableA2a = true;
    
    // Advanced: supply custom CORS policy
    options.ConfigureCors = policy => 
        policy.WithOrigins("http://my-frontend.com").AllowAnyMethod().AllowAnyHeader();

    // Hook into the underlying IServiceCollection
    options.ConfigureServices = services => 
    {
        services.AddSingleton<MyCustomService>();
    };

    // Hook into the underlying IApplicationBuilder
    options.ConfigureApp = app => 
    {
        app.UseMyCustomMiddleware();
    };
});
```

## ASP.NET Core Integration

If you already have an existing ASP.NET Core application, you don't need to use `AdkServer.RunAsync()`. You can seamlessly integrate the ADK directly into your pipeline using standard ASP.NET Core extension methods.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add your existing services...
builder.Services.AddControllers();

// 1. Add ADK services to the container
builder.Services.AddAdk(myComplexAgent, options => 
{
    options.EnableA2a = true;
    // Note: Host/Port options are ignored here as your ASP.NET host controls binding
});

var app = builder.Build();

// Add your existing middleware...
app.UseHttpsRedirection();

// 2. Add ADK middleware (Swagger UI, CORS, WebSockets, ADK Web UI)
app.UseAdk();

// 3. Map ADK API endpoints and fallback routes
app.MapAdk();
app.MapControllers();

app.Run();
```

