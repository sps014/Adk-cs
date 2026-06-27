using Google.Cloud.Spanner.Data;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using System.Data.Common;

namespace GoogleAdk.Core.Tools;

public sealed class SpannerQueryTool : BaseTool
{
    public SpannerQueryTool()
        : base("spanner_query", "Executes a SQL query against a Cloud Spanner database.")
    {
    }

    public override async Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        var projectId = CloudTool.GetString(args, "projectId");
        if (string.IsNullOrEmpty(projectId))
            return CloudTool.MissingArgument("projectId");

        var instanceId = CloudTool.GetString(args, "instanceId");
        if (string.IsNullOrEmpty(instanceId))
            return CloudTool.MissingArgument("instanceId");

        var databaseId = CloudTool.GetString(args, "databaseId");
        if (string.IsNullOrEmpty(databaseId))
            return CloudTool.MissingArgument("databaseId");

        var query = CloudTool.GetString(args, "query");
        if (string.IsNullOrEmpty(query))
            return CloudTool.MissingArgument("query");

        string connectionString = $"Data Source=projects/{projectId}/instances/{instanceId}/databases/{databaseId}";

        try
        {
            using var connection = new SpannerConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;

            if (query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = await command.ExecuteReaderAsync();
                var rows = new List<Dictionary<string, object?>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    rows.Add(row);
                }

                return CloudTool.Success(("rows", rows));
            }
            else
            {
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return CloudTool.Success(("rows_affected", rowsAffected));
            }
        }
        catch (Exception ex)
        {
            return CloudTool.Error(ex);
        }
    }

    public override FunctionDeclaration? GetDeclaration()
    {
        return new FunctionDeclaration
        {
            Name = Name,
            Description = Description,
            Parameters = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object?>
                {
                    ["projectId"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The Google Cloud project ID."
                    },
                    ["instanceId"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The Cloud Spanner instance ID."
                    },
                    ["databaseId"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The Cloud Spanner database ID."
                    },
                    ["query"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The SQL query to execute."
                    }
                },
                ["required"] = new[] { "projectId", "instanceId", "databaseId", "query" }
            }
        };
    }
}
