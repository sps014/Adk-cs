using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GoogleAdk.Core.Tests;

public class VertexAiSearchToolTests
{
    [Fact]
    public void Constructor_WithNeitherDataStoreNorEngine_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VertexAiSearchTool());
    }

    [Fact]
    public void Constructor_WithBothDataStoreAndEngine_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VertexAiSearchTool(dataStoreId: "store1", searchEngineId: "engine1"));
    }

    [Fact]
    public void Constructor_WithDataStoreSpecsButNoEngine_ThrowsArgumentException()
    {
        var specs = new List<VertexAiSearchDataStoreSpec> { new VertexAiSearchDataStoreSpec { DataStore = "store1" } };
        Assert.Throws<ArgumentException>(() => new VertexAiSearchTool(dataStoreId: "store1", dataStoreSpecs: specs));
    }

    [Fact]
    public async Task ProcessLlmRequestAsync_AppendsRetrievalToolToConfig()
    {
        var tool = new VertexAiSearchTool(dataStoreId: "projects/p1/locations/l1/collections/c1/dataStores/ds1");
        var request = new LlmRequest();
        var context = new AgentContext(new InvocationContext { Session = GoogleAdk.Core.Abstractions.Sessions.Session.Create("s1", "app", "u1") });

        await tool.ProcessLlmRequestAsync(context, request);

        Assert.NotNull(request.Config);
        Assert.NotNull(request.Config.Tools);
        Assert.Single(request.Config.Tools);

        var toolDecl = request.Config.Tools[0];
        Assert.NotNull(toolDecl.Retrieval);
        Assert.NotNull(toolDecl.Retrieval.VertexAiSearch);
        Assert.Equal("projects/p1/locations/l1/collections/c1/dataStores/ds1", toolDecl.Retrieval.VertexAiSearch.Datastore);
    }
}
