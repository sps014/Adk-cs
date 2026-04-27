---
hide:
  - navigation
  - toc
---

<div class="adk-hero">
  <img src="assets/agent-development-kit.png" width="160" style="margin-bottom: 2rem;"/>
  <h1 class="adk-gradient-text">Agent Development Kit<br>for .NET</h1>
  <p class="adk-hero-subtitle">
    An open-source, code-first .NET framework for building, evaluating, and deploying sophisticated AI agents with flexibility and control.
  </p>
  <div class="adk-hero-actions">
    <a href="getting-started/" class="md-button md-button--primary">Get Started</a>
    <a href="https://github.com/sps014/GoogleAdk-cs" class="md-button">
      GitHub
    </a>
  </div>
</div>

<div class="adk-code-showcase" markdown>

```csharp
using GoogleAdk.Core;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;

AdkEnv.Load();

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "quickstart",
    Model = "gemini-2.5-flash",
    Instruction = "You are a concise, helpful assistant."
});

await ConsoleRunner.RunAsync(agent);
```

</div>

<div class="grid cards" markdown>

-   :material-robot-outline: **Agents**

    ---

    Core execution logic powered by LLMs like `LlmAgent`, or structural orchestrators like `SequentialAgent` and `ParallelAgent`.

    [:octicons-arrow-right-24: Models & Agents](features.md)

-   :material-toolbox-outline: **Tools & Plugins**

    ---

    Actions an agent can take (e.g., Search, API calls) and middleware (e.g., metrics, logging) generated right from your C# code.

    [:octicons-arrow-right-24: Tools Overview](tools.md)

-   :material-server-network: **Runners**

    ---

    Execution engines handling state, streaming, and context. Run in a console, web UI, or background service.

    [:octicons-arrow-right-24: Running Agents](running-agents.md)

-   :material-connection: **Agent-to-Agent (A2A)**

    ---

    Built-in support for Remote Agent-to-Agent communication using the A2A protocol (Client and Server).

    [:octicons-arrow-right-24: Agent-to-Agent](a2a.md)

-   :material-api: **Model Context Protocol (MCP)**

    ---

    Full Model Context Protocol integration allowing dynamic tool discovery and external system connections.

    [:octicons-arrow-right-24: MCP](mcp.md)

-   :material-clipboard-check-outline: **Evaluations**

    ---

    Test your agents effectively with Rubric-based, Hallucination, and LLM-as-a-Judge evaluators out of the box.

    [:octicons-arrow-right-24: Evaluation](evaluation-optimization.md)

-   :material-sitemap: **Orchestration**

    ---

    Coordinate multiple agents to solve complex workflows using Sequential, Parallel, and Loop architectures.

    [:octicons-arrow-right-24: Orchestration](orchestration.md)

-   :material-brain: **Memory & State**

    ---

    Give agents persistent context, session history management, and the ability to recall previous interactions.

    [:octicons-arrow-right-24: Memory](memory.md)

-   :material-map-marker-path: **Planning**

    ---

    Equip your agents with goal-oriented planning capabilities to break down tasks and execute them step-by-step.

    [:octicons-arrow-right-24: Planning](planning.md)

-   :material-cached: **Caching**

    ---

    Prompt and context caching using Gemini-backed implicit caching to lower latency and costs.

    [:octicons-arrow-right-24: Caching](caching.md)

-   :material-puzzle-outline: **Plugins & Telemetry**

    ---

    Lifecycle hooks, streaming events, and OpenTelemetry-style tracing for deep observability.

    [:octicons-arrow-right-24: Plugins](plugins.md)

-   :material-folder-zip-outline: **Skills**

    ---

    File-based modular skills for agents using the `SkillLoader` and standard `SKILL.md` configurations.

    [:octicons-arrow-right-24: Skills](skills.md)

-   :material-volume-high: **Text-to-Speech (TTS)**

    ---

    Generate audio directly from LLMs like `gemini-2.5-flash-preview-tts` alongside your responses.

    [:octicons-arrow-right-24: TTS](tts.md)

-   :material-code-json: **Code Execution**

    ---

    Allow your agents to securely execute generated code and analyze the outputs dynamically in real-time.

    [:octicons-arrow-right-24: Code Execution](code-execution.md)

</div>
