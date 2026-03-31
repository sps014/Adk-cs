// Copyright 2025 Google LLC
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Dev.Server;
using Microsoft.AspNetCore.Mvc;

namespace GoogleAdk.Dev.Server;

/// <summary>
/// Registers all ADK API endpoints on the WebApplication, matching the JS ADK API surface.
/// </summary>
public static class AdkApiEndpoints
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static WebApplication MapAdkApi(this WebApplication app)
    {
        // ── List Apps ──────────────────────────────────────────────────────
        app.MapGet("/list-apps", (AgentLoader loader) =>
        {
            return Results.Json(loader.ListAgents(), s_jsonOptions);
        });

        // ── Session CRUD ───────────────────────────────────────────────────
        app.MapGet("/apps/{appName}/users/{userId}/sessions",
            async (string appName, string userId, RunnerManager mgr) =>
            {
                var result = await mgr.SessionService.ListSessionsAsync(new ListSessionsRequest
                {
                    AppName = appName,
                    UserId = userId,
                });
                return Results.Json(result.Sessions, s_jsonOptions);
            });

        app.MapGet("/apps/{appName}/users/{userId}/sessions/{sessionId}",
            async (string appName, string userId, string sessionId, RunnerManager mgr) =>
            {
                var session = await mgr.SessionService.GetSessionAsync(new GetSessionRequest
                {
                    AppName = appName,
                    UserId = userId,
                    SessionId = sessionId,
                });
                return session is null ? Results.NotFound() : Results.Json(session, s_jsonOptions);
            });

        app.MapPost("/apps/{appName}/users/{userId}/sessions",
            async (string appName, string userId, [FromBody] CreateSessionBody? body, RunnerManager mgr) =>
            {
                var session = await mgr.SessionService.CreateSessionAsync(new CreateSessionRequest
                {
                    AppName = appName,
                    UserId = userId,
                    State = body?.State,
                });
                return Results.Json(session, s_jsonOptions, statusCode: 201);
            });

        app.MapPost("/apps/{appName}/users/{userId}/sessions/{sessionId}",
            async (string appName, string userId, string sessionId,
                   [FromBody] CreateSessionBody? body, RunnerManager mgr) =>
            {
                var session = await mgr.SessionService.CreateSessionAsync(new CreateSessionRequest
                {
                    AppName = appName,
                    UserId = userId,
                    SessionId = sessionId,
                    State = body?.State,
                });
                return Results.Json(session, s_jsonOptions, statusCode: 201);
            });

        app.MapDelete("/apps/{appName}/users/{userId}/sessions/{sessionId}",
            async (string appName, string userId, string sessionId, RunnerManager mgr) =>
            {
                await mgr.SessionService.DeleteSessionAsync(new DeleteSessionRequest
                {
                    AppName = appName,
                    UserId = userId,
                    SessionId = sessionId,
                });
                return Results.Ok();
            });

        // ── Run (synchronous) ──────────────────────────────────────────────
        app.MapPost("/run", async (HttpContext http, [FromBody] RunAgentRequest req, RunnerManager mgr) =>
        {
            var runner = mgr.GetOrCreate(req.AppName);
            var message = req.ResolveMessage();
            var events = new List<Event>();

            await foreach (var evt in runner.RunAsync(
                req.UserId, req.SessionId, message,
                stateDelta: req.StateDelta,
                cancellationToken: http.RequestAborted))
            {
                events.Add(evt);
            }

            return Results.Json(events, s_jsonOptions);
        });

        // ── Run SSE (streaming) ────────────────────────────────────────────
        app.MapPost("/run_sse", async (HttpContext http, [FromBody] RunAgentRequest req, RunnerManager mgr) =>
        {
            var runner = mgr.GetOrCreate(req.AppName);
            var message = req.ResolveMessage();

            http.Response.ContentType = "text/event-stream";
            http.Response.Headers.CacheControl = "no-cache";
            http.Response.Headers.Connection = "keep-alive";
            http.Response.Headers["X-Accel-Buffering"] = "no";

            var runConfig = new GoogleAdk.Core.Agents.RunConfig
            {
                StreamingMode = GoogleAdk.Core.Agents.StreamingMode.Sse,
            };

            await foreach (var evt in runner.RunAsync(
                req.UserId, req.SessionId, message,
                stateDelta: req.StateDelta,
                runConfig: runConfig,
                cancellationToken: http.RequestAborted))
            {
                var json = JsonSerializer.Serialize(evt, s_jsonOptions);
                await http.Response.WriteAsync($"data: {json}\n\n", http.RequestAborted);
                await http.Response.Body.FlushAsync(http.RequestAborted);
            }
        });

        // ── Agent Graph ────────────────────────────────────────────────────
        app.MapGet("/apps/{appName}/agent-graph", (string appName, AgentLoader loader) =>
        {
            var agent = loader.GetAgent(appName);
            var graph = AgentGraphBuilder.BuildGraph(agent);
            return Results.Json(graph, s_jsonOptions);
        });

        return app;
    }
}
