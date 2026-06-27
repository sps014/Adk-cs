using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

public sealed class PubSubMessageTool : BaseTool
{
    public PubSubMessageTool()
        : base("pubsub_publish", "Publishes a message to a Cloud Pub/Sub topic.")
    {
    }

    public override async Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        var projectId = CloudTool.GetString(args, "projectId");
        if (string.IsNullOrEmpty(projectId))
            return CloudTool.MissingArgument("projectId");

        var topicId = CloudTool.GetString(args, "topicId");
        if (string.IsNullOrEmpty(topicId))
            return CloudTool.MissingArgument("topicId");

        var message = CloudTool.GetString(args, "message");
        if (string.IsNullOrEmpty(message))
            return CloudTool.MissingArgument("message");

        try
        {
            var topicName = TopicName.FromProjectTopic(projectId, topicId);
            var publisher = await PublisherClient.CreateAsync(topicName);

            var pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(message)
            };

            // Publish message
            var messageId = await publisher.PublishAsync(pubsubMessage);

            // Shutdown publisher gracefully
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));

            return CloudTool.Success(("messageId", messageId));
        }
        catch (Exception ex)
        {
            return CloudTool.Error(ex);
        }
    }

    public override FunctionDeclaration? GetDeclaration()
    {
        return new FunctionDeclaration
        {
            Name = Name,
            Description = Description,
            Parameters = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object?>
                {
                    ["projectId"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The Google Cloud project ID."
                    },
                    ["topicId"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The Pub/Sub topic ID."
                    },
                    ["message"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["description"] = "The message payload (string)."
                    }
                },
                ["required"] = new[] { "projectId", "topicId", "message" }
            }
        };
    }
}
