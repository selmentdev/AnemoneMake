// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace Anemone.Framework.Sdks.GamingDesktop;

public static class GamingDesktopSdkLocator
{
    private static readonly Dictionary<string, string> s_Sdks = new();

    static GamingDesktopSdkLocator()
    {
        if (OperatingSystem.IsWindows())
        {
            DiscoverSdks();
        }
    }

    [SupportedOSPlatform("windows")]
    private static void DiscoverSdks()
    {
        var envGameDk = Environment.GetEnvironmentVariable("GameDK");

        if (envGameDk != null)
        {
            foreach (var path in Directory.EnumerateDirectories(envGameDk, "*", SearchOption.TopDirectoryOnly))
            {
                var pathGrdkIni = Path.Combine(path, "GRDK", "grdk.ini");

                if (File.Exists(pathGrdkIni))
                {
                    var version = Path.GetFileName(path);

                    if (version != null)
                    {
                        s_Sdks.Add(version, path);
                    }
                }
            }
        }
    }

    public static GamingDesktopSdkRules? Locate(string version)
    {
        if (s_Sdks.TryGetValue(version, out var path))
        {
            return new GamingDesktopSdkRules(version, path);
        }

        return null;
    }
}
