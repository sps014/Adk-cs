# Output Schema (Structured Output)

A common requirement for LLM integrations is coercing the model to generate responses strictly conforming to a predefined data structure (typically JSON). The ADK accomplishes this dynamically by leveraging the `typeof` keyword and injecting a specific tool (`set_model_response`) into the agent's context.

## Enforcing Structured Outputs

By specifying an `OutputSchema` on the `LlmAgentConfig`, the ADK converts the C# `Type` into a JSON schema behind the scenes. The agent is forced to use the `SetModelResponseTool` to return the final answer.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using System.Text.Json.Serialization;

// 1. Define the desired C# output object structure
public class SchemaOutput
{
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("confidenceScore")]
    public double? ConfidenceScore { get; set; }
}

// 2. Configure the agent
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "schema_agent",
    Model = "gemini-2.5-flash",
    // Instruct the agent to strictly follow the schema
    Instruction = "Return a JSON response matching the schema using the set_model_response tool.",
    
    // Apply the C# type definition
    OutputSchema = typeof(SchemaOutput)
});

var runner = new InMemoryRunner("output-schema-sample", agent);

var userMessage = new Content
{
    Role = "user",
    Parts = [new Part { Text = "Provide a summary of the Eiffel Tower." }]
};

// 3. Execution
await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
{
    if (evt.IsFinalResponse() && evt.Content?.Parts != null)
    {
        foreach (var part in evt.Content.Parts)
        {
            if (part.Text != null)
            {
                // The output will strictly conform to the SchemaOutput JSON structure
                Console.WriteLine($"Agent: {part.Text}");
                // Example Output: {"summary": "An iron lattice tower on the Champ de Mars...", "confidenceScore": 0.99}
            }
        }
    }
}
```

The underlying JSON object returned by the model is exposed as a string representation in the `Text` field, which you can easily deserialize back into the `SchemaOutput` object natively in your application code.