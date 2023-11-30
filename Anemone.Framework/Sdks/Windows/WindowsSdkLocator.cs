// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Anemone.Framework.Sdks.Windows;

public static class WindowsSdkLocator
{
    private static readonly string s_HostPrefix;

    private static readonly Dictionary<Version, string> s_Sdks = new();

    public static IReadOnlyDictionary<Version, string> Sdks => s_Sdks;

    static WindowsSdkLocator()
    {
        s_HostPrefix = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => throw new NotSupportedException("Unsupported architecture"),
        };

        if (OperatingSystem.IsWindows())
        {
            DiscoverSdks();
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool TryGetRegistryValueImpl(string key, string name, out string value)
    {
        var result = Registry.GetValue(key, name, null) as string;

        if (string.IsNullOrEmpty(result))
        {
            value = string.Empty;
            return false;
        }

        value = result;
        return true;
    }

    private static readonly string[] s_RegistryKeyRoots = {
        @"HKEY_CURRENT_USER\SOFTWARE\",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\",
        @"HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\",
    };

    [SupportedOSPlatform("windows")]
    private static bool TryGetRegistryValue(string key, string name, out string value)
    {
        foreach (var root in s_RegistryKeyRoots)
        {
            if (TryGetRegistryValueImpl(root + key, name, out value))
            {
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> GetRoots()
    {
        if (TryGetRegistryValue(@"Microsoft\Windows Kits\Installed Roots", "KitsRoot10", out var root1))
        {
            yield return root1;
        }

        if (TryGetRegistryValue("Microsoft\\Microsoft SDKs\\Windows\\v10.0", "InstallationFolder", out var root2))
        {
            yield return root2;
        }
    }

    [SupportedOSPlatform("windows")]
    private static void DiscoverSdks()
    {
        var roots = GetRoots().Distinct();

        foreach (var root in roots)
        {
            var includePath = Path.Combine(root, "Include");

            if (Directory.Exists(includePath))
            {
                foreach (var include in Directory.GetDirectories(includePath))
                {
                    if (Version.TryParse(Path.GetFileName(include), out var version) && File.Exists(Path.Combine(include, "um", "windows.h")))
                    {
                        s_Sdks.Add(version, root);
                    }
                }
            }
        }
    }

    private static string GetTargetPrefix(TargetArchitecture architecture)
    {
        return architecture switch
        {
            TargetArchitecture.X64 => "x64",
            TargetArchitecture.AArch64 => "arm64",
            _ => throw new NotSupportedException("Unsupported architecture"),
        };
    }

    public static WindowsSdkRules? Locate(TargetArchitecture architecture)
    {
        var max = s_Sdks.Keys.Max();

        if (max != null)
        {
            return new WindowsSdkRules(s_Sdks[max], max.ToString(), s_HostPrefix, GetTargetPrefix(architecture));
        }

        return null;
    }

    public static WindowsSdkRules? Locate(TargetArchitecture architecture, string version)
    {
        if (Version.TryParse(version, out var versionValue))
        {
            if (s_Sdks.TryGetValue(versionValue, out var root))
            {
                return new WindowsSdkRules(root, version, s_HostPrefix, GetTargetPrefix(architecture));
            }
        }

        return null;
    }
}
