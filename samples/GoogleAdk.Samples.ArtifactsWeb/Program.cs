using GoogleAdk.Core.Agents;
using GoogleAdk.Dev;
using GoogleAdk.Core.Abstractions.Artifacts;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Tools;
using System.Text;

namespace GoogleAdk.Samples.ArtifactsWeb;

/// <summary>
/// Tools for the Artifacts Web Sample.
/// </summary>
public static partial class ArtifactsWebTools
{
    /// <summary>
    /// Reads a PDF artifact and generates a TXT artifact containing its text content.
    /// </summary>
    /// <param name="inputName">Name of the PDF file to read (e.g., invoice.pdf)</param>
    /// <param name="outputName">Name of the output TXT file to generate (e.g., invoice.txt)</param>
    /// <param name="context">The agent context injected by the runtime</param>
    [FunctionTool]
    public static async Task<object?> ConvertPdfToTxt(string inputName, string outputName, AgentContext context)
    {
        if (string.IsNullOrWhiteSpace(inputName) || string.IsNullOrWhiteSpace(outputName))
        {
            return "Error: input_name and output_name are required.";
        }
        
        var artifactService = context.InvocationContext.ArtifactService;
        if (artifactService == null)
        {
            return "Error: No artifact service configured in the runner.";
        }

        var appName = context.InvocationContext.Session?.AppName ?? "default";
        var userId = context.InvocationContext.Session?.UserId ?? "default";
        var sessionId = context.InvocationContext.Session?.Id ?? "default";

        var loadReq = new LoadArtifactRequest 
        { 
            AppName = appName,
            UserId = userId,
            SessionId = sessionId,
            Filename = inputName 
        };

        var loadedPart = await artifactService.LoadArtifactAsync(loadReq);
        if (loadedPart == null)
        {
            return $"Error: Could not load artifact {inputName}.";
        }
        
        var pdfLength = loadedPart.InlineData?.Data?.Length ?? loadedPart.Text?.Length ?? 0;
        
        // Simulating PDF reading logic
        var extractedText = $"Extracted text from {inputName} (Estimated Size: {pdfLength} units).";
        
        var saveReq = new SaveArtifactRequest
        {
            AppName = appName,
            UserId = userId,
            SessionId = sessionId,
            Filename = outputName,
            Artifact = new Part { Text = extractedText }
        };

        await artifactService.SaveArtifactAsync(saveReq);
        
        return $"Successfully read {inputName} and generated {outputName}.";
    }
}

/// <summary>
/// Main program class.
/// </summary>
public class Program
{
    /// <summary>
    /// Entry point for the sample.
    /// </summary>
    public static async Task Main(string[] args)
    {
        Console.WriteLine("==> Demo: Artifacts Web Sample\n");

        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "artifact_web_agent",
            ModelName = "gemini-2.5-flash",
            Instruction = "You are an assistant that converts PDF artifacts to TXT artifacts. Use the tool provided. When a user asks you to read a pdf, execute the tool to convert it to a txt, then let the user know what was generated.",
            Tools = [ArtifactsWebTools.ConvertPdfToTxtTool]
        });

        AdkWeb.Root = agent;
        await AdkWeb.RunAsync();
    }
}
