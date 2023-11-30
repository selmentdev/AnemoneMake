// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Anemone.Framework.Platforms.Linux;

[PlatformRulesCreator]
public sealed class LinuxPlatformRulesCreator : PlatformRulesCreator
{
    public override IEnumerable<PlatformRules> Create()
    {
        if (OperatingSystem.IsLinux())
        {
            foreach (var descriptor in Descriptors)
            {
                yield return new LinuxPlatformRules(descriptor);
            }
        }
        else
        {
            Trace.TraceWarning($@"Linux host required. No cross-compilation available on {System.Environment.OSVersion.VersionString}");
        }
    }

    private static IReadOnlyCollection<PlatformDescriptor> Descriptors { get; } = new[]
    {
        new PlatformDescriptor("Linux-X64", TargetPlatform.Linux, TargetArchitecture.X64, TargetToolchain.Clang),
        new PlatformDescriptor("Linux-X64-Clang", TargetPlatform.Linux, TargetArchitecture.X64, TargetToolchain.Clang),
        new PlatformDescriptor("Linux-X64-GCC", TargetPlatform.Linux, TargetArchitecture.X64, TargetToolchain.GCC),
        //new PlatformDescriptor("Linux-X64-Intel", TargetPlatform.Linux, TargetArchitecture.X64, TargetToolchain.Intel),
        //new PlatformDescriptor("Linux-AArch64", TargetPlatform.Linux, TargetArchitecture.AArch64, TargetToolchain.Clang),
        //new PlatformDescriptor("Linux-AArch64-Clang", TargetPlatform.Linux, TargetArchitecture.AArch64, TargetToolchain.Clang),
        //new PlatformDescriptor("Linux-AArch64-GCC", TargetPlatform.Linux, TargetArchitecture.AArch64, TargetToolchain.GCC),
        //new PlatformDescriptor("Linux-AArch64-Intel", TargetPlatform.Linux, TargetArchitecture.AArch64, TargetToolchain.Intel),
        //new PlatformDescriptor("Linux-RiscV64", TargetPlatform.Linux, TargetArchitecture.RiscV64, TargetToolchain.Clang),
        //new PlatformDescriptor("Linux-RiscV64-Clang", TargetPlatform.Linux, TargetArchitecture.RiscV64, TargetToolchain.Clang),
        //new PlatformDescriptor("Linux-RiscV64-GCC", TargetPlatform.Linux, TargetArchitecture.RiscV64, TargetToolchain.GCC),
        //new PlatformDescriptor("Linux-RiscV64-Intel", TargetPlatform.Linux, TargetArchitecture.RiscV64, TargetToolchain.Intel),
    };
}
