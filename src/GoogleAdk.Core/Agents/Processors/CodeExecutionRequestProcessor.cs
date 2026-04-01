using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.CodeExecutors;

namespace GoogleAdk.Core.Agents.Processors;

/// <summary>
/// Preprocesses LLM requests to enable code execution workflows.
/// </summary>
public class CodeExecutionRequestProcessor : BaseLlmRequestProcessor
{
    public static readonly CodeExecutionRequestProcessor Instance = new();

    private static readonly HashSet<string> SupportedInlineDataTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/csv",
    };

    public override async IAsyncEnumerable<Event> RunAsync(
        InvocationContext invocationContext,
        LlmRequest llmRequest)
    {
        await Task.CompletedTask;

        if (invocationContext.Agent is not LlmAgent agent)
            yield break;

        var executor = agent.CodeExecutor;
        if (executor == null)
            yield break;

        if (executor is BuiltInCodeExecutor builtIn)
        {
            builtIn.ProcessLlmRequest(llmRequest);
            yield break;
        }

        if (!executor.OptimizeDataFile)
            yield break;

        var state = new State(invocationContext.Session.State);
        var context = new CodeExecutorContext(state);

        if (context.GetErrorCount() >= executor.ErrorRetryAttempts)
            yield break;

        var inputFiles = context.GetInputFiles();
        var processed = new HashSet<string>(context.GetProcessedFileNames());
        var newFiles = new List<CodeFile>();

        var fileIndex = inputFiles.Count;
        foreach (var content in llmRequest.Contents)
        {
            if (content.Parts == null) continue;
            foreach (var part in content.Parts)
            {
                if (part.InlineData == null) continue;
                if (!SupportedInlineDataTypes.Contains(part.InlineData.MimeType)) continue;

                var name = part.FileData?.Name;
                if (string.IsNullOrWhiteSpace(name))
                    name = $"input_{fileIndex++}.csv";

                if (processed.Contains(name))
                    continue;

                var mimeType = part.InlineData.MimeType;
                newFiles.Add(new CodeFile
                {
                    Name = name,
                    MimeType = mimeType,
                    Content = part.InlineData.Data,
                });

                part.Text = $"[file:{name}]";
                part.InlineData = null;
                part.FileData = new FileData { Name = name, MimeType = mimeType };
            }
        }

        if (newFiles.Count == 0)
            yield break;

        inputFiles.AddRange(newFiles);
        context.SetInputFiles(inputFiles);
        foreach (var file in newFiles)
            context.AddProcessedFileName(file.Name);

        if (executor.Stateful && string.IsNullOrWhiteSpace(context.GetExecutionId()))
            context.SetExecutionId(Guid.NewGuid().ToString());
    }
}
