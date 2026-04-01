using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace GoogleAdk.Tools.Mcp;

/// <summary>
/// A tool that wraps an MCP server tool and delegates execution to the MCP server.
/// Translates between ADK and MCP tool formats transparently.
/// </summary>
public sealed class McpTool : Core.BaseTool
{
    private readonly McpClientTool _mcpTool;
    private readonly McpSessionManager _sessionManager;

    internal McpTool(McpClientTool mcpTool, McpSessionManager sessionManager, string? prefix = null)
        : base(
            prefix != null ? $"{prefix}_{mcpTool.Name}" : mcpTool.Name,
            mcpTool.Description ?? string.Empty)
    {
        _mcpTool = mcpTool;
        _sessionManager = sessionManager;
    }

    public override FunctionDeclaration? GetDeclaration()
    {
        return new FunctionDeclaration
        {
            Name = Name,
            Description = Description,
            Parameters = ConvertInputSchema()
        };
    }

    public override async Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        await using var client = await _sessionManager.CreateSessionAsync();
        var result = await client.CallToolAsync(_mcpTool.Name, args);
        // Return text content from the result
        var textParts = result.Content
            .OfType<TextContentBlock>()
            .Select(c => c.Text)
            .ToList();

        return textParts.Count switch
        {
            0 => result.IsError ? "Tool returned an error with no content." : null,
            1 => textParts[0],
            _ => string.Join("\n", textParts)
        };
    }

    private Dictionary<string, object?>? ConvertInputSchema()
    {
        // McpClientTool exposes the JSON schema via the AIFunction interface.
        // We convert it to our generic dictionary-based schema format.
        var jsonSchema = _mcpTool.JsonSchema;
        if (jsonSchema.ValueKind == System.Text.Json.JsonValueKind.Undefined ||
            jsonSchema.ValueKind == System.Text.Json.JsonValueKind.Null)
            return null;

        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonSchema.GetRawText());
    }
}
