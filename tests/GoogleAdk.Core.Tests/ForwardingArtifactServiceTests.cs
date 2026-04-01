using GoogleAdk.Core.Abstractions.Artifacts;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Artifacts;
using GoogleAdk.Core.Tools;

namespace GoogleAdk.Core.Tests;

public class ForwardingArtifactServiceTests
{
    private sealed class DummyAgent : BaseAgent
    {
        public DummyAgent() : base(new BaseAgentConfig { Name = "agent" }) { }

        protected override async IAsyncEnumerable<Event> RunAsyncImpl(
            InvocationContext context,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    [Fact]
    public async Task ForwardingArtifactService_UsesParentContext()
    {
        var artifactService = new InMemoryArtifactService();
        var session = Session.Create("session-1", "app-1", "user-1");

        var invocationContext = new InvocationContext
        {
            Agent = new DummyAgent(),
            Session = session,
            ArtifactService = artifactService,
        };

        var agentContext = new AgentContext(invocationContext);
        var forwarding = new ForwardingArtifactService(agentContext);

        await forwarding.SaveArtifactAsync(new SaveArtifactRequest
        {
            AppName = "ignored",
            UserId = "ignored",
            SessionId = "ignored",
            Filename = "file.txt",
            Artifact = new Part { Text = "hello" },
        });

        var loaded = await artifactService.LoadArtifactAsync(new LoadArtifactRequest
        {
            AppName = "app-1",
            UserId = "user-1",
            SessionId = "session-1",
            Filename = "file.txt",
        });

        Assert.NotNull(loaded);
        Assert.Equal("hello", loaded!.Text);
    }
}
