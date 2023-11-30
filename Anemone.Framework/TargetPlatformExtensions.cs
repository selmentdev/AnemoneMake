// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Anemone.Framework;

internal static class TargetPlatformExtensions
{
    private static readonly TargetPlatform[] s_AllPlatforms = {
        TargetPlatform.Windows,
        TargetPlatform.UniversalWindows,
        TargetPlatform.XboxGaming,
        TargetPlatform.GamingDesktop,
        TargetPlatform.Android,
        TargetPlatform.Linux,
    };

    private static readonly TargetPlatform[] s_MobilePlatforms = {
        TargetPlatform.Android,
    };

    private static readonly TargetPlatform[] s_ConsolePlatforms = {
        TargetPlatform.XboxGaming,
        TargetPlatform.GamingDesktop, // PC treated as console
    };

    private static readonly TargetPlatform[] s_DesktopPlatforms = {
        TargetPlatform.Windows,
        TargetPlatform.Linux,
    };

    private static readonly TargetPlatform[] s_EditorPlatforms = {
        TargetPlatform.Windows,
        TargetPlatform.Linux,
    };

    private static readonly TargetPlatform[] s_ServerPlatforms = {
        TargetPlatform.Windows,
        TargetPlatform.Linux,
    };

    public static TargetPlatform[] ToPlatforms(this TargetPlatformKind self)
    {
        switch (self)
        {
            case TargetPlatformKind.All:
                {
                    return s_AllPlatforms;
                }

            case TargetPlatformKind.Mobile:
                {
                    return s_MobilePlatforms;
                }

            case TargetPlatformKind.Console:
                {
                    return s_ConsolePlatforms;
                }

            case TargetPlatformKind.Desktop:
                {
                    return s_DesktopPlatforms;
                }

            case TargetPlatformKind.Editor:
                {
                    return s_EditorPlatforms;
                }

            case TargetPlatformKind.Server:
                {
                    return s_ServerPlatforms;
                }

            default:
                {
                    throw new NotSupportedException();
                }
        }
    }
}
