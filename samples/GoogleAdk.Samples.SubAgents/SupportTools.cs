// Copyright 2025 Google LLC
// SPDX-License-Identifier: Apache-2.0

using GoogleAdk.Core.Abstractions.Tools;

namespace GoogleAdk.Samples.SubAgents;

/// <summary>
/// Tools for the sub-agent transfer sample.
/// </summary>
public static partial class SupportTools
{
    /// <summary>Checks the operational status of a service or system.</summary>
    /// <param name="service">The service name to check (e.g., 'api', 'database', 'auth')</param>
    [FunctionTool]
    public static object? CheckStatus(string service)
    {
        var statuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["api"] = "operational",
            ["database"] = "operational",
            ["auth"] = "degraded — elevated latency detected",
            ["cdn"] = "operational",
            ["email"] = "outage — investigating",
        };

        var status = statuses.GetValueOrDefault(service, "unknown service");
        return new { service, status, timestamp = DateTime.UtcNow.ToString("u") };
    }
}
