# Auth Tool and Authentication

The ADK handles authenticated tools using `AuthConfig` and `AgentContext`. When a tool requires user credentials (like OAuth2), the ADK halts the agent, signals the frontend, and resumes once authorized.

## Usage

Define an `AuthConfig` and request credentials inside your `[FunctionTool]`.

```csharp
using GoogleAdk.Core.Abstractions.Auth;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Agents;

public static partial class AuthTools
{
    public static AuthConfig CalendarAuth { get; } = new AuthConfig
    {
        CredentialKey = "calendar_oauth",
        AuthScheme = new AuthScheme
        {
            Type = AuthSchemeType.OAuth2,
            Flows = new OAuth2Flows
            {
                AuthorizationCode = new OAuth2Flow
                {
                    AuthorizationUrl = "https://accounts.example.com/auth",
                    TokenUrl = "https://oauth2.example.com/token",
                }
            }
        }
    };

    [FunctionTool(Name = "calendar_events")]
    public static object? GetEvents(AgentContext ctx)
    {
        var credential = ctx.GetAuthResponse(CalendarAuth);
        
        if (credential == null)
        {
            // Request credentials; the ADK will halt the tool call here
            ctx.RequestCredential(CalendarAuth);
            return new { status = "auth_required" };
        }

        // Action utilizing credential.OAuth2Auth.AccessToken
        return new { title = "Project Sync" };
    }
}
```

## Runner Integration

Monitor output events for `RequestedAuthConfigs` and pause your app to perform the OAuth flow.

```csharp
await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
{
    if (evt.Actions.RequestedAuthConfigs.Count > 0)
    {
        // PAUSE EXECUTION: Output auth URL and wait for user token
        var config = evt.Actions.RequestedAuthConfigs.Values.First();
        break; 
    }
}
```

Resume with the new `stateDelta`:

```csharp
var authState = new Dictionary<string, object?>
{
    ["temp:" + AuthTools.CalendarAuth.CredentialKey] = new AuthCredential
    {
        AuthType = AuthCredentialType.OAuth2,
        OAuth2Auth = new OAuth2Auth { AccessToken = "ya29.token" }
    }
};

// Resume execution
await foreach (var evt in runner.RunAsync("user-1", session.Id, followup, authState)) { }
```