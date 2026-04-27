# Cloud Spanner Tools

The ADK provides built-in tools for Google Cloud Spanner:
- **`SpannerQueryTool`**: Executes standard SQL queries.
- **`SpannerSearchTool`**: For RAG flows. Performs vector similarity searches using text queries dynamically converted to embeddings.

## Usage

Ensure your environment is authenticated with Google Cloud and has appropriate IAM roles.

### Query Tool

When invoked, the LLM provides `projectId`, `instanceId`, `databaseId`, and the SQL `query`.

```csharp
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "spanner_agent",
    Model = "gemini-2.5-flash",
    Instruction = "Query Spanner. Use projectId 'my-project', instanceId 'prod-instance', and databaseId 'main-db'.",
    Tools = [ new SpannerQueryTool() ]
});
```

### Search Tool

Abstracts embedding generation by converting a search phrase (e.g., "how to reset password") into embeddings for a KNN vector search.

```csharp
var supportAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "support_agent",
    Model = "gemini-2.5-flash",
    Instruction = @"Use SpannerSearch for articles. 
        Use projectId 'my-project', instanceId 'docs', databaseId 'articles'.
        Table is 'HelpDocs', embedding column is 'ContentEmbeddings'.
        Use 'text-embedding-004' for the modelName.",
    Tools = [ new SpannerSearchTool() ]
});
```