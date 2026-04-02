using GoogleAdk.Core.Abstractions.Tools;

namespace GoogleAdk.Core.Tests;

public static partial class GeneratedTools
{
    /// <summary>Adds two numbers.</summary>
    /// <param name="a">First number.</param>
    /// <param name="b">Second number.</param>
    [FunctionTool]
    public static int AddTool(int a, int b) => a + b;

    /// <summary>Test tool.</summary>
    [FunctionTool]
    public static object? TestTool() => null;

    /// <summary>Greets a person.</summary>
    /// <param name="name">The name to greet.</param>
    [FunctionTool]
    public static string Greet(string name) => $"Hello {name}!";

    /// <summary>No-op tool used for wiring tests.</summary>
    [FunctionTool]
    public static object? Noop() => null;

    /// <summary>Always throws to simulate tool failure.</summary>
    [FunctionTool]
    public static object? FailTool() => throw new InvalidOperationException("boom");

    /// <summary>A safe tool for policy tests.</summary>
    [FunctionTool]
    public static object? SafeTool() => "ok";

    /// <summary>A dangerous tool for policy tests.</summary>
    [FunctionTool]
    public static object? DangerousTool() => "ok";

    /// <summary>A sensitive tool for policy tests.</summary>
    [FunctionTool]
    public static object? SensitiveTool() => "ok";

    /// <summary>A tool used by in-memory policy engine tests.</summary>
    [FunctionTool]
    public static object? AnyTool() => "ok";
}
