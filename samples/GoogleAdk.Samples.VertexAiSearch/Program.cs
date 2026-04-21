using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Core.Tools;
using GoogleAdk.ApiServer;
using GoogleAdk.Models.Gemini;

AdkEnv.Load();

Console.WriteLine("==> Demo: Vertex AI Search Tool\n");

// Ensure you have configured Google Cloud credentials (e.g., via gcloud auth application-default login)
// and set your project/location/collection/dataStore below.

var projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
var location = "global";
var datastoreId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_DATASTORE");

if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(datastoreId))
{
    Console.WriteLine("Error: GOOGLE_CLOUD_PROJECT and GOOGLE_CLOUD_DATASTORE environment variables must be set to run this sample.");
    return;
}

var dataStoreId = $"projects/{projectId}/locations/{location}/collections/default_collection/dataStores/{datastoreId}";

// For this demo, we'll configure the agent and tool to show how it's done.
var vertexSearchTool = new VertexAiSearchTool(dataStoreId: dataStoreId);

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "search_agent",
    Model = "gemini-2.5-pro",
    Instruction = "You are a helpful assistant. Always Use the Vertex AI Search tool to find information by reforming user query cleanly.",
    Tools = [vertexSearchTool]
});

Console.WriteLine($"Agent configured with tool: {vertexSearchTool.Name}");
Console.WriteLine($"DataStore ID: {vertexSearchTool.DataStoreId}\n");


if (args.Contains("--console"))
{
    await ConsoleRunner.RunAsync(agent);
    return;
}

await AdkServer.RunAsync(agent);

Console.WriteLine("\nDone!");
