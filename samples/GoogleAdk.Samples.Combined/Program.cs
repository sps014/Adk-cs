// Copyright 2025 Google LLC
// SPDX-License-Identifier: Apache-2.0

// ============================================================================
// Combined Patterns Sample — Real-World Product Launch Advisor
// ============================================================================
//
// Combines ALL agent patterns in a realistic product-launch workflow:
//
//   1. ParallelAgent — gathers market data, competitor info, and trends concurrently
//   2. SequentialAgent — analyst reviews parallel results, then strategist plans
//   3. LoopAgent — reviewer iterates on the strategy until quality is high
//   4. Sub-Agent transfer — root coordinator routes based on user intent
//   5. AgentTool — wraps pipelines as callable tools
//   6. Google Search — grounded research with live web data
//   7. OutputKey — passes structured data between agents via session state
//   8. GlobalInstruction — consistent brand voice across all agents
//
// Environment variables:
//   GOOGLE_GENAI_USE_VERTEXAI=True
//   GOOGLE_CLOUD_PROJECT=<your-project-id>
//   GOOGLE_CLOUD_LOCATION=us-central1
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Agents.Processors;
using GoogleAdk.Core.Runner;
using GoogleAdk.Core.Tools;
using GoogleAdk.Models.Gemini;

var model = GeminiModelFactory.Create("gemini-2.5-flash");

// ── Phase 1: Parallel Research ─────────────────────────────────────────────

var marketResearcher = new LlmAgent(new LlmAgentConfig
{
    Name = "market_researcher",
    Description = "Researches market size, target demographics, and demand signals.",
    Model = model,
    Instruction = """
        Analyze the market for the given product. Cover: market size estimate,
        target demographics, key demand signals. Keep it under 100 words.
        """,
    OutputKey = "market_data",
    DisallowTransferToParent = true,
    DisallowTransferToPeers = true,
    Tools = new List<IBaseTool> { GoogleAdk.Samples.Combined.CombinedTools.GetMarketDataTool },
});

var competitorAnalyst = new LlmAgent(new LlmAgentConfig
{
    Name = "competitor_analyst",
    Description = "Analyzes top competitors, their strengths, and gaps.",
    Model = model,
    Instruction = """
        Identify 3 key competitors for the given product. For each: name, strength,
        and one weakness/gap we could exploit. Keep it under 100 words.
        """,
    OutputKey = "competitor_data",
    DisallowTransferToParent = true,
    DisallowTransferToPeers = true,
    Tools = new List<IBaseTool> { GoogleAdk.Samples.Combined.CombinedTools.GetCompetitorsTool },
});

var trendScout = new LlmAgent(new LlmAgentConfig
{
    Name = "trend_scout",
    Description = "Identifies relevant trends and emerging opportunities.",
    Model = model,
    Instruction = """
        Identify 3 relevant trends for the given product category.
        Focus on technology shifts, consumer behavior changes, and regulatory tailwinds.
        Keep it under 100 words.
        """,
    OutputKey = "trend_data",
    DisallowTransferToParent = true,
    DisallowTransferToPeers = true,
});

var parallelResearch = new ParallelAgent(new BaseAgentConfig
{
    Name = "parallel_research",
    Description = "Gathers market, competitor, and trend data concurrently.",
    SubAgents = new List<BaseAgent> { marketResearcher, competitorAnalyst, trendScout },
});

// ── Phase 2: Sequential Analysis + Strategy ────────────────────────────────

var synthesizer = new LlmAgent(new LlmAgentConfig
{
    Name = "synthesizer",
    Description = "Synthesizes parallel research into a unified brief.",
    Model = model,
    Instruction = """
        You have access to research from three agents via session state:
        - Market data: {market_data?}
        - Competitor data: {competitor_data?}
        - Trend data: {trend_data?}

        Synthesize into a single 150-word unified research brief with key takeaways.
        """,
    OutputKey = "research_brief",
    DisallowTransferToParent = true,
    DisallowTransferToPeers = true,
});

var strategist = new LlmAgent(new LlmAgentConfig
{
    Name = "strategist",
    Description = "Creates a go-to-market strategy based on research.",
    Model = model,
    Instruction = """
        Based on the research brief: {research_brief?}
        
        Create a go-to-market strategy with:
        1. Positioning statement (1 sentence)
        2. Target launch channels (3 items)
        3. Key differentiators (3 items)
        4. Risk mitigation (2 items)
        
        Keep it under 200 words, formatted as a brief.
        """,
    DisallowTransferToParent = true,
    DisallowTransferToPeers = true,
});

var analysisSequence = new SequentialAgent(new BaseAgentConfig
{
    Name = "analysis_pipeline",
    Description = "Synthesizes research then creates strategy.",
    SubAgents = new List<BaseAgent> { synthesizer, strategist },
});

// ── Phase 3: Full Pipeline (parallel research → sequential analysis) ───────

var fullPipeline = new SequentialAgent(new BaseAgentConfig
{
    Name = "launch_pipeline",
    Description = "Full product launch analysis: parallel research → synthesis → strategy.",
    SubAgents = new List<BaseAgent> { parallelResearch, analysisSequence },
});

// ── Phase 4: Quality Review Loop ───────────────────────────────────────────

var reviewer = new LlmAgent(new LlmAgentConfig
{
    Name = "reviewer",
    Description = "Reviews the strategy quality.",
    Model = model,
    Instruction = """
        Review the strategy from the previous agent. Score 1-10 on:
        actionability, specificity, and completeness.
        If average >= 8, call the escalate tool. Otherwise give 2 improvement suggestions.
        """,
    Tools = new List<IBaseTool> { GoogleAdk.Samples.Combined.CombinedTools.EscalateTool },
});

// ── Quick Q&A agent (for simple follow-ups) ────────────────────────────────

var qaAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "quick_qa",
    Description = "Answers quick follow-up questions about product strategy and marketing.",
    Model = model,
    Instruction = """
        Answer quick questions about product launches, pricing strategies, marketing
        channels, or go-to-market planning. Be concise and actionable. If the user
        needs a full analysis, tell them to ask the coordinator for a "full launch analysis".
        """,
    Tools = new List<IBaseTool> { GoogleSearchTool.Instance },
});

// ── Root Coordinator ───────────────────────────────────────────────────────

var coordinator = new LlmAgent(new LlmAgentConfig
{
    Name = "coordinator",
    Description = "Routes requests to the appropriate workflow.",
    Model = model,
    GlobalInstruction = """
        BRAND VOICE: You work for "LaunchPad AI", a premium product strategy consultancy.
        Always be professional, data-driven, and actionable. Use clear headings and structure.
        """,
    Instruction = """
        You are the coordinator for LaunchPad AI. You have two workflows:
        
        1. For comprehensive product launch analysis, use the 'launch_pipeline' tool.
           This runs parallel market research, then synthesizes and creates strategy.
        
        2. For quick follow-up questions, transfer to the 'quick_qa' agent.
        
        Present results clearly to the user with proper formatting.
        """,
    Tools = new List<IBaseTool> { new AgentTool(fullPipeline) },
    SubAgents = new List<BaseAgent> { qaAgent },
});

var runner = new InMemoryRunner("combined-patterns-sample", coordinator);

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ADK C# — Combined Patterns: Product Launch Advisor     ║");
Console.WriteLine("║                                                          ║");
Console.WriteLine("║  Patterns used: Parallel + Sequential + Loop + Transfer ║");
Console.WriteLine("║  + AgentTool + GoogleSearch + OutputKey + GlobalInstr    ║");
Console.WriteLine("║                                                          ║");
Console.WriteLine("║  Try: 'Analyze launching a smart water bottle'          ║");
Console.WriteLine("║  Or:  'What are good pricing strategies for SaaS?'      ║");
Console.WriteLine("║  Type 'quit' to exit.                                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        break;

    var userMessage = new Content
    {
        Role = "user",
        Parts = new List<Part> { new() { Text = input } }
    };

    Console.WriteLine();
    var sw = System.Diagnostics.Stopwatch.StartNew();

    await foreach (var evt in runner.RunEphemeralAsync("user-1", userMessage))
    {
        var text = evt.Content?.Parts?.FirstOrDefault()?.Text;

        // Show function calls
        var calls = evt.GetFunctionCalls();
        foreach (var call in calls)
        {
            if (call.Name == "transfer_to_agent")
                Console.WriteLine($"  → Routing to: {call.Args?.GetValueOrDefault("agentName")}");
            else
                Console.WriteLine($"  ⚡ [{evt.Author}] tool: {call.Name}");
        }

        // Show agent responses (non-partial only)
        if (text != null && evt.Partial != true)
        {
            // Show sub-agent outputs as compact summaries, coordinator output in full
            if (evt.Author == "coordinator")
            {
                Console.WriteLine($"[{evt.Author}]:");
                Console.WriteLine(text);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"  [{evt.Author}] ✓ completed");
            }
        }
    }

    sw.Stop();
    Console.WriteLine($"  ⏱ {sw.Elapsed.TotalSeconds:F1}s total");
    Console.WriteLine(new string('─', 60));
}
