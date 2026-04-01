namespace GoogleAdk.Core.Examples;

/// <summary>
/// Base class for example providers.
/// </summary>
public abstract class BaseExampleProvider
{
    public abstract IEnumerable<Example> GetExamples(string query);
}

public static class ExampleProviderUtils
{
    public static bool IsBaseExampleProvider(object? obj) => obj is BaseExampleProvider;
}
