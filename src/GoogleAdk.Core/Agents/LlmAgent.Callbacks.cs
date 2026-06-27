namespace GoogleAdk.Core.Agents;

// Callback composition for LlmAgent.
//
// The six callback families (before/after model, model error, before/after tool,
// tool error) all compose the same way: run each callback in the configured list,
// then the single configured callback, and return the first non-null result. The
// shared chain logic lives in InvokeCallbackChainAsync; each Compose* method only
// adapts the call shape for its delegate's argument arity.
public partial class LlmAgent
{
    private static async Task<TResult?> InvokeCallbackChainAsync<TCallback, TResult>(
        List<TCallback>? list,
        TCallback? single,
        Func<TCallback, Task<TResult?>> invoke)
        where TCallback : Delegate
        where TResult : class
    {
        if (list != null)
        {
            foreach (var callback in list)
            {
                var result = await invoke(callback);
                if (result != null) return result;
            }
        }

        return single != null ? await invoke(single) : null;
    }

    private static bool HasNoCallbacks<TCallback>(List<TCallback>? list, TCallback? single)
        where TCallback : Delegate
        => (list == null || list.Count == 0) && single == null;

    private static BeforeModelCallback? ComposeBeforeModel(List<BeforeModelCallback>? list, BeforeModelCallback? single)
        => HasNoCallbacks(list, single)
            ? null
            : (ctx, req) => InvokeCallbackChainAsync(list, single, cb => cb(ctx, req));

    private static AfterModelCallback? ComposeAfterModel(List<AfterModelCallback>? list, AfterModelCallback? single)
        => HasNoCallbacks(list, single)
            ? null
            : (ctx, resp) => InvokeCallbackChainAsync(list, single, cb => cb(ctx, resp));

    private static OnModelErrorCallback? ComposeOnModelError(List<OnModelErrorCallback>? list, OnModelErrorCallback? single)
        => HasNoCallbacks(list, single)
            ? null
            : (ctx, req, err) => InvokeCallbackChainAsync(list, single, cb => cb(ctx, req, err));

    private static BeforeToolCallback? ComposeBeforeTool(List<BeforeToolCallback>? list, BeforeToolCallback? single)
        => HasNoCallbacks(list, single)
            ? null
            : (tool, args, ctx) => InvokeCallbackChainAsync(list, single, cb => cb(tool, args, ctx));

    private static OnToolErrorCallback? ComposeOnToolError(List<OnToolErrorCallback>? list, OnToolErrorCallback? single)
        => HasNoCallbacks(list, single)
            ? null
            : (tool, args, ctx, err) => InvokeCallbackChainAsync(list, single, cb => cb(tool, args, ctx, err));

    private static AfterToolCallback? ComposeAfterTool(List<AfterToolCallback>? list, AfterToolCallback? single)
        => HasNoCallbacks(list, single)
            ? null
            : (tool, args, ctx, resp) => InvokeCallbackChainAsync(list, single, cb => cb(tool, args, ctx, resp));
}
