# Auth Tool and Authentication

The ADK provides a unified framework for handling authenticated tools. When a tool requires user credentials (like an API Key or an OAuth2 token), the ADK handles the interruption, signals the frontend/client that authorization is required, and resumes the tool call once the credential is provided.

## Overview

Authentication is built around `AuthConfig` and the `AgentContext`. Instead of hardcoding tokens, tools request them dynamically.

## Defining an Authenticated Tool

Using the `[FunctionTool]` source generator, you define an `AuthConfig` and use the injected `AgentContext` to request credentials.

```csharp
using GoogleAdk.Core.Abstractions.Auth;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Agents;

public static partial class SampleAuthTools
{
    public static AuthConfig CalendarAuthConfig { get; } = new AuthConfig
    {
        CredentialKey = "calendar_oauth",
        AuthScheme = new AuthScheme
        {
            Type = AuthSchemeType.OAuth2,
            Description = "OAuth2 authorization code flow for calendar access",
            Flows = new OAuth2Flows
            {
                AuthorizationCode = new OAuth2Flow
                {
                    AuthorizationUrl = "https://accounts.example.com/o/oauth2/v2/auth",
                    TokenUrl = "https://oauth2.example.com/token",
                    Scopes = new Dictionary<string, string> { ["calendar.read"] = "Read calendar events" }
                }
            }
        }
    };

    [FunctionTool(Name = "calendar_next_event")]
    public static object? CalendarNextEvent(string timezone, AgentContext ctx)
    {
        // Check if we already have the credential in the current state
        var credential = ctx.GetAuthResponse(CalendarAuthConfig);
        
        if (credential == null)
        {
            // We don't have it. Request it. The ADK will halt the tool and bubble this up.
            ctx.RequestCredential(CalendarAuthConfig);
            return new Dictionary<string, object?> { ["status"] = "auth_required" };
        }

        // Proceed with the authenticated action using credential.OAuth2Auth.AccessToken
        return new Dictionary<string, object?>
        {
            ["title"] = "Project Sync",
            ["start"] = "2026-04-02T09:30:00-07:00",
        };
    }
}
```

## Handling Auth Events in the Runner

When the agent executes, you must monitor the events for `RequestedAuthConfigs`. If auth is requested, your application (or API) must pause, perform the OAuth flow or ask the user for a token, and then resume the runner with the new `stateDelta`.

```csharp
await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
{
    if (evt.Actions.RequestedAuthConfigs.Count > 0)
    {
        // The LLM tried to use the calendar tool, but we need auth.
        var authConfig = evt.Actions.RequestedAuthConfigs.Values.First() as AuthConfig;
        
        // Output the authorization URL to the user...
        Console.WriteLine($"Please authorize at: {authConfig.AuthScheme.Flows.AuthorizationCode.AuthorizationUrl}");
        
        // PAUSE EXECUTION. Wait for user to complete OAuth.
        break; 
    }
}
```

Once the user completes the flow and you have the token, you provide it back to the agent in the next run cycle using `stateDelta`:

```csharp
// The user completed OAuth. We got a token.
var authState = new Dictionary<string, object?>
{
    // The key must match the CredentialKey prefixed with "temp:" for ephemeral state
    ["temp:" + CalendarAuthConfig.CredentialKey] = new AuthCredential
    {
        AuthType = AuthCredentialType.OAuth2,
        OAuth2Auth = new OAuth2Auth { AccessToken = "ya29.real-token-here" }
    }
};

var followup = new Content { Role = "user", Parts = [new Part { Text = "Continue." }] };

// Resume the agent with the auth state
await foreach (var evt in runner.RunAsync("user-1", session.Id, followup, authState))
{
    // The tool will now execute successfully
}
```

## Example

For a complete working example, see the [Auth Sample](../samples/GoogleAdk.Samples.Auth) in the repository.