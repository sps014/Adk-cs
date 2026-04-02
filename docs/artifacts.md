# Artifacts

Artifacts are the explicit outputs produced by agents during a session. While conversation text is transient, artifacts are tangible assets like generated CSV files, compiled source code, data reports, or created images. 

The ADK provides a robust mechanism to save, load, and manage these artifacts persistently.

## Artifact Services

The ADK includes several built-in services to handle artifact storage depending on your environment.

- **`InMemoryArtifactService`**: Stores artifacts in memory. Excellent for unit testing or ephemeral CLI applications.
- **`FileArtifactService`**: Stores artifacts on the local filesystem. Suitable for desktop applications or local development.
- **`GcsArtifactService`**: Stores artifacts remotely in Google Cloud Storage. Supports versioning and is recommended for production cloud environments.

## Configuring Artifacts in the Runner

Agents rely on the `Runner` to provide access to the Artifact Service. You must configure the artifact service when bootstrapping your application.

```csharp
using GoogleAdk.Core.Runner;
using GoogleAdk.Core.Artifacts;

// Configure the runner to save artifacts to the local "output_files" folder
var runner = new Runner(new RunnerConfig
{
    AppName = "my_app",
    Agent = myAgent,
    ArtifactService = new FileArtifactService("output_files")
});
```

## Creating Artifacts within an Agent (Inline Data)

When an agent needs to produce an artifact (like generating a text report or an image), the `SaveFilesAsArtifactsPlugin` can automatically intercept `InlineData` objects attached to an `Event` and store them in the configured `ArtifactService`.

If you write a custom tool that generates a file, you can manually push it to the artifact service using the `AgentContext`.

```csharp
using GoogleAdk.Core.Artifacts;
using GoogleAdk.Core.Abstractions.Models;

// Example of saving an artifact manually
var service = new FileArtifactService("my_artifacts_folder");

await service.SaveArtifactAsync(new SaveArtifactRequest
{
    AppName = "my_app",
    UserId = "user_123",
    SessionId = "session_456",
    Filename = "summary_report.txt",
    // Wrap the artifact content inside a Part
    Artifact = new Part { Text = "This is the final summary output." }
});
```

## Loading Artifacts

If an agent or external process needs to read an existing artifact, it can request it by filename from the service.

```csharp
var loadedPart = await service.LoadArtifactAsync(new LoadArtifactRequest
{
    AppName = "my_app",
    UserId = "user_123",
    SessionId = "session_456",
    Filename = "summary_report.txt"
});

Console.WriteLine(loadedPart?.Text); 
// Output: This is the final summary output.
```