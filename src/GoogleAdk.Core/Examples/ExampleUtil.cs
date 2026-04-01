using System.Text;
using System.Text.Json;
using GoogleAdk.Core.Abstractions.Models;

namespace GoogleAdk.Core.Examples;

public static class ExampleUtil
{
    public static string ConvertExamplesToText(IEnumerable<Example> examples, string? model = null)
    {
        var exampleList = examples.ToList();
        if (exampleList.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("<EXAMPLES>");
        sb.AppendLine("Begin few-shot");

        for (var i = 0; i < exampleList.Count; i++)
        {
            var example = exampleList[i];
            sb.AppendLine($"EXAMPLE {i + 1}:");
            sb.AppendLine("Begin example");

            AppendContent(sb, "user", example.Input, model);
            foreach (var output in example.Output)
                AppendContent(sb, output.Role ?? "model", output, model);

            sb.AppendLine("End example");
        }

        sb.AppendLine("End few-shot");
        sb.AppendLine("</EXAMPLES>");
        return sb.ToString();
    }

    public static string BuildExampleSystemInstruction(
        IEnumerable<Example> examples,
        string? model = null)
    {
        return ConvertExamplesToText(examples, model);
    }

    public static string BuildExampleSystemInstruction(
        BaseExampleProvider provider,
        string query,
        string? model = null)
    {
        var examples = provider.GetExamples(query).ToList();
        return ConvertExamplesToText(examples, model);
    }

    private static void AppendContent(StringBuilder sb, string role, Content content, string? model)
    {
        sb.AppendLine($"[{role}]");
        if (content.Parts == null) return;

        foreach (var part in content.Parts)
        {
            if (!string.IsNullOrWhiteSpace(part.Text))
            {
                sb.AppendLine(part.Text);
                continue;
            }

            if (part.FunctionCall != null)
            {
                AppendFunctionCall(sb, part.FunctionCall, model);
                continue;
            }

            if (part.FunctionResponse != null)
            {
                AppendFunctionResponse(sb, part.FunctionResponse, model);
            }
        }
    }

    private static void AppendFunctionCall(StringBuilder sb, FunctionCall call, string? model)
    {
        var gemini2 = IsGemini2OrAbove(model);
        var argsText = SerializeArgs(call.Args);

        if (gemini2)
        {
            sb.AppendLine("```");
            sb.AppendLine($"{call.Name}({argsText})");
            sb.AppendLine("```");
        }
        else
        {
            sb.AppendLine("```tool_code");
            sb.AppendLine($"{call.Name}({argsText})");
            sb.AppendLine("```");
        }
    }

    private static void AppendFunctionResponse(StringBuilder sb, FunctionResponse response, string? model)
    {
        var gemini2 = IsGemini2OrAbove(model);
        var responseText = JsonSerializer.Serialize(response.Response ?? new Dictionary<string, object?>());

        if (gemini2)
        {
            sb.AppendLine("```");
            sb.AppendLine(responseText);
            sb.AppendLine("```");
        }
        else
        {
            sb.AppendLine("```tool_outputs");
            sb.AppendLine(responseText);
            sb.AppendLine("```");
        }
    }

    private static string SerializeArgs(Dictionary<string, object?>? args)
    {
        if (args == null || args.Count == 0) return string.Empty;
        var parts = new List<string>();
        foreach (var (key, value) in args)
        {
            if (value is string s)
                parts.Add($"{key}='{s}'");
            else
                parts.Add($"{key}={JsonSerializer.Serialize(value)}");
        }
        return string.Join(", ", parts);
    }

    private static bool IsGemini2OrAbove(string? model)
    {
        if (string.IsNullOrWhiteSpace(model)) return true;
        var lower = model.ToLowerInvariant();
        return lower.Contains("gemini-2") || lower.Contains("gemini-3");
    }
}
