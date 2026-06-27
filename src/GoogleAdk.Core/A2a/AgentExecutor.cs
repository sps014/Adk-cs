using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using RunnerType = GoogleAdk.Core.Runner.Runner;
using Task = System.Threading.Tasks.Task;

namespace GoogleAdk.Core.A2a;

public sealed class AgentExecutorConfig
{
    public required RunnerOrRunnerConfig Runner { get; set; }
    public RunConfig? RunConfig { get; set; }

    /// <summary>
    /// Optional interceptor invoked once before the agent starts running, after the
    /// session is resolved. Mirrors adk-python's executor <c>before_agent</c> hook.
    /// </summary>
    public Func<MessageSendParams, CancellationToken, Task>? BeforeAgentCallback { get; set; }

    /// <summary>
    /// Optional interceptor invoked for each A2A event emitted from the agent run,
    /// allowing inspection/augmentation. Mirrors adk-python's <c>after_event</c> hook.
    /// </summary>
    public Func<IA2aEvent, CancellationToken, Task>? AfterEventCallback { get; set; }

    /// <summary>
    /// Optional interceptor invoked once after the agent run completes, before the
    /// terminal status update is emitted. Mirrors adk-python's <c>after_agent</c> hook.
    /// </summary>
    public Func<CancellationToken, Task>? AfterAgentCallback { get; set; }
}

public delegate Task<RunnerOrRunnerConfig> RunnerFactory();

public sealed class RunnerOrRunnerConfig
{
    public RunnerType? Runner { get; init; }
    public RunnerConfig? RunnerConfig { get; init; }
    public RunnerFactory? Factory { get; init; }
}

public sealed class A2aAgentExecutor
{
    private readonly AgentExecutorConfig _config;
    private readonly Dictionary<string, string> _agentPartialArtifactIdsMap = new();

    public A2aAgentExecutor(AgentExecutorConfig config)
    {
        _config = config;
    }

    public async IAsyncEnumerable<IA2aEvent> ExecuteAsync(
        MessageSendParams request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request.Message == null)
            throw new InvalidOperationException("message not provided");

        var userId = $"A2A_USER_{request.Message.ContextId ?? Guid.NewGuid().ToString()}";
        var sessionId = request.Message.ContextId ?? Guid.NewGuid().ToString();
        var taskId = request.Message.TaskId ?? Guid.NewGuid().ToString();

        var userContent = PartConverterUtils.ToContent(request.Message);
        var runner = await GetRunnerAsync(_config.Runner);
        var session = await GetOrCreateSessionAsync(
            userId,
            sessionId,
            runner.SessionService,
            runner.AppName);
        var executorContext = ExecutorContextFactory.Create(session, userContent, request, taskId, sessionId);

        if (_config.BeforeAgentCallback != null)
            await _config.BeforeAgentCallback(request, cancellationToken);

        if (request.Message.TaskId == null)
        {
            yield return A2aEventHelpers.CreateTask(taskId, sessionId, request.Message);
        }

        var aggregator = new TaskResultAggregator();
        var workingEvent = A2aEventHelpers.CreateTaskWorkingEvent(taskId, sessionId);
        aggregator.Process(workingEvent);
        yield return workingEvent;

        var adkEvents = new List<Event>();
        var enumerator = runner.RunAsync(
            userId,
            sessionId,
            userContent,
            runConfig: _config.RunConfig,
            cancellationToken: cancellationToken).GetAsyncEnumerator(cancellationToken);

        try
        {
            while (true)
            {
                Event adkEvent;
                Exception? failure = null;
                try
                {
                    if (!await enumerator.MoveNextAsync())
                        break;
                    adkEvent = enumerator.Current;
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    failure = ex;
                    adkEvent = null!;
                }

                if (failure != null)
                {
                    // Surface runtime failures as a terminal failed task event instead
                    // of aborting the HTTP stream (mirrors adk-python's catch-all).
                    yield return A2aEventHelpers.CreateTaskFailedEvent(
                        taskId,
                        sessionId,
                        failure,
                        MetadataConverterUtils.GetA2ASessionMetadata(
                            executorContext.AppName, executorContext.UserId, executorContext.SessionId));
                    yield break;
                }

                adkEvents.Add(adkEvent);
                var a2aEvent = ConvertAdkEventToA2aEvent(adkEvent, executorContext);
                if (a2aEvent == null) continue;
                aggregator.Process(a2aEvent);
                if (_config.AfterEventCallback != null)
                    await _config.AfterEventCallback(a2aEvent, cancellationToken);
                yield return a2aEvent;
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        if (_config.AfterAgentCallback != null)
            await _config.AfterAgentCallback(cancellationToken);

        var finalStatus = EventProcessorUtils.GetFinalTaskStatusUpdate(adkEvents, executorContext);
        aggregator.Process(finalStatus);

        // If an interrupting state (failed / auth-required / input-required) was
        // observed anywhere in the stream, ensure the terminal event reflects it
        // rather than being masked by a later "working"/"completed" update.
        if (aggregator.TaskState == TaskState.Failed && finalStatus.Status.State != TaskState.Failed)
        {
            finalStatus.Status.State = TaskState.Failed;
            finalStatus.Final = true;
        }

        yield return finalStatus;
    }

    private TaskArtifactUpdateEvent? ConvertAdkEventToA2aEvent(Event adkEvent, ExecutorContext context)
    {
        var a2aParts = PartConverterUtils.ToA2aParts(adkEvent.Content?.Parts, adkEvent.LongRunningToolIds);
        if (a2aParts.Count == 0) return null;

        var artifactId = _agentPartialArtifactIdsMap.TryGetValue(adkEvent.Author ?? string.Empty, out var existing)
            ? existing
            : Guid.NewGuid().ToString();

        var a2aEvent = A2aEventHelpers.CreateTaskArtifactUpdateEvent(
            context.TaskId,
            context.ContextId,
            artifactId,
            a2aParts,
            MetadataConverterUtils.GetA2AEventMetadata(adkEvent, context.AppName, context.UserId, context.SessionId),
            append: adkEvent.Partial,
            lastChunk: !adkEvent.Partial);

        if (adkEvent.Partial == true)
            _agentPartialArtifactIdsMap[adkEvent.Author ?? string.Empty] = artifactId;
        else
            _agentPartialArtifactIdsMap.Remove(adkEvent.Author ?? string.Empty);

        return a2aEvent;
    }

    private static async Task<Session> GetOrCreateSessionAsync(
        string userId,
        string sessionId,
        BaseSessionService sessionService,
        string appName)
    {
        var session = await sessionService.GetSessionAsync(new GetSessionRequest
        {
            AppName = appName,
            UserId = userId,
            SessionId = sessionId,
        });
        if (session != null) return session;

        return await sessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AppName = appName,
            UserId = userId,
            SessionId = sessionId,
        });
    }

    private static async Task<RunnerType> GetRunnerAsync(RunnerOrRunnerConfig runnerOrConfig)
    {
        if (runnerOrConfig.Factory != null)
        {
            var result = await runnerOrConfig.Factory();
            return await GetRunnerAsync(result);
        }

        if (runnerOrConfig.Runner != null)
            return runnerOrConfig.Runner;

        if (runnerOrConfig.RunnerConfig != null)
            return new RunnerType(runnerOrConfig.RunnerConfig);

        throw new InvalidOperationException("Invalid runner configuration.");
    }
}

