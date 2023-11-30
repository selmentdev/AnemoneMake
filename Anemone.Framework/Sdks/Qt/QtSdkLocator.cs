// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Anemone.Framework.Sdks.Qt;

public static class QtSdkLocator
{
    private const string s_Root = @"D:\Qt";

    private static readonly Dictionary<string, QtSdkInstance> s_Sdks = new();
    private static readonly Dictionary<(TargetPlatform, TargetArchitecture), string> s_TargetPrefix = new()
    {
        { (TargetPlatform.Windows, TargetArchitecture.X64), "msvc2019_64" },
        { (TargetPlatform.Windows, TargetArchitecture.AArch64), "msvc2019_arm64" },
        { (TargetPlatform.Android, TargetArchitecture.X64), "android_x86_64" },
        { (TargetPlatform.Android, TargetArchitecture.AArch64), "android_arm64_v8a" },
    };

    static QtSdkLocator()
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            DiscoverSdks();
        }

        foreach (var sdk in s_Sdks)
        {
            Trace.TraceInformation($@"Qt: {sdk.Key}");
            Trace.TraceInformation($@"  - root:         {sdk.Value.Root}");
            Trace.TraceInformation($@"  - platform:     {sdk.Value.Platform}");
            Trace.TraceInformation($@"  - architecture: {sdk.Value.Architecture}");
            Trace.TraceInformation($@"  - version:      {sdk.Value.Version}");
        }
    }

    public static QtSdkRules? Locate(TargetPlatform platform, TargetArchitecture architecture, TargetToolchain toolchain, string version)
    {
        var key = $@"{version}-{platform}-{architecture}";

        if (s_Sdks.TryGetValue(key, out var instance))
        {
            return new QtSdkRules(instance, toolchain);
        }

        return null;
    }

    private static void DiscoverSdks()
    {
        if (Directory.Exists(s_Root))
        {
            foreach (var path in Directory.GetDirectories(s_Root))
            {
                var directory = new DirectoryInfo(path);

                if (Version.TryParse(directory.Name, out var version))
                {
                    foreach (var variant in directory.GetDirectories())
                    {
                        foreach (var ((platform, architecture), prefix) in s_TargetPrefix)
                        {
                            if (string.Equals(variant.Name, prefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                var key = $@"{version}-{platform}-{architecture}";

                                s_Sdks.Add(key, new QtSdkInstance(
                                    root: Path.Combine(directory.FullName, prefix),
                                    version,
                                    platform,
                                    architecture));
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
