# BigQuery Tools

The `GoogleAdk.Core.Tools` namespace includes two powerful built-in tools for working with Google Cloud BigQuery: `BigQueryQueryTool` and `BigQueryMetadataTool`. These tools allow the LLM to inspect database schemas and execute SQL queries directly against your data warehouse.

## Overview

- **`BigQueryMetadataTool`**: Used to fetch datasets, table lists, schemas, and descriptions. This allows the LLM to "explore" the database and understand the structure before writing queries.
- **`BigQueryQueryTool`**: Used to execute standard SQL queries. The LLM automatically formulates the query based on the metadata it explored or prior knowledge.

## Prerequisites

To use these tools, the application running the ADK must be authenticated with Google Cloud (typically via `GOOGLE_APPLICATION_CREDENTIALS` or workload identity) and have permissions to read metadata and execute queries in the target BigQuery project.

## Usage

You can attach these tools directly to your agent. Because the LLM will decide when to use them, giving the agent clear instructions on its role (e.g., "Data Analyst") is highly recommended.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

// Instantiate the BigQuery tools
var bqQueryTool = new BigQueryQueryTool();
var bqMetadataTool = new BigQueryMetadataTool();

var analystAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "data_analyst",
    Model = "gemini-2.5-flash",
    Instruction = "You are a data analyst. Use the metadata tool to explore the dataset structure, and then use the query tool to answer the user's questions.",
    Tools = [bqQueryTool, bqMetadataTool]
});
```

### How the LLM interacts

When a user asks: *"What are the top 5 countries by sales in the `my-project.sales.2024` table?"*

1. **Step 1:** The LLM might first call `bigquery_metadata` passing `projectId="my-project"`, `datasetId="sales"`, and `tableId="2024"` to understand the exact column names (e.g., `country_code`, `total_amount`).
2. **Step 2:** Armed with the schema, the LLM will call `bigquery_query` passing the `projectId` and the formulated `query` (e.g., `SELECT country_code, SUM(total_amount) FROM \`my-project.sales.2024\` GROUP BY country_code ORDER BY 2 DESC LIMIT 5`).
3. **Step 3:** The tool executes the query via the official Google Cloud BigQuery client library and returns the rows as JSON.
4. **Step 4:** The LLM reads the JSON and formats a natural language response for the user.

## Example

For a complete working example of attaching built-in tools, see the [Tools Sample](../samples/GoogleAdk.Samples.Tools).