// Copyright 2025 Google LLC
// SPDX-License-Identifier: Apache-2.0

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

namespace GoogleAdk.Samples.Combined;

/// <summary>
/// Tools for the combined patterns sample.
/// </summary>
public static partial class CombinedTools
{
    /// <summary>
    /// Escalate tool — breaks the review loop when quality is satisfactory.
    /// </summary>
    public static readonly FunctionTool EscalateTool = new(
        name: "escalate",
        description: "Call this tool when the quality score is >= 8 to end the review loop.",
        execute: (args, ctx) =>
        {
            ctx.EventActions.Escalate = true;
            return Task.FromResult<object?>(new { status = "escalated", message = "Review loop completed." });
        },
        parameters: new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object?>(),
        });

    /// <summary>Gets simulated market data for a product category.</summary>
    /// <param name="category">The product category to look up</param>
    [FunctionTool]
    public static object? GetMarketData(string category)
    {
        return new
        {
            category,
            estimatedMarketSize = "$4.2B globally",
            growthRate = "12% CAGR",
            topSegments = new[] { "millennials", "health-conscious consumers", "enterprise buyers" },
            source = "simulated-market-api"
        };
    }

    /// <summary>Gets simulated competitor information for a product category.</summary>
    /// <param name="category">The product category to analyze</param>
    [FunctionTool]
    public static object? GetCompetitors(string category)
    {
        return new
        {
            category,
            competitors = new[]
            {
                new { name = "AlphaCorp", strength = "Brand recognition", weakness = "High pricing" },
                new { name = "BetaTech", strength = "Technical features", weakness = "Poor UX" },
                new { name = "GammaStart", strength = "Low cost", weakness = "Limited features" },
            },
            source = "simulated-competitor-db"
        };
    }
}
