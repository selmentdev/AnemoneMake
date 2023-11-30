// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.IO;

namespace Anemone.Framework.Sdks.Vulkan;

public static class VulkanSdkLocator
{
    private static readonly string? s_Root;

    static VulkanSdkLocator()
    {
        var rootPath = System.Environment.GetEnvironmentVariable("VULKAN_SDK");

        if (rootPath != null && Directory.Exists(rootPath))
        {
            s_Root = rootPath;
        }
    }

    public static VulkanSdkRules? Locate(TargetPlatform platform, TargetArchitecture architecture, TargetToolchain toolchain)
    {
        _ = toolchain;

        if (System.OperatingSystem.IsWindows() && platform == TargetPlatform.Windows && architecture == TargetArchitecture.X64)
        {
            if (s_Root != null)
            {
                return new VulkanSdkRules(s_Root);
            }
        }

        return null;
    }
}
