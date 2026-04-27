# Bigtable Query Tool

The `BigtableQueryTool` enables your LLM agents to read rows and row ranges directly from Google Cloud Bigtable.

## Usage

Provide the tool to your agent and ensure your application has the `roles/bigtable.reader` IAM role.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "bigtable_agent",
    Model = "gemini-2.5-flash",
    Instruction = "Use the bigtable_query tool to find user profiles. Use projectId 'my-project', instanceId 'primary', and tableId 'users'.",
    Tools = [ new BigtableQueryTool() ]
});
```

### LLM Parameters
When the LLM decides to query Bigtable, it will pass:
- `projectId`, `instanceId`, `tableId` (required)
- `rowKey` (optional) - Fetches a single exact row.
- `rowPrefix` (optional) - Fetches all rows starting with this prefix.
- `limit` (optional) - Limits the number of rows returned.