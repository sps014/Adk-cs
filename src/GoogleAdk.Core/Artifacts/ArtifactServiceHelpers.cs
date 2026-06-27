using GoogleAdk.Core.Abstractions.Artifacts;

namespace GoogleAdk.Core.Artifacts;

/// <summary>
/// Shared logic for <see cref="IBaseArtifactService"/> implementations
/// (in-memory, file, and GCS) so each one validates input and interprets the
/// user-scope filename convention the same way.
/// </summary>
internal static class ArtifactServiceHelpers
{
    /// <summary>
    /// Prefix that marks an artifact as user-scoped (shared across sessions)
    /// rather than scoped to a single session.
    /// </summary>
    public const string UserScopePrefix = "user:";

    /// <summary>Validates that a save request carries some content to persist.</summary>
    /// <exception cref="ArgumentException">Thrown when the artifact has neither inline data nor text.</exception>
    public static void EnsureHasContent(SaveArtifactRequest request)
    {
        if (request.Artifact.InlineData == null && request.Artifact.Text == null)
        {
            throw new ArgumentException("Artifact must have either InlineData or Text content.");
        }
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="filename"/> targets the user scope.</summary>
    public static bool IsUserScoped(string filename)
        => filename.StartsWith(UserScopePrefix, StringComparison.Ordinal);

    /// <summary>Removes the user-scope prefix from <paramref name="filename"/> if present.</summary>
    public static string StripUserScope(string filename)
        => IsUserScoped(filename) ? filename.Substring(UserScopePrefix.Length) : filename;
}
