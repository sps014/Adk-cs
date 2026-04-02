using GoogleAdk.Core.Abstractions.Artifacts;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Plugins;

/// <summary>
/// Saves user-uploaded inline data as artifacts.
/// </summary>
public sealed class SaveFilesAsArtifactsPlugin : BasePlugin
{
    public SaveFilesAsArtifactsPlugin() : base(nameof(SaveFilesAsArtifactsPlugin)) { }

    public override async Task<Content?> OnUserMessageCallbackAsync(
        InvocationContext invocationContext,
        Content userMessage)
    {
        if (invocationContext.ArtifactService == null || userMessage.Parts == null)
            return null;

        var updatedParts = new List<Part>();
        var index = 0;
        foreach (var part in userMessage.Parts)
        {
            if (part.InlineData != null)
            {
                var fileName = part.InlineData.DisplayName ?? $"upload_{index++}";
                var req = new SaveArtifactRequest
                {
                    AppName = invocationContext.AppName,
                    UserId = invocationContext.UserId,
                    SessionId = invocationContext.Session.Id,
                    Filename = fileName,
                    Artifact = part,
                    CustomMetadata = new Dictionary<string, object?>
                    {
                        ["mimeType"] = part.InlineData.MimeType
                    }
                };
                await invocationContext.ArtifactService.SaveArtifactAsync(req);

                updatedParts.Add(new Part
                {
                    FileData = new FileData
                    {
                        Name = fileName,
                        FileUri = $"artifact://{fileName}",
                        MimeType = part.InlineData.MimeType
                    }
                });
            }
            else
            {
                updatedParts.Add(part);
            }
        }

        userMessage.Parts = updatedParts;
        return userMessage;
    }
}
