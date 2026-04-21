# Bigtable Query Tool

The `BigtableQueryTool` enables your LLM agents to read rows and row ranges directly from Google Cloud Bigtable.

## Overview

Bigtable is a highly scalable NoSQL database. Since it doesn't use standard SQL, querying it typically involves specific row keys or prefixes. By providing this tool, the LLM can look up wide-column data based on user requests, making it perfect for retrieving profiles, time-series data, or user preferences.

## Prerequisites

Your application must be authenticated with Google Cloud and possess the necessary IAM roles (like `roles/bigtable.reader`) to access the target instance and table.

## Usage

Instantiate the tool and add it to your agent's configuration.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "bigtable_agent",
    Model = "gemini-2.5-flash",
    Instruction = @"
        You are a data explorer. Use the bigtable_query tool to find user profiles. 
        Always use projectId 'my-project', instanceId 'primary-instance', and tableId 'user-profiles'.
        If the user asks for a specific user ID, pass it as the 'rowKey'.
        If the user asks for a region of users, pass the region as the 'rowPrefix'.",
    Tools = [ new BigtableQueryTool() ]
});
```

### Parameters passed by the LLM

When the LLM decides to use this tool, it must pass the following JSON arguments:
- `projectId` (string, required)
- `instanceId` (string, required)
- `tableId` (string, required)
- `rowKey` (string, optional) - Fetches a single exact row.
- `rowPrefix` (string, optional) - Fetches all rows starting with this prefix.
- `limit` (integer, optional) - Limits the number of rows returned.

### Output
The tool parses the Bigtable cells and returns them to the LLM as structured JSON, allowing the LLM to read the column families and values and format a friendly response for the user.