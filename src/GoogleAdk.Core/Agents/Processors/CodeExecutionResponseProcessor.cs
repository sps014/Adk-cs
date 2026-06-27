using System.Text;
using GoogleAdk.Core.Abstractions.Artifacts;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.CodeExecutors;

namespace GoogleAdk.Core.Agents.Processors;

/// <summary>
/// Processes code-execution responses: extracts the first code block from a model
/// response, executes it, emits the code and result events, and clears the model
/// response so the generation loop continues until the model stops emitting code.
/// Mirrors adk-python's <c>_CodeExecutionResponseProcessor</c>.
/// </summary>
public sealed class CodeExecutionResponseProcessor : BaseLlmResponseProcessor
{
    public static readonly CodeExecutionResponseProcessor Instance = new();

    public override async IAsyncEnumerable<Event> RunAsync(
        InvocationContext invocationContext,
        LlmResponse llmResponse)
    {
        // Skip streaming chunks.
        if (llmResponse.Partial == true)
            yield break;

        if (invocationContext.Agent is not LlmAgent agent)
            yield break;

        var executor = agent.CodeExecutor;
        if (executor == null)
            yield break;

        if (llmResponse.Content?.Parts == null || llmResponse.Content.Parts.Count == 0)
            yield break;

        // Built-in (model-side) code execution: persist any generated images as
        // artifacts and emit an actions-only event.
        if (executor is BuiltInCodeExecutor)
        {
            var actions = new EventActions();
            foreach (var part in llmResponse.Content.Parts)
            {
                if (part.InlineData == null || !part.InlineData.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (invocationContext.ArtifactService == null)
                    throw new InvalidOperationException("Artifact service is not initialized.");

                var fileName = !string.IsNullOrEmpty(part.InlineData.DisplayName)
                    ? part.InlineData.DisplayName!
                    : $"{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.{part.InlineData.MimeType.Split('/')[^1]}";

                var version = await invocationContext.ArtifactService.SaveArtifactAsync(new SaveArtifactRequest
                {
                    AppName = invocationContext.AppName,
                    UserId = invocationContext.UserId,
                    SessionId = invocationContext.Session.Id,
                    Filename = fileName,
                    Artifact = new Part { InlineData = part.InlineData },
                });
                actions.ArtifactDelta[fileName] = version;
                part.InlineData = null;
                part.Text = $"Saved as artifact: {fileName}. ";
            }

            yield return Event.Create(e =>
            {
                e.InvocationId = invocationContext.InvocationId;
                e.Author = agent.Name;
                e.Branch = invocationContext.Branch;
                e.Actions = actions;
            });
            yield break;
        }

        var state = new State(invocationContext.Session.State);
        var context = new CodeExecutorContext(state);

        if (context.GetErrorCount() >= executor.ErrorRetryAttempts)
            yield break;

        // Extract code from the model response and truncate to the first code block.
        var responseContent = llmResponse.Content;
        var code = ExtractCodeAndTruncateContent(responseContent, executor.CodeBlockDelimiters);

        // Terminal state: no code to execute.
        if (string.IsNullOrEmpty(code))
            yield break;

        // Emit the (truncated) code event.
        yield return Event.Create(e =>
        {
            e.InvocationId = invocationContext.InvocationId;
            e.Author = agent.Name;
            e.Branch = invocationContext.Branch;
            e.Content = responseContent;
        });

        var executionId = GetOrSetExecutionId(invocationContext, executor, context);
        var result = await executor.ExecuteCodeAsync(
            invocationContext,
            new CodeExecutionInput
            {
                Code = code!,
                InputFiles = context.GetInputFiles(),
                ExecutionId = executionId,
            });

        // Track consecutive errors for retry handling.
        if (!string.IsNullOrEmpty(result.Stderr))
            context.SetErrorCount(context.GetErrorCount() + 1);
        else
            context.SetErrorCount(0);

        yield return await BuildResultEventAsync(invocationContext, executor, context, result);

        // Clear the model response so the generation loop continues.
        llmResponse.Content = null;
    }

    private static string? GetOrSetExecutionId(
        InvocationContext invocationContext,
        BaseCodeExecutor executor,
        CodeExecutorContext context)
    {
        if (!executor.Stateful)
            return null;

        var executionId = context.GetExecutionId();
        if (string.IsNullOrEmpty(executionId))
        {
            executionId = invocationContext.Session.Id;
            context.SetExecutionId(executionId);
        }
        return executionId;
    }

    private static async Task<Event> BuildResultEventAsync(
        InvocationContext invocationContext,
        BaseCodeExecutor executor,
        CodeExecutorContext context,
        CodeExecutionOutput result)
    {
        var hasError = !string.IsNullOrEmpty(result.Stderr);
        var resultPart = new Part
        {
            CodeExecutionResult = new CodeExecutionResult
            {
                Outcome = hasError ? "OUTCOME_FAILED" : "OUTCOME_OK",
                Output = hasError ? result.Stderr : result.Stdout,
            }
        };

        var actions = new EventActions();
        foreach (var (k, v) in context.GetStateDelta())
            actions.StateDelta[k] = v;

        // Persist output files as artifacts.
        if (result.OutputFiles.Count > 0)
        {
            if (invocationContext.ArtifactService == null)
                throw new InvalidOperationException("Artifact service is not initialized.");

            foreach (var outputFile in result.OutputFiles)
            {
                var version = await invocationContext.ArtifactService.SaveArtifactAsync(new SaveArtifactRequest
                {
                    AppName = invocationContext.AppName,
                    UserId = invocationContext.UserId,
                    SessionId = invocationContext.Session.Id,
                    Filename = outputFile.Name,
                    Artifact = new Part { InlineData = new InlineData { MimeType = outputFile.MimeType, Data = outputFile.Content } },
                });
                actions.ArtifactDelta[outputFile.Name] = version;
            }
        }

        return Event.Create(e =>
        {
            e.InvocationId = invocationContext.InvocationId;
            e.Author = invocationContext.Agent.Name;
            e.Branch = invocationContext.Branch;
            e.Content = new Content { Role = "model", Parts = new List<Part> { resultPart } };
            e.Actions = actions;
        });
    }

    /// <summary>
    /// Extracts the first code block from the content and truncates the content to
    /// the part containing it. Returns the code, or null if no code is found.
    /// Recognizes explicit <see cref="ExecutableCode"/> parts as well as fenced
    /// code blocks within text parts (using the configured delimiters).
    /// </summary>
    private static string? ExtractCodeAndTruncateContent(
        Content content,
        List<(string Open, string Close)> delimiters)
    {
        if (content.Parts == null)
            return null;

        for (var i = 0; i < content.Parts.Count; i++)
        {
            var part = content.Parts[i];

            // 1) Explicit executable code part.
            if (!string.IsNullOrEmpty(part.ExecutableCode?.Code))
            {
                content.Parts = content.Parts.Take(i + 1).ToList();
                return part.ExecutableCode!.Code;
            }

            // 2) Fenced code block inside a text part.
            if (part.Text != null)
            {
                foreach (var (open, close) in delimiters)
                {
                    var code = ExtractFencedCode(part.Text, open, close);
                    if (code != null)
                    {
                        // Truncate the content to this part and keep only the text
                        // up to and including the code block.
                        var endIndex = part.Text.IndexOf(close, part.Text.IndexOf(open, StringComparison.Ordinal) + open.Length, StringComparison.Ordinal);
                        if (endIndex >= 0)
                            part.Text = part.Text.Substring(0, endIndex + close.Length);
                        content.Parts = content.Parts.Take(i + 1).ToList();
                        return code;
                    }
                }
            }
        }

        return null;
    }

    private static string? ExtractFencedCode(string text, string open, string close)
    {
        var start = text.IndexOf(open, StringComparison.Ordinal);
        if (start < 0)
            return null;
        var codeStart = start + open.Length;
        var end = text.IndexOf(close, codeStart, StringComparison.Ordinal);
        if (end < 0)
            return null;
        return text.Substring(codeStart, end - codeStart);
    }
}
