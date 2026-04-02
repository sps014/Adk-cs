# Plugins

The ADK includes a powerful plugin system that allows you to intercept and modify the execution pipeline at key lifecycle stages. Plugins are ideal for implementing cross-cutting concerns like telemetry, auditing, security gating, and debugging.

## Core Plugin Architecture

You create a custom plugin by inheriting from `BasePlugin` and overriding the desired virtual lifecycle methods.

```csharp
using GoogleAdk.Core.Plugins;
using GoogleAdk.Core.Context;
using GoogleAdk.Core.Abstractions.Events;

public class LoggingPlugin : BasePlugin
{
    private readonly Action<string> _logAction;

    public LoggingPlugin(Action<string> logAction)
    {
        _logAction = logAction;
    }

    // Intercept when the user's initial message is received
    public override Task<Content?> OnUserMessageCallbackAsync(InvocationContext context, Content message)
    {
        _logAction($"[User]: {message.Parts?.FirstOrDefault()?.Text}");
        return Task.FromResult<Content?>(null); // Return null to continue unmodified
    }

    // Intercept generic events emitted by the runner
    public override Task<Event?> OnEventCallbackAsync(InvocationContext context, Event adkEvent)
    {
        _logAction($"[Event]: {adkEvent.Author} emitted {adkEvent.Id}");
        return Task.FromResult<Event?>(null);
    }
}
```

## Security & Policy Engine Plugins

A very common use case for plugins is enforcing execution boundaries. The `SecurityPlugin` acts as a gatekeeper, evaluating tool calls against an `IBasePolicyEngine` before allowing execution.

### Creating a Custom Policy Engine

You can implement an `IBasePolicyEngine` that defines a granular permission set, such as a deny list, allow list, or required confirmations.

```csharp
using GoogleAdk.Core.Plugins;
using GoogleAdk.Core.Context;

public class DenyListPolicyEngine : IBasePolicyEngine
{
    private readonly HashSet<string> _denyList = new() { "dangerous_tool", "drop_table" };
    private readonly HashSet<string> _confirmList = new() { "sensitive_tool" };

    public Task<PolicyCheckResult> EvaluateAsync(ToolCallPolicyContext context)
    {
        var toolName = context.Tool.Name;

        // Immediately block execution
        if (_denyList.Contains(toolName))
            return Task.FromResult(new PolicyCheckResult
            {
                Outcome = PolicyOutcome.Deny,
                Reason = $"Tool '{toolName}' is strictly prohibited."
            });

        // Pause execution and require human confirmation
        if (_confirmList.Contains(toolName))
            return Task.FromResult(new PolicyCheckResult
            {
                Outcome = PolicyOutcome.Confirm,
                Reason = $"Tool '{toolName}' accesses PII and requires manual confirmation."
            });

        // Standard execution
        return Task.FromResult(new PolicyCheckResult
        {
            Outcome = PolicyOutcome.Allow,
            Reason = "Approved."
        });
    }
}
```

## Wiring Plugins to the Runner

Plugins are instantiated and registered globally across your `Runner` via the `RunnerConfig`.

```csharp
var logs = new List<string>();
var loggingPlugin = new LoggingPlugin(msg => logs.Add(msg));

var customPolicy = new DenyListPolicyEngine();
var securityPlugin = new SecurityPlugin(customPolicy);

var runner = new Runner(new RunnerConfig
{
    AppName = "plugins-sample",
    Agent = myAgent,
    // Apply plugins to the execution pipeline
    Plugins = [ loggingPlugin, securityPlugin ]
});
```