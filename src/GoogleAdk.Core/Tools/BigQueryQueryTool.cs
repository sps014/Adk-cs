using Google.Cloud.BigQuery.V2;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

public sealed class BigQueryQueryTool : BaseTool
{
    public BigQueryQueryTool()
        : base("bigquery_query", "Executes a query in BigQuery.")
    {
    }

    public override async Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        var projectId = CloudTool.GetString(args, "projectId");
        if (string.IsNullOrEmpty(projectId))
            return CloudTool.MissingArgument("projectId");

        var query = CloudTool.GetString(args, "query");
        if (string.IsNullOrEmpty(query))
            return CloudTool.MissingArgument("query");

        try
        {
            var client = await BigQueryClient.CreateAsync(projectId);
            var results = await client.ExecuteQueryAsync(query, parameters: null);

            var rows = new List<Dictionary<string, object?>>();
            foreach (var row in results)
            {
                var dict = new Dictionary<string, object?>();
                foreach (var field in results.Schema.Fields)
                {
                    dict[field.Name] = row[field.Name];
                }
                rows.Add(dict);
            }

            return CloudTool.Success(("rows", rows));
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
                    ["query"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The SQL query to execute."
                    }
                },
                ["required"] = new[] { "projectId", "query" }
            }
        };
    }
}
