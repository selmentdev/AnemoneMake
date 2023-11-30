// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

public sealed class PlatformDescriptor
{
    public string Moniker { get; }
    public TargetPlatform Platform { get; }
    public TargetArchitecture Architecture { get; }
    public TargetToolchain Toolchain { get; }

    public PlatformDescriptor(
        string moniker,
        TargetPlatform platform,
        TargetArchitecture architecture,
        TargetToolchain toolchain)
    {
        this.Moniker = moniker;
        this.Platform = platform;
        this.Architecture = architecture;
        this.Toolchain = toolchain;
    }
}
