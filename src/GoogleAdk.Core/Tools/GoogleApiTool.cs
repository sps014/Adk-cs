using Google.Apis.Discovery.v1;
using Google.Apis.Services;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

public sealed class GoogleApiTool : BaseTool
{
    public GoogleApiTool()
        : base("google_api_call", "Dynamically calls Google Cloud APIs using Discovery.")
    {
    }

    public override async Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        var apiName = CloudTool.GetString(args, "apiName");
        if (string.IsNullOrEmpty(apiName))
            return CloudTool.MissingArgument("apiName");

        var apiVersion = CloudTool.GetString(args, "apiVersion");
        if (string.IsNullOrEmpty(apiVersion))
            return CloudTool.MissingArgument("apiVersion");

        try
        {
            var discoveryService = new DiscoveryService(new BaseClientService.Initializer());
            var restDescription = await discoveryService.Apis.GetRest(apiName, apiVersion).ExecuteAsync();

            return CloudTool.Success(
                ("api_info", new Dictionary<string, object?>
                {
                    ["id"] = restDescription.Id,
                    ["title"] = restDescription.Title,
                    ["description"] = restDescription.Description,
                    ["version"] = restDescription.Version
                }));
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
                    ["apiName"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The name of the API (e.g., 'compute')."
                    },
                    ["apiVersion"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The version of the API (e.g., 'v1')."
                    }
                },
                ["required"] = new[] { "apiName", "apiVersion" }
            }
        };
    }
}
