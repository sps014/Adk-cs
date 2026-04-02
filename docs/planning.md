# Planning

Standard LLMs utilize "autoregressive" generation, meaning they predict the next token immediately without explicitly "thinking" ahead. While powerful, this struggles with multi-step reasoning tasks (e.g., "Schedule a meeting, but if X is busy, email Y instead").

The ADK solves this by integrating **Planners** into the agent execution pipeline.

## The ReAct Planner

The most common implementation provided by the ADK is the `PlanReActPlanner`. ReAct (Reasoning + Acting) fundamentally changes how the model is prompted. It instructs the LLM to output an explicit "Thought" block formulating its internal logic and strategy prior to executing a tool or delivering an action.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Planning;
using GoogleAdk.Core.Runner;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "planner_agent",
    Model = "gemini-2.5-flash",
    Instruction = "You are a highly logical and meticulous assistant.",
    
    // Inject the ReAct planner into the middleware pipeline
    Planner = new PlanReActPlanner()
});

var runner = new InMemoryRunner("planning-sample", agent);
var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest { AppName = "planning-sample", UserId = "user-1" });

var userMessage = new Content
{
    Role = "user",
    Parts = [new Part { Text = "Plan a weekend itinerary for Paris based on typical weather." }]
};

await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
{
    var text = evt.Content?.Parts?.FirstOrDefault()?.Text;
    if (!string.IsNullOrWhiteSpace(text))
    {
        Console.WriteLine(text);
    }
}
```

Behind the scenes, the `NlPlanningRequestProcessor` intercepts the request, appends strict instructional logic requiring "Thought" definitions, and handles the `NlPlanningResponseProcessor` mapping during response parsing.

## Built-in Planner Configuration

For models that natively support specific "thinking" mechanisms directly via their REST API schemas, you can utilize the `BuiltInPlanner` to pass specific configurations directly to the underlying model adapter.

```csharp
// Configures the LLM provider's native "thinking" mechanism (e.g., high-reasoning mode)
var builtInPlanner = new BuiltInPlanner(new Dictionary<string, object?> 
{ 
    ["mode"] = "fast" 
});

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "builtin_planner_agent",
    Model = "gemini-2.5-flash",
    Planner = builtInPlanner
});
```