// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Framework.Sdks.GamingDesktop;
using Anemone.Framework.Sdks.VisualStudio;
using Anemone.Framework.Sdks.Windows;
using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.GamingDesktop;

[PlatformRulesCreator]
public sealed class GamingDesktopPlatformRulesCreator : PlatformRulesCreator
{
    public override IEnumerable<PlatformRules> Create()
    {
        if (OperatingSystem.IsWindows())
        {
            var sdkVisualStudio = VisualStudioSdkLocator.Locate(
                TargetArchitecture.X64,
                "vs2022");

            var sdkWindows = WindowsSdkLocator.Locate(
                TargetArchitecture.X64,
                "10.0.19041.0");

            var sdkGamingDesktop = GamingDesktopSdkLocator.Locate(
                "220601");

            if (sdkVisualStudio != null && sdkWindows != null && sdkGamingDesktop != null)
            {
                yield return new GamingDesktopPlatformRules(s_Descriptor);
            }
        }
    }

    private static readonly PlatformDescriptor s_Descriptor = new(
        "GamingDesktop-X64",
        TargetPlatform.GamingDesktop,
        TargetArchitecture.X64,
        TargetToolchain.MSVC);
}
