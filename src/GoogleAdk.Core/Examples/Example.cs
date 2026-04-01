using GoogleAdk.Core.Abstractions.Models;

namespace GoogleAdk.Core.Examples;

/// <summary>
/// Represents a few-shot example with input and output turns.
/// </summary>
public sealed class Example
{
    public Content Input { get; set; } = new();
    public List<Content> Output { get; set; } = new();
}
