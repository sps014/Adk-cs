# ADK for .NET — Samples

Runnable, self-contained samples for the C# Agent Development Kit. Each sample is
a standalone console app; the header comment at the top of every `Program.cs`
explains what it demonstrates in detail.

## Setup

1. **Install the .NET SDK** (10.0 or later).
2. **Configure credentials.** Most samples talk to Gemini. Copy the sample's
   `.env.example` to `.env` in that sample's folder and fill in your values:

   ```bash
   cd samples/GoogleAdk.Samples.GoogleSearch
   cp .env.example .env
   # edit .env
   ```

   Every sample calls `AdkEnv.Load()` on startup, which reads the local `.env`
   (and falls back to process environment variables).

3. **Run it:**

   ```bash
   dotnet run --project samples/GoogleAdk.Samples.GoogleSearch
   ```

### Common environment variables

| Variable | Purpose |
| --- | --- |
| `GOOGLE_API_KEY` | Gemini API key (AI Studio). Simplest path. |
| `GOOGLE_GENAI_USE_VERTEXAI` | Set `True` to use Vertex AI instead of an API key. |
| `GOOGLE_CLOUD_PROJECT` / `GOOGLE_CLOUD_LOCATION` | Required when using Vertex AI. |

Samples that need extra variables document them in their own `.env.example`.

### Run modes

A handful of samples can launch the ADK Web UI in addition to the interactive
console. For those, pass `--web` (then open <http://localhost:5000>):

```bash
dotnet run --project samples/GoogleAdk.Samples.Thinking -- --web
```

Web-capable samples: `LoopAgent`, `MultiOrchestration`, `Ollama`,
`RequireConfirmation`, `Thinking`.

## Samples

### Orchestration & multi-agent patterns

| Sample | What it shows |
| --- | --- |
| `SubAgents` | LLM-driven routing via `transfer_to_agent` to specialist sub-agents. |
| `ParallelAgent` | Running multiple agents concurrently and collecting their outputs. |
| `LoopAgent` | Iterative refinement with `LoopAgent`. |
| `Orchestration` | Researcher → Analyst → Writer pipeline using Google Search + MCP tools. |
| `MultiOrchestration` | Sequential + parallel news-aggregator pattern with grounded search. |

### Tools & integrations

| Sample | What it shows |
| --- | --- |
| `Tools` | LLM with auth, bash execution, and search grounding. |
| `GoogleSearch` | Gemini's built-in Google Search grounding. |
| `OpenApi` | Generating tools from an OpenAPI spec with `OpenAPIToolset`. |
| `BlenderMcp` | Connecting to an MCP server over stdio (`uvx`). |
| `Auth` | OAuth2 `AuthConfig` wired to an authenticated tool. |
| `ComputerUse` | Tool-backed screen actions (computer use). |
| `VertexAiSearch` | Querying a Vertex AI Search data store. |
| `RagEngineSearch` | Retrieval over a Vertex AI RAG corpus. |

### Model features

| Sample | What it shows |
| --- | --- |
| `Thinking` | Model-native reasoning via `BuiltInPlanner` + `ThinkingConfig`. |
| `Planning` | Structured planning with `PlanReActPlanner`. |
| `ContextCaching` | Reusing cached context with `ContextCacheConfig`. |
| `OutputSchema` | Structured output via `SetModelResponseTool`. |
| `Ollama` | Running against a local Ollama model (no cloud key needed). |
| `LiveBidi` | Bidirectional streaming responses with `RunLiveAsync`. |
| `AudioAgent` | Audio input/output with an agent. |

### Platform & operations

| Sample | What it shows |
| --- | --- |
| `Plugins` | Logging and policy-based security plugins in the runner pipeline. |
| `FeatureFlags` | Feature flags and the app-container pattern. |
| `EvalOptimize` | LLM-powered evaluation and prompt optimization. |
| `Skills` | Packaging reusable agent skills as tools. |
| `ArtifactsWeb` | Saving and serving artifacts through the web UI. |
| `A2a` | Agent-to-Agent protocol (run with `--mode=server` or `--mode=client`). |
| `RequireConfirmation` | Human-in-the-loop tool confirmation. |

> Note: `Anthropic`, `Aot`, `A2uiWeb`, and `VideoCreator` are work-in-progress
> sketches without a project file and are not part of the solution yet.
