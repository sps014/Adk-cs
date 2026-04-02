using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Samples.FeatureFlags;

public static partial class SampleFeatureTools
{
    /// <summary>Adds a UI widget to the event stream.</summary>
    [FunctionTool(Name = "render_widget")]
    public static object? RenderWidget(AgentContext context)
    {
        context.RenderUiWidget(new UiWidget { Id = "widget-1", Provider = "mcp" });
        return "ok";
    }
}
