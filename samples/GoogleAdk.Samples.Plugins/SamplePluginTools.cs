using GoogleAdk.Core.Abstractions.Tools;

namespace GoogleAdk.Samples.Plugins;

public static partial class SamplePluginTools
{
    /// <summary>A safe tool for policy checks.</summary>
    [FunctionTool(Name = "safe_tool")]
    public static object? SafeTool() => "ok";

    /// <summary>A dangerous tool for policy checks.</summary>
    [FunctionTool(Name = "dangerous_tool")]
    public static object? DangerousTool() => "ok";

    /// <summary>A sensitive tool for policy checks.</summary>
    [FunctionTool(Name = "sensitive_tool")]
    public static object? SensitiveTool() => "ok";
}
