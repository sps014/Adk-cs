using System.Diagnostics;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Executes a shell command with a prefix allowlist.
/// </summary>
public sealed class ExecuteBashTool : BaseTool
{
    private readonly IReadOnlyList<string> _allowedPrefixes;

    public ExecuteBashTool(IEnumerable<string>? allowedPrefixes = null)
        : base("bash", "Executes a shell command.")
    {
        _allowedPrefixes = (allowedPrefixes ?? new[] { "echo", "dir", "type" }).ToList();
    }

    public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        var command = args.GetValueOrDefault("command")?.ToString() ?? string.Empty;
        if (!_allowedPrefixes.Any(p => command.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult<object?>(new Dictionary<string, object?>
            {
                ["error"] = "Command not allowed by policy."
            });
        }

        var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var proc = Process.Start(psi);
        if (proc == null)
            return Task.FromResult<object?>(new Dictionary<string, object?> { ["error"] = "Failed to start process." });

        var output = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit(5000);

        return Task.FromResult<object?>(new Dictionary<string, object?>
        {
            ["output"] = output,
            ["error"] = error
        });
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
                    ["command"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "Shell command to execute."
                    }
                },
                ["required"] = new[] { "command" }
            }
        };
    }
}
