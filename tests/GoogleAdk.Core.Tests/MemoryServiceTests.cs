using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Memory;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Memory;

namespace GoogleAdk.Core.Tests;

public class MemoryServiceTests
{
    private static Event UserEvent(string text) => new()
    {
        Author = "user",
        Content = new Content { Role = "user", Parts = [new Part { Text = text }] }
    };

    [Fact]
    public async Task SearchMemory_KeywordMatch_ReturnsMatchingSessionEvent()
    {
        var service = new InMemoryMemoryService();
        var session = Session.Create("s1", "app", "u1");
        session.Events.Add(UserEvent("The capital of France is Paris"));
        await service.AddSessionToMemoryAsync(session);

        var response = await service.SearchMemoryAsync(new SearchMemoryRequest
        {
            AppName = "app",
            UserId = "u1",
            Query = "paris"
        });

        Assert.Single(response.Memories);
        Assert.Contains("Paris", response.Memories[0].Content?.Parts?[0].Text);
    }

    [Fact]
    public async Task SearchMemory_NoKeywordMatch_ReturnsEmpty()
    {
        var service = new InMemoryMemoryService();
        var session = Session.Create("s1", "app", "u1");
        session.Events.Add(UserEvent("Hello world"));
        await service.AddSessionToMemoryAsync(session);

        var response = await service.SearchMemoryAsync(new SearchMemoryRequest
        {
            AppName = "app",
            UserId = "u1",
            Query = "unrelated"
        });

        Assert.Empty(response.Memories);
    }

    [Fact]
    public async Task SearchMemory_IsScopedToUser()
    {
        var service = new InMemoryMemoryService();
        var session = Session.Create("s1", "app", "u1");
        session.Events.Add(UserEvent("secret token banana"));
        await service.AddSessionToMemoryAsync(session);

        var otherUser = await service.SearchMemoryAsync(new SearchMemoryRequest
        {
            AppName = "app",
            UserId = "someone-else",
            Query = "banana"
        });

        Assert.Empty(otherUser.Memories);
    }

    [Fact]
    public async Task AddMemory_DirectEntry_IsSearchable()
    {
        var service = new InMemoryMemoryService();
        await service.AddMemoryAsync("app", "u1",
        [
            new MemoryEntry
            {
                Author = "assistant",
                Content = new Content { Role = "model", Parts = [new Part { Text = "remember the pelican" }] }
            }
        ]);

        var response = await service.SearchMemoryAsync(new SearchMemoryRequest
        {
            AppName = "app",
            UserId = "u1",
            Query = "pelican"
        });

        Assert.Single(response.Memories);
    }
}
