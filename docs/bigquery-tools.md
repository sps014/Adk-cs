# BigQuery Tools

The `GoogleAdk.Core.Tools` namespace includes two tools for working with Google Cloud BigQuery:
- **`BigQueryMetadataTool`**: Fetches datasets, tables, and schemas so the LLM can "explore" the database structure.
- **`BigQueryQueryTool`**: Executes standard SQL queries.

## Prerequisites

The host application must be authenticated with Google Cloud (via ADC or `GOOGLE_APPLICATION_CREDENTIALS`) and have permissions to execute queries in the target project.

## Usage

Attach the tools to your agent and provide clear instructions. The LLM will automatically determine when to explore metadata and when to execute a query.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var analystAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "data_analyst",
    Model = "gemini-2.5-flash",
    Instruction = "You are a data analyst. Explore the dataset structure, then query to answer questions.",
    Tools = [
        new BigQueryMetadataTool(), 
        new BigQueryQueryTool()
    ]
});
```

### Execution Flow

1. User asks: *"Top 5 countries in `my-project.sales.2024`?"*
2. LLM calls `bigquery_metadata` to find columns (e.g., `country_code`, `total_amount`).
3. LLM calls `bigquery_query` with generated SQL: `SELECT country_code, SUM(total_amount)...`
4. The tool executes the query via the BigQuery client and returns JSON.
5. The LLM formats the final answer.