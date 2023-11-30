// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents a linker descriptor.
/// </summary>
public sealed class LinkerDescriptor : ToolDescriptor
{
    public LinkerDescriptor(TargetPlatform platform, TargetArchitecture architecture, TargetToolchain toolchain, string id)
        : base(platform, architecture, toolchain, id)
    {
    }
}
