// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents a resource compiler descriptor.
/// </summary>
public sealed class ResourceCompilerDescriptor : ToolDescriptor
{
    public ResourceCompilerDescriptor(TargetPlatform platform, TargetArchitecture architecture, TargetToolchain toolchain, string id)
        : base(platform, architecture, toolchain, id)
    {
    }
}
