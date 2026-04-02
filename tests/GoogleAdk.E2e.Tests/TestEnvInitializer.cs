using System.Runtime.CompilerServices;
using GoogleAdk.Core;

namespace GoogleAdk.E2e.Tests;

public static class TestEnvInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        AdkEnv.Load();
    }
}
