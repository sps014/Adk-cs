using GoogleAdk.Core.Abstractions.Models;

namespace GoogleAdk.Core.Abstractions.Events;

/// <summary>
/// The minimal contract a tool must expose so it can be referenced from an
/// <see cref="LlmRequest"/> and surfaced to the model as a callable function.
/// </summary>
public interface IBaseTool
{
    /// <summary>The unique tool name presented to the model.</summary>
    string Name { get; }

    /// <summary>A human-readable description of what the tool does.</summary>
    string Description { get; }

    /// <summary>
    /// Whether the tool runs asynchronously beyond a single model turn
    /// (for example, a human-in-the-loop or long-running operation).
    /// </summary>
    bool IsLongRunning { get; }

    /// <summary>
    /// Returns the function declaration advertised to the model, or
    /// <see langword="null"/> when the tool is not a function-style tool.
    /// </summary>
    FunctionDeclaration? GetDeclaration();
}
