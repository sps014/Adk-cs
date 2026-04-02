using GoogleAdk.Core.Abstractions.Tools;

namespace GoogleAdk.Samples.OutputSchema;

public static partial class SampleSchemaTools
{
    /// <summary>No-op tool used to enable output schema flow.</summary>
    [FunctionTool(Name = "noop")]
    public static object? Noop() => null;
}
