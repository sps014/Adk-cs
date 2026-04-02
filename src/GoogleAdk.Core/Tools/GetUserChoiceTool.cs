using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Long-running tool that requests the user to choose among options.
/// </summary>
public sealed class GetUserChoiceTool : LongRunningFunctionTool
{
    public GetUserChoiceTool()
        : base(
            name: "get_user_choice",
            description: "Presents the user with options and waits for a choice.",
            execute: (args, ctx) =>
            {
                var options = args.GetValueOrDefault("options");
                return Task.FromResult<object?>(new Dictionary<string, object?>
                {
                    ["status"] = "pending",
                    ["options"] = options
                });
            },
            parameters: new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object?>
                {
                    ["prompt"] = new Dictionary<string, object?> { ["type"] = "string" },
                    ["options"] = new Dictionary<string, object?> { ["type"] = "array" }
                },
                ["required"] = new[] { "options" }
            })
    {
    }
}
