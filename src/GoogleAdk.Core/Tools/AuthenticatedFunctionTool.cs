using GoogleAdk.Core.Abstractions.Auth;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Function tool that requires authentication before running.
/// </summary>
public sealed class AuthenticatedFunctionTool : BaseAuthenticatedTool
{
    private readonly Func<Dictionary<string, object?>, AgentContext, Task<object?>> _execute;
    private readonly Dictionary<string, object?>? _parameters;

    public AuthenticatedFunctionTool(
        string name,
        string description,
        AuthConfig authConfig,
        Func<Dictionary<string, object?>, AgentContext, Task<object?>> execute,
        Dictionary<string, object?>? parameters = null)
        : base(name, description, authConfig)
    {
        _execute = execute;
        _parameters = parameters;
    }

    public override FunctionDeclaration? GetDeclaration()
    {
        return new FunctionDeclaration
        {
            Name = Name,
            Description = Description,
            Parameters = _parameters
        };
    }

    public override async Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        if (!EnsureAuthenticated(context))
            return new Dictionary<string, object?> { ["status"] = "auth_required" };

        return await _execute(args, context);
    }
}
