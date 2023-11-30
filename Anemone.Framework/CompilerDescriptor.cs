// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents a compiler descriptor.
/// </summary>
public sealed class CompilerDescriptor : ToolDescriptor
{
    public CompilerDescriptor(TargetPlatform platform, TargetArchitecture architecture, TargetToolchain toolchain, string id)
        : base(platform, architecture, toolchain, id)
    {
    }
}
