using System.Runtime.CompilerServices;
using GoogleAdk.Core;

namespace GoogleAdk.Core.Tests;

public static class TestEnvInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        AdkEnv.Load();
    }
}
