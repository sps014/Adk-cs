using System.Text.Json;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Tool to allow structured output when tools are also used.
/// </summary>
public sealed class SetModelResponseTool : BaseTool
{
    public const string ToolName = "set_model_response";
    private readonly Dictionary<string, object?> _schema;

    public SetModelResponseTool(Dictionary<string, object?> schema)
        : base(ToolName, "Sets the final structured model response.")
    {
        _schema = schema;
    }

    public override FunctionDeclaration? GetDeclaration()
    {
        return new FunctionDeclaration
        {
            Name = ToolName,
            Description = "Sets the final structured response for the user.",
            Parameters = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object?>
                {
                    ["result"] = _schema
                },
                ["required"] = new[] { "result" }
            }
        };
    }

    public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        if (args.TryGetValue("result", out var result))
            return Task.FromResult(result);
        return Task.FromResult<object?>(args);
    }

    public static string? TryExtractStructuredResponse(Event functionResponseEvent)
    {
        if (functionResponseEvent.Content?.Parts == null)
            return null;

        foreach (var part in functionResponseEvent.Content.Parts)
        {
            var response = part.FunctionResponse;
            if (response == null || response.Name != ToolName || response.Response == null)
                continue;

            if (response.Response.TryGetValue("result", out var result))
                return JsonSerializer.Serialize(result);

            return JsonSerializer.Serialize(response.Response);
        }

        return null;
    }
}
