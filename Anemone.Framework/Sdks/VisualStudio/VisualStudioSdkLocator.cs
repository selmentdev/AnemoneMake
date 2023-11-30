// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Anemone.Framework.Sdks.VisualStudio;

public static class VisualStudioSdkLocator
{
    private static readonly string s_HostPrefix;

    static VisualStudioSdkLocator()
    {
        s_HostPrefix = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "Hostx64",
            Architecture.X86 => "Hostx86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => throw new NotSupportedException("Unsupported architecture"),
        };

        if (OperatingSystem.IsWindows())
        {
            DiscoverSdks();
        }
    }

    public static VisualStudioSdkRules? Locate(
        TargetArchitecture architecture,
        string key,
        bool prerelease)
    {
        if (s_Sdks.TryGetValue(key, out var instance) && instance.IsPrerelease == prerelease)
        {
            return new VisualStudioSdkRules(key, instance, architecture, s_HostPrefix);
        }

        return null;
    }

    public static VisualStudioSdkRules? Locate(TargetArchitecture architecture, string key)
    {
        if (s_Sdks.TryGetValue(key, out var instance))
        {
            return new VisualStudioSdkRules(key, instance, architecture, s_HostPrefix);
        }

        return null;
    }

    private static IEnumerable<(string Key, string? Toolkit, string Toolset)> GetToolkits(string prefix, string root)
    {
        var directory = new DirectoryInfo(Path.Combine(root, "VC", "Auxiliary", "Build"));

        //
        // Test default tools.
        //

        var defaultFile = Path.Combine(directory.FullName, "Microsoft.VCToolsVersion.default.txt");
        if (File.Exists(defaultFile))
        {
            yield return new(prefix, null, File.ReadAllLines(defaultFile).First());
        }


        //
        // Enumerate additional tools by toolkit.
        //

        var files = directory.EnumerateFiles("Microsoft.VCToolsVersion.*.txt", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var match = Regex.Match(file.Name, @"^Microsoft\.VCToolsVersion\.(.+)\.default\.txt$");
            if (match.Success)
            {
                var toolkit = match.Groups[1].Value;
                var toolset = File.ReadAllLines(file.FullName).First();

                yield return new($@"{prefix}-{toolkit}", toolkit, toolset);
            }
        }
    }

    private static string GetVisualStudioProductId(string value)
    {
        var lastDot = value.LastIndexOf('.');

        if (lastDot != -1)
        {
            return value[(lastDot + 1)..];
        }

        return value;
    }


    private static readonly Dictionary<string, VisualStudioInstance> s_Sdks = new();

    public static IReadOnlyDictionary<string, VisualStudioInstance> Sdks => s_Sdks;

    private static void DiscoverSdks()
    {
        var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

        if (programFiles != null)
        {
            var vswhere = Path.Combine(
                programFiles,
                "Microsoft Visual Studio",
                "Installer",
                "vswhere.exe");

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = vswhere,
                Arguments = "-prerelease -products * -format json -utf8",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

            if (process != null)
            {
                var document = JsonDocument.Parse(process.StandardOutput.ReadToEnd());

                foreach (var instance in document.RootElement.EnumerateArray())
                {
                    var isComplete = instance.GetProperty("isComplete").GetBoolean();
                    if (isComplete)
                    {
                        var installationVersion = instance.GetProperty("installationVersion").GetString() ?? throw new Exception("Missing installationVersion property");
                        var installationPath = instance.GetProperty("installationPath").GetString() ?? throw new Exception("Missing installationPath property");
                        var productId = instance.GetProperty("productId").GetString() ?? throw new Exception("Missing productId property");
                        var isPrerelease = instance.GetProperty("isPrerelease").GetBoolean();
                        var productLine = instance.GetProperty("catalog").GetProperty("productLineVersion").GetString() ?? throw new Exception("Missing productLineVersion property");

                        var toolkits = GetToolkits($@"vs{productLine}", installationPath);

                        foreach (var (key, toolkit, toolset) in toolkits)
                        {
                            var sdk = new VisualStudioInstance(
                                productId: GetVisualStudioProductId(productId),
                                productLine: productLine,
                                root: installationPath,
                                toolkit: toolkit,
                                toolsetVersion: Version.Parse(toolset),
                                productVersion: Version.Parse(installationVersion),
                                isPrerelease: isPrerelease);

                            s_Sdks.Add(key, sdk);
                        }
                    }
                }
            }
        }
    }
}
