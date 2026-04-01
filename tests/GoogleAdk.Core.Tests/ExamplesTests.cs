using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Examples;

namespace GoogleAdk.Core.Tests;

public class ExamplesTests
{
    [Fact]
    public void ExampleUtil_FormatsExamplesAsText()
    {
        var example = new Example
        {
            Input = new Content
            {
                Role = "user",
                Parts = new List<Part> { new Part { Text = "Hello" } }
            },
            Output =
            [
                new Content
                {
                    Role = "model",
                    Parts =
                    [
                        new Part { Text = "Hi!" },
                        new Part
                        {
                            FunctionCall = new FunctionCall
                            {
                                Name = "tool",
                                Args = new Dictionary<string, object?> { ["q"] = "test" }
                            }
                        }
                    ]
                }
            ]
        };

        var text = ExampleUtil.ConvertExamplesToText(new[] { example }, "gemini-2.5-flash");

        Assert.Contains("<EXAMPLES>", text);
        Assert.Contains("EXAMPLE 1", text);
        Assert.Contains("[user]", text);
        Assert.Contains("[model]", text);
        Assert.Contains("tool(q='test')", text);
    }
}
