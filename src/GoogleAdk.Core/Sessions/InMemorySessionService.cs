using System.Collections.Concurrent;
using System.Text.Json;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Sessions;

namespace GoogleAdk.Core.Sessions;

/// <summary>
/// An in-memory implementation of the session service.
/// </summary>
public class InMemorySessionService : BaseSessionService
{
    // appName -> userId -> sessionId -> SessionEntry
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, SessionEntry>>> _sessions = new();
    // appName -> userId -> key -> value
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, object?>>> _userState = new();
    // appName -> key -> value
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object?>> _appState = new();

    // Per-session lock so concurrent sub-agents don't corrupt a single session's event list
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _sessionLocks = new();

    private sealed class SessionEntry
    {
        public Session Session { get; }
        public SessionEntry(Session session) => Session = session;
    }

    private SemaphoreSlim GetSessionLock(string sessionId) =>
        _sessionLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));

    public override Task<Session> CreateSessionAsync(CreateSessionRequest request)
    {
        var session = Session.Create(
            id: request.SessionId ?? Guid.NewGuid().ToString(),
            appName: request.AppName,
            userId: request.UserId,
            state: request.State != null ? new Dictionary<string, object?>(request.State) : null
        );
        session.LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var byUser = _sessions.GetOrAdd(request.AppName, _ => new());
        var bySession = byUser.GetOrAdd(request.UserId, _ => new());
        bySession[session.Id] = new SessionEntry(session);

        var copied = DeepClone(session);
        copied.State = MergeStates(
            _appState.GetValueOrDefault(request.AppName)?.ToDictionary(),
            _userState.GetValueOrDefault(request.AppName)?.GetValueOrDefault(request.UserId)?.ToDictionary(),
            copied.State);

        return Task.FromResult(copied);
    }

    public override Task<Session?> GetSessionAsync(GetSessionRequest request)
    {
        if (!_sessions.TryGetValue(request.AppName, out var byUser) ||
            !byUser.TryGetValue(request.UserId, out var bySession) ||
            !bySession.TryGetValue(request.SessionId, out var entry))
        {
            return Task.FromResult<Session?>(null);
        }

        var copied = DeepClone(entry.Session);

        if (request.Config != null)
        {
            if (request.Config.NumRecentEvents.HasValue)
            {
                var n = request.Config.NumRecentEvents.Value;
                copied.Events = copied.Events.Count > n
                    ? copied.Events.GetRange(copied.Events.Count - n, n)
                    : copied.Events;
            }

            if (request.Config.AfterTimestamp.HasValue)
            {
                var ts = request.Config.AfterTimestamp.Value;
                int i = copied.Events.Count - 1;
                while (i >= 0 && copied.Events[i].Timestamp >= ts) i--;
                if (i >= 0)
                    copied.Events = copied.Events.GetRange(i + 1, copied.Events.Count - i - 1);
            }
        }

        copied.State = MergeStates(
            _appState.GetValueOrDefault(request.AppName)?.ToDictionary(),
            _userState.GetValueOrDefault(request.AppName)?.GetValueOrDefault(request.UserId)?.ToDictionary(),
            copied.State);

        return Task.FromResult<Session?>(copied);
    }

    public override Task<ListSessionsResponse> ListSessionsAsync(ListSessionsRequest request)
    {
        if (!_sessions.TryGetValue(request.AppName, out var byUser) ||
            !byUser.TryGetValue(request.UserId, out var bySession))
        {
            return Task.FromResult(new ListSessionsResponse());
        }

        var sessions = bySession.Values.Select(e =>
        {
            var s = Session.Create(e.Session.Id, e.Session.AppName, e.Session.UserId);
            s.LastUpdateTime = e.Session.LastUpdateTime;
            return s;
        }).ToList();

        return Task.FromResult(new ListSessionsResponse { Sessions = sessions });
    }

    public override Task DeleteSessionAsync(DeleteSessionRequest request)
    {
        if (_sessions.TryGetValue(request.AppName, out var byUser) &&
            byUser.TryGetValue(request.UserId, out var bySession))
        {
            bySession.TryRemove(request.SessionId, out _);
            _sessionLocks.TryRemove(request.SessionId, out _);
        }
        return Task.CompletedTask;
    }

    public override async Task<Event> AppendEventAsync(AppendEventRequest request)
    {
        // base.AppendEventAsync assigns Id/Timestamp — no shared state, safe without lock
        var evt = await base.AppendEventAsync(request);

        var appName = request.Session.AppName;
        var userId = request.Session.UserId;
        var sessionId = request.Session.Id;

        if (!_sessions.TryGetValue(appName, out var byUser) ||
            !byUser.TryGetValue(userId, out var bySession) ||
            !bySession.TryGetValue(sessionId, out var entry))
        {
            return evt;
        }

        // Per-session lock: only one writer per session at a time (needed for List<Event>)
        var sessionLock = GetSessionLock(sessionId);
        await sessionLock.WaitAsync();
        try
        {
            var storageSession = entry.Session;

            // Scope app/user state prefixes into their own ConcurrentDictionaries
            if (evt.Actions?.StateDelta != null)
            {
                foreach (var (key, value) in evt.Actions.StateDelta)
                {
                    if (key.StartsWith(State.AppPrefix))
                        _appState.GetOrAdd(appName, _ => new())[key[State.AppPrefix.Length..]] = value;

                    if (key.StartsWith(State.UserPrefix))
                        _userState.GetOrAdd(appName, _ => new())
                                  .GetOrAdd(userId, _ => new())[key[State.UserPrefix.Length..]] = value;
                }
            }

            await base.AppendEventAsync(new AppendEventRequest { Session = storageSession, Event = evt });
            storageSession.LastUpdateTime = evt.Timestamp;
        }
        finally { sessionLock.Release(); }

        return evt;
    }

    private static Session DeepClone(Session session)
    {
        var json = JsonSerializer.Serialize(session);
        return JsonSerializer.Deserialize<Session>(json)!;
    }
}
