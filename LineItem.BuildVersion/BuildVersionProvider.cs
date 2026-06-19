using System;
using System.Linq;
using System.Reflection;

namespace LineItem.BuildVersion;

public static class BuildVersionProvider
{
    /// <summary>
    ///     Using Lazy to ensure that the value is only loaded when the version is actually requested, which can improve
    ///     performance and reduce memory usage.
    /// </summary>
    private static readonly Lazy<string> _cachedVersion = new(() =>
    {
        return Assembly.GetEntryAssembly()?
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attr => attr.Key == "BuildVersion")?
            .Value ?? "unknown";
    });

    /// <summary>
    ///     Retrieves the custom BuildVersion baked into the entry assembly during the build.
    /// </summary>
    public static string GetVersion()
    {
        return _cachedVersion.Value;
    }
}