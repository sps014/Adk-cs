using System.Runtime.CompilerServices;
using GoogleAdk.Models.Gemini;

namespace GoogleAdk.Core;

internal static class ModelDefaultsInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        GeminiModelFactory.RegisterDefaults();
    }
}
