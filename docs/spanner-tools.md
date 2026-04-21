# Cloud Spanner Tools

The ADK provides built-in tools for interacting with Google Cloud Spanner: `SpannerQueryTool` and `SpannerSearchTool`. These tools allow agents to retrieve relational data as well as perform vector similarity searches over embeddings.

## Overview

- **`SpannerQueryTool`**: Executes standard SQL queries against a Spanner database.
- **`SpannerSearchTool`**: Specifically built for Retrieval-Augmented Generation (RAG) flows. It performs vector similarity searches using a text query that gets dynamically converted to embeddings using Google Cloud's embedding models.

## Usage

To use these tools, your environment must be authenticated with Google Cloud, and you need the appropriate IAM roles for Spanner read operations.

### SpannerQueryTool

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "spanner_agent",
    Model = "gemini-2.5-flash",
    Instruction = @"
        You are a database admin. Query Spanner to retrieve user data. 
        Always use projectId 'my-project', instanceId 'prod-instance', and databaseId 'main-db'.",
    Tools = [ new SpannerQueryTool() ]
});
```
When invoked, the LLM provides `projectId`, `instanceId`, `databaseId`, and the SQL `query`.

### SpannerSearchTool

The search tool is powerful because it abstracts the embedding generation. When the LLM passes a search phrase (e.g., "how to reset password"), the tool generates embeddings for that phrase and executes a KNN vector search against a specified column.

```csharp
var searchTool = new SpannerSearchTool();

var supportAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "support_agent",
    Model = "gemini-2.5-flash",
    Instruction = @"
        You are a support bot. Use SpannerSearch to find related articles. 
        Use projectId 'my-project', instanceId 'docs-instance', databaseId 'articles-db'.
        The table is 'HelpDocs', embedding column is 'ContentEmbeddings'.
        Use 'text-embedding-004' for the modelName.",
    Tools = [ searchTool ]
});
```

*Note:* When defining agents that rely on complex tools like Spanner, always provide explicit instructions containing the required connection parameters (project, instance, database) so the LLM knows what to pass into the tool calls.