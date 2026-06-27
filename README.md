# Agent Development Kit (ADK) for .NET

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE.md)
[![NuGet](https://img.shields.io/badge/NuGet-Preview-informational)](#installation)

<div align="center">
  <img src="docs/assets/agent-development-kit.png" width="256"/>
  <h3>A .NET toolkit for building, evaluating, and deploying AI agents.</h3>
</div>

The Agent Development Kit (ADK) is a code-first toolkit for building AI agents in .NET. It lets you define agent behavior, orchestration, and tools directly in C#, integrating closely with Google Cloud services.

---

> **⚠️ EXPERIMENTAL** - This project is in active development. The API is subject to change.

## Documentation

For guides, API references, and feature details, see the **[documentation](docs/index.md)**.

## Installation

```bash
dotnet add package GoogleAdk --prerelease
```

## Features

### Agents & Models
- **LLM agents**: `LlmAgent` with tool use, instructions, streaming, and structured output.
- **Models**: Gemini and local providers such as Ollama.
- **Orchestration**: `SequentialAgent`, `ParallelAgent`, and `LoopAgent` for multi-step workflows.
- **Planning**: ReAct-style planners for goal-oriented task breakdown.
- **Caching**: Gemini-backed implicit prompt and context caching.

### Tools & Integrations
- **Function tools**: Turn C# methods into agent tools with the `[FunctionTool]` attribute.
- **MCP**: Model Context Protocol support for connecting to external MCP servers and discovering tools at runtime.
- **OpenAPI**: Generate tools from OpenAPI specs with `OpenAPIToolset`.
- **Agent skills**: Folder-based skills (`SKILL.md`, `references/`, `assets/`, `scripts/`) via `SkillToolset` and `SkillLoader.LoadFromDirectory`.
- **Built-in tools**: Google Search, BigQuery, Cloud Spanner, Vertex AI Search, RAG Engine, Computer Use, and code execution.
- **Text-to-speech**: Audio generation from compatible models such as `gemini-2.5-flash-preview-tts`.

### Protocols & Connectivity
- **A2A**: Agent-to-agent communication over the A2A protocol, with client and server support.
- **Plugins & telemetry**: Lifecycle hooks, streaming events, and OpenTelemetry-style tracing.

### State, Memory & Evaluation
- **Session state**: Dynamic instructions with placeholder injection.
- **Memory**: Built-in memory services and persistence, including EF Core storage.
- **Evaluation & optimization**: Eval sets, inference runs, LLM-as-judge scoring, and prompt tuning.

### Runners & UI
- **Console runner**: Run agents interactively from the command line.
- **Web server & UI**: Embedded development server with REST/WebSocket APIs, Swagger, and a built-in UI for testing and debugging agents.

## Example Usage

```csharp
// Load env variables like GOOGLE_API_KEY
AdkEnv.Load();

var rootAgent = new LlmAgent(new()
{
    Name = "weather_assistant",
    Description = "An assistant that provides weather data.",
    Model = "gemini-2.5-flash",
    Instruction = "You are a helpful assistant. If the user asks about weather, use the GetWeatherData tool to provide the forecast.",
    Tools = [GetWeatherDataTool]
});

// Creates a webserver that can launch the ADK web UI and other endpoints 
await AdkServer.RunAsync(rootAgent);

// await ConsoleRunner.RunAsync(rootAgent); // For CLI usage

/// <summary>
/// Fetches the current weather data for a given location.
/// </summary>
/// <param name="location">The location to get the weather for (e.g., 'New York')</param>
/// <returns>A WeatherData object containing the location and forecast</returns>
[FunctionTool]
static WeatherData? GetWeatherData(string location)
{
    return new WeatherData(location, "Sunny with a chance of rainbows");
}

public record WeatherData(string Location, string Forecast);
```

### Development UI

A built-in development UI to test, evaluate, and debug your agents.

<img alt="Image" src="https://github.com/user-attachments/assets/1f8db230-b8f2-4b3c-85c8-54c32ea9e379" />

## Samples

Runnable samples cover MCP, A2A, orchestration, skills, evaluation, plugins, audio, and more. See [`samples/README.md`](samples/README.md) for the full index.

```bash
cd samples/GoogleAdk.Samples.GoogleSearch
cp .env.example .env          # then edit .env and set GOOGLE_API_KEY
dotnet run
```

## Building from Source

**Prerequisites:** [.NET SDK 10.0+](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/sps014/GoogleAdk-cs.git
cd GoogleAdk-cs
dotnet build GoogleAdk.slnx
dotnet test GoogleAdk.slnx
```

Package versions are managed centrally in [`Directory.Packages.props`](Directory.Packages.props), and shared build settings are in [`Directory.Build.props`](Directory.Build.props).

## Contributing

See our guidelines:
- [General contribution guideline and flow](https://google.github.io/adk-docs/contributing-guide/)
- [Code Contributing Guidelines](./CONTRIBUTING.md)

## License

Google’s **Agent Development Kit (ADK)** is a copyrighted product of Google LLC. Google retains applicable rights in that product and in the ADK and Google trademarks.

**This repository** is a **.NET port**: an open-source implementation aligned with ADK-style APIs and patterns. It is **not** Google’s first-party release of ADK for .NET. Copyright in this port belongs to its contributors, who license the code under the **Apache License, Version 2.0**. See **[LICENSE.md](LICENSE.md)** for full terms.

Use, reproduction, and distribution of this repository are governed by the Apache License, Version 2.0. It does not transfer ownership of Google’s ADK, Google’s trademarks, or third-party materials.
