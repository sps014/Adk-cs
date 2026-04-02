# Code Execution

The ADK natively integrates Google's Code Execution engine, allowing the underlying model to dynamically generate, compile, and execute code (such as Python scripts) during a conversational turn, evaluating the output immediately.

This is exceptionally powerful for data processing, advanced mathematics, or retrieving precise programmatic outputs without writing bespoke external tools.

## Enabling Built-in Code Execution

To grant an agent the ability to execute code, simply attach a `BuiltInCodeExecutor` to the `LlmAgentConfig.CodeExecutor` property. The framework will automatically instruct the LLM on how to utilize it.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.CodeExecutors;
using GoogleAdk.Core.Runner;

// 1. Configure the LLM to use code execution capabilities
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "code_agent",
    Model = "gemini-2.5-flash",
    Instruction = "Use Python code execution for complex calculations.",
    // Attach the executor
    CodeExecutor = new BuiltInCodeExecutor()
});

var runner = new InMemoryRunner("code-exec-app", agent);

var userMessage = new Content
{
    Role = "user",
    Parts = [new Part { Text = "Calculate the mean and standard deviation of [3, 5, 8, 10, 12]." }]
};

// 2. Run the agent.
await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
{
    if (evt.Content?.Parts == null) continue;

    foreach (var part in evt.Content.Parts)
    {
        // 3. Inspect the code the LLM generated and ran
        if (part.ExecutableCode?.Code != null)
        {
            Console.WriteLine($"[Executed Python]:\n{part.ExecutableCode.Code}\n");
        }

        // 4. Inspect the exact output the LLM received from the python runtime
        if (part.CodeExecutionResult != null)
        {
            Console.WriteLine($"[Result]:\n{part.CodeExecutionResult.Output}\n");
        }

        // 5. The LLM's final natural language conclusion
        if (!string.IsNullOrWhiteSpace(part.Text))
        {
            Console.WriteLine($"Agent: {part.Text}");
        }
    }
}
```

The output stream will demonstrate the agent writing a block of Python code utilizing standard libraries, capturing the `stdout` response, and formulating a final user-friendly reply based on that calculation.