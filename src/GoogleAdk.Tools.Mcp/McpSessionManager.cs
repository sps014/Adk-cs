using ModelContextProtocol.Client;

namespace GoogleAdk.Tools.Mcp;

/// <summary>
/// Creates MCP client sessions based on connection parameters.
/// </summary>
public sealed class McpSessionManager
{
    private readonly McpConnectionParams _connectionParams;

    public McpSessionManager(McpConnectionParams connectionParams)
    {
        _connectionParams = connectionParams;
    }

    public async Task<IMcpClient> CreateSessionAsync()
    {
        IClientTransport transport = _connectionParams switch
        {
            StdioConnectionParams stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "AdkMcpClient",
                Command = stdio.Command,
                Arguments = stdio.Arguments?.ToList(),
                EnvironmentVariables = stdio.EnvironmentVariables?.ToDictionary(kv => kv.Key, kv => (string?)kv.Value),
                WorkingDirectory = stdio.WorkingDirectory,
            }),
            HttpConnectionParams http => new SseClientTransport(new SseClientTransportOptions
            {
                Endpoint = new Uri(http.Url),
                Name = "AdkMcpClient",
            }),
            _ => throw new NotSupportedException($"Unsupported connection params type: {_connectionParams.GetType().Name}")
        };

        return await McpClientFactory.CreateAsync(transport);
    }
}
