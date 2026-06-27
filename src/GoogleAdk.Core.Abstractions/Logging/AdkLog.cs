using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GoogleAdk.Core.Abstractions.Logging;

/// <summary>
/// Central entry point for ADK library logging.
/// </summary>
/// <remarks>
/// Many ADK components (agents, services, tools) are created directly with
/// <c>new</c> rather than through a dependency-injection container, so they cannot
/// rely on constructor-injected <see cref="ILogger"/> instances. This facade gives
/// them a single, swappable source of loggers.
/// <para>
/// By default the facade is wired to <see cref="NullLoggerFactory"/>, so the library
/// is silent unless a host opts in. A host (for example the ADK API server or a
/// console app) calls <see cref="Configure(ILoggerFactory)"/> once at startup to route
/// ADK diagnostics into its own logging pipeline.
/// </para>
/// </remarks>
public static class AdkLog
{
    private static ILoggerFactory _factory = NullLoggerFactory.Instance;

    /// <summary>
    /// Gets or sets the <see cref="ILoggerFactory"/> used to create ADK loggers.
    /// Setting this to <see langword="null"/> throws; use <see cref="NullLoggerFactory.Instance"/>
    /// to disable logging instead.
    /// </summary>
    public static ILoggerFactory Factory
    {
        get => _factory;
        set => _factory = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Routes ADK library logging through the supplied <paramref name="factory"/>.</summary>
    /// <param name="factory">The factory ADK components should use to create loggers.</param>
    public static void Configure(ILoggerFactory factory) => Factory = factory;

    /// <summary>Creates a logger whose category is derived from <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type the logger is associated with.</typeparam>
    public static ILogger<T> CreateLogger<T>() => _factory.CreateLogger<T>();

    /// <summary>Creates a logger for the given <paramref name="categoryName"/>.</summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    public static ILogger CreateLogger(string categoryName) => _factory.CreateLogger(categoryName);
}
