// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.IO;

namespace Anemone.Framework.Sdks.Vulkan;

public sealed class VulkanSdkRules : SdkRules
{
    public VulkanSdkRules(string root)
    {
        this.IncludePaths = new[]
        {
            Path.Combine(root, "Include"),
        };

        this.LibraryPaths = new[]
        {
            Path.Combine(root, "Lib"),
        };
    }
}
