// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Framework.Sdks.VisualStudio;
using Anemone.Framework.Sdks.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Anemone.Framework.Platforms.Windows;

[PlatformRulesCreator]
public sealed class WindowsPlatformRulesCreator : PlatformRulesCreator
{
    public override IEnumerable<PlatformRules> Create()
    {
        if (OperatingSystem.IsWindows())
        {
            var versionVisualStudio = "vs2022";
            var versionWindowsSdk = "10.0.22621.0";

            foreach (var descriptor in Descriptors)
            {

                var sdkVisualStudio = VisualStudioSdkLocator.Locate(
                    descriptor.Architecture,
                    versionVisualStudio);

                var sdkWindows = WindowsSdkLocator.Locate(
                    descriptor.Architecture,
                    versionWindowsSdk);

                if (sdkVisualStudio == null)
                {
                    Trace.TraceWarning($@"Cannot create {descriptor.Moniker} platform. Missing Visual Studio {versionVisualStudio} installation.");
                }

                if (sdkWindows == null)
                {
                    Trace.TraceWarning($@"Cannot create {descriptor.Moniker} platform. Missing Windows SDK {versionWindowsSdk}");
                }

                if (sdkVisualStudio != null && sdkWindows != null)
                {
                    yield return new WindowsPlatformRules(
                        descriptor,
                        sdkWindows,
                        sdkVisualStudio);
                }
            }
        }
        else
        {
            Trace.TraceWarning($@"Windows host required. No cross-compilation available on {System.Environment.OSVersion.VersionString}");
        }
    }

    private static IReadOnlyCollection<PlatformDescriptor> Descriptors { get; } = new[]
    {
        new PlatformDescriptor("Windows-X64", TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.MSVC),
        new PlatformDescriptor("Windows-X64-MSVC", TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.MSVC),
        new PlatformDescriptor("Windows-X64-Clang", TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.Clang),
        new PlatformDescriptor("Windows-X64-ClangCL", TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.ClangCL),
        new PlatformDescriptor("Windows-AArch64", TargetPlatform.Windows, TargetArchitecture.AArch64, TargetToolchain.MSVC),
        new PlatformDescriptor("Windows-AArch64-MSVC", TargetPlatform.Windows, TargetArchitecture.AArch64, TargetToolchain.MSVC),
    };
}
