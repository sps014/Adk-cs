using System.Text.Json;
using GoogleAdk.Core.Abstractions.Auth;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Auth;

namespace GoogleAdk.Core.Agents.Processors;

/// <summary>
/// Resumes tool execution after the client returns requested auth credentials.
///
/// When the previous (user-authored) event carries <c>adk_request_credential</c>
/// function responses, this processor stores the returned credentials in session
/// state and re-executes the original tool calls that were waiting on auth,
/// mirroring adk-python's <c>auth_preprocessor</c>.
/// </summary>
public class AuthLlmRequestProcessor : BaseLlmRequestProcessor
{
    public static readonly AuthLlmRequestProcessor Instance = new();

    /// <summary>
    /// Prefix used by toolset auth credential IDs. Auth requests with this prefix
    /// are for toolset authentication and don't map to a resumable function call.
    /// </summary>
    private const string ToolsetAuthCredentialIdPrefix = "_adk_toolset_auth_";

    private static readonly JsonSerializerOptions s_jsonOptions = GoogleAdk.Core.Abstractions.Json.AdkJson.CamelCaseCaseInsensitive;

    public override async IAsyncEnumerable<Event> RunAsync(
        InvocationContext invocationContext,
        LlmRequest llmRequest)
    {
        if (invocationContext.Agent is not LlmAgent agent)
            yield break;

        var events = invocationContext.Session.Events;
        if (events == null || events.Count == 0)
            yield break;

        // Find the last event with content; it must be a user-authored event
        // carrying the adk_request_credential function responses.
        Event? lastEventWithContent = null;
        for (var i = events.Count - 1; i >= 0; i--)
        {
            if (events[i].Content != null)
            {
                lastEventWithContent = events[i];
                break;
            }
        }

        if (lastEventWithContent == null || lastEventWithContent.Author != "user")
            yield break;

        var responses = lastEventWithContent.GetFunctionResponses();
        if (responses.Count == 0)
            yield break;

        // Collect adk_request_credential function response IDs and their response dicts.
        var authResponses = new Dictionary<string, Dictionary<string, object?>?>();
        foreach (var response in responses)
        {
            if (response.Name != FunctionCallHandler.RequestEucFunctionCallName)
                continue;
            if (response.Id == null)
                continue;
            authResponses[response.Id] = response.Response;
        }

        if (authResponses.Count == 0)
            yield break;

        var toolsToResume = StoreAuthAndCollectResumeTargets(
            events, authResponses, new Abstractions.Sessions.State(invocationContext.Session.State));
        if (toolsToResume.Count == 0)
            yield break;

        // Find the original function call event and re-execute the tools that needed auth.
        for (var i = events.Count - 2; i >= 0; i--)
        {
            var evt = events[i];
            var functionCalls = evt.GetFunctionCalls();
            if (functionCalls.Count == 0)
                continue;

            if (functionCalls.Any(fc => fc.Id != null && toolsToResume.Contains(fc.Id)))
            {
                var tools = await agent.CanonicalToolsAsync(new ReadonlyContext(invocationContext));
                var toolsDict = tools.ToDictionary(t => t.Name, t => (IBaseTool)t);

                var responseEvent = await FunctionCallHandler.HandleFunctionCallsAsync(
                    invocationContext,
                    evt,
                    toolsDict,
                    agent.BeforeToolCallback,
                    agent.OnToolErrorCallback,
                    agent.AfterToolCallback,
                    toolConfirmations: null,
                    filterFunctionCallIds: toolsToResume);

                if (responseEvent != null)
                    yield return responseEvent;
                yield break;
            }
        }
    }

    /// <summary>
    /// Stores auth credentials and returns the original function call IDs to resume.
    /// </summary>
    private static HashSet<string> StoreAuthAndCollectResumeTargets(
        IReadOnlyList<Event> events,
        Dictionary<string, Dictionary<string, object?>?> authResponses,
        Abstractions.Sessions.State state)
    {
        // Scan events for matching adk_request_credential function calls to extract
        // their AuthToolArguments (which carry credential_key + function_call_id).
        var argsById = new Dictionary<string, AuthToolArguments>();
        foreach (var evt in events)
        {
            foreach (var fc in evt.GetFunctionCalls())
            {
                if (fc.Id == null || !authResponses.ContainsKey(fc.Id))
                    continue;
                if (fc.Name != FunctionCallHandler.RequestEucFunctionCallName)
                    continue;
                if (TryParseAuthToolArguments(fc.Args, out var args))
                    argsById[fc.Id] = args;
            }
        }

        // Store credentials. Merge credential_key from the original request into the
        // client's auth response before storing.
        foreach (var (fcId, responseDict) in authResponses)
        {
            if (responseDict == null)
                continue;
            if (!TryCoerce<AuthConfig>(responseDict, out var authConfig) || authConfig == null)
                continue;

            if (argsById.TryGetValue(fcId, out var requested)
                && !string.IsNullOrEmpty(requested.AuthConfig.CredentialKey))
            {
                authConfig.CredentialKey = requested.AuthConfig.CredentialKey;
            }

            new AuthHandler(authConfig).ParseAndStoreAuthResponse(state);
        }

        // Collect original function call IDs to resume, skipping toolset auth entries.
        var toolsToResume = new HashSet<string>();
        foreach (var (_, requested) in argsById)
        {
            if (string.IsNullOrEmpty(requested.FunctionCallId))
                continue;
            if (requested.FunctionCallId.StartsWith(ToolsetAuthCredentialIdPrefix, StringComparison.Ordinal))
                continue;
            toolsToResume.Add(requested.FunctionCallId);
        }

        return toolsToResume;
    }

    private static bool TryParseAuthToolArguments(Dictionary<string, object?>? args, out AuthToolArguments result)
    {
        result = new AuthToolArguments();
        if (args == null)
            return false;

        var functionCallId = args.TryGetValue("function_call_id", out var fcId) ? fcId?.ToString() : null;
        if (functionCallId != null)
            result.FunctionCallId = functionCallId;

        if (args.TryGetValue("auth_config", out var authConfigValue)
            && TryCoerce<AuthConfig>(authConfigValue, out var authConfig)
            && authConfig != null)
        {
            result.AuthConfig = authConfig;
            return true;
        }

        return !string.IsNullOrEmpty(result.FunctionCallId);
    }

    private static bool TryCoerce<T>(object? value, out T? result) where T : class
    {
        result = null;
        if (value == null)
            return false;
        if (value is T typed)
        {
            result = typed;
            return true;
        }

        try
        {
            var json = value is JsonElement element
                ? element.GetRawText()
                : JsonSerializer.Serialize(value, s_jsonOptions);
            result = JsonSerializer.Deserialize<T>(json, s_jsonOptions);
            return result != null;
        }
        catch
        {
            return false;
        }
    }
}
