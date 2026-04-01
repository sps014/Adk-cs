using System.Collections.Concurrent;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Core.Abstractions.Sessions;

namespace GoogleAdk.Dev.Server;

/// <summary>
/// Manages Runner instances, caching one per agent app.
/// </summary>
public class RunnerManager
{
    private readonly ConcurrentDictionary<string, Runner> _runners = new(StringComparer.OrdinalIgnoreCase);
    private readonly AgentLoader _agentLoader;
    private readonly BaseSessionService _sessionService;

    public RunnerManager(AgentLoader agentLoader, BaseSessionService sessionService)
    {
        _agentLoader = agentLoader;
        _sessionService = sessionService;
    }

    public Runner GetOrCreate(string appName)
    {
        return _runners.GetOrAdd(appName, name =>
        {
            var agent = _agentLoader.GetAgent(name);
            return new Runner(new RunnerConfig
            {
                AppName = name,
                Agent = agent,
                SessionService = _sessionService,
            });
        });
    }

    public BaseSessionService SessionService => _sessionService;
}
