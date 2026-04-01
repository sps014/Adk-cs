using GoogleAdk.Core.Abstractions.Artifacts;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Forwards artifact operations to the parent invocation context.
/// </summary>
public sealed class ForwardingArtifactService : IBaseArtifactService
{
    private readonly AgentContext _context;

    public ForwardingArtifactService(AgentContext context)
    {
        _context = context;
    }

    public Task<int> SaveArtifactAsync(SaveArtifactRequest request)
    {
        var service = GetServiceOrThrow();
        return service.SaveArtifactAsync(WithContext(request));
    }

    public Task<Part?> LoadArtifactAsync(LoadArtifactRequest request)
    {
        var service = GetServiceOrThrow();
        return service.LoadArtifactAsync(WithContext(request));
    }

    public Task<List<string>> ListArtifactKeysAsync(ListArtifactKeysRequest request)
    {
        var service = GetServiceOrThrow();
        return service.ListArtifactKeysAsync(new ListArtifactKeysRequest
        {
            AppName = _context.AppName,
            UserId = _context.UserId,
            SessionId = _context.Session.Id,
        });
    }

    public Task DeleteArtifactAsync(DeleteArtifactRequest request)
    {
        var service = GetServiceOrThrow();
        return service.DeleteArtifactAsync(new DeleteArtifactRequest
        {
            AppName = _context.AppName,
            UserId = _context.UserId,
            SessionId = _context.Session.Id,
            Filename = request.Filename,
        });
    }

    public Task<List<int>> ListVersionsAsync(ListVersionsRequest request)
    {
        var service = GetServiceOrThrow();
        return service.ListVersionsAsync(WithContext(request));
    }

    public Task<List<ArtifactVersion>> ListArtifactVersionsAsync(ListVersionsRequest request)
    {
        var service = GetServiceOrThrow();
        return service.ListArtifactVersionsAsync(WithContext(request));
    }

    public Task<ArtifactVersion?> GetArtifactVersionAsync(LoadArtifactRequest request)
    {
        var service = GetServiceOrThrow();
        return service.GetArtifactVersionAsync(WithContext(request));
    }

    private IBaseArtifactService GetServiceOrThrow()
    {
        return _context.InvocationContext.ArtifactService
               ?? throw new InvalidOperationException("Artifact service is not available in the parent context.");
    }

    private SaveArtifactRequest WithContext(SaveArtifactRequest request)
    {
        return new SaveArtifactRequest
        {
            AppName = _context.AppName,
            UserId = _context.UserId,
            SessionId = _context.Session.Id,
            Filename = request.Filename,
            Artifact = request.Artifact,
            CustomMetadata = request.CustomMetadata,
        };
    }

    private LoadArtifactRequest WithContext(LoadArtifactRequest request)
    {
        return new LoadArtifactRequest
        {
            AppName = _context.AppName,
            UserId = _context.UserId,
            SessionId = _context.Session.Id,
            Filename = request.Filename,
            Version = request.Version,
        };
    }

    private ListVersionsRequest WithContext(ListVersionsRequest request)
    {
        return new ListVersionsRequest
        {
            AppName = _context.AppName,
            UserId = _context.UserId,
            SessionId = _context.Session.Id,
            Filename = request.Filename,
        };
    }
}
