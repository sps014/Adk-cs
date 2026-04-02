using GoogleAdk.Core.Abstractions.Auth;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Base tool that requests credentials before execution.
/// </summary>
public abstract class BaseAuthenticatedTool : BaseTool
{
    protected AuthConfig AuthConfig { get; }

    protected BaseAuthenticatedTool(string name, string description, AuthConfig authConfig)
        : base(name, description)
    {
        AuthConfig = authConfig;
    }

    protected bool EnsureAuthenticated(AgentContext context)
    {
        var credential = context.GetAuthResponse(AuthConfig);
        if (credential == null)
        {
            context.RequestCredential(AuthConfig);
            return false;
        }
        return true;
    }
}
