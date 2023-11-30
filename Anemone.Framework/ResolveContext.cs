// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents target resolve context.
/// </summary>
/// <remarks>
///     This class is used to pass platform specific information about evaluated target and modules.
/// </remarks>
public sealed class ResolveContext
{
    public TargetContext TargetContext { get; }

    public PlatformRules PlatformRules => this.TargetContext.Platform;

    public TargetPlatform Platform => this.PlatformRules.Platform;
    public TargetArchitecture Architecture => this.PlatformRules.Architecture;
    public TargetToolchain Toolchain => this.PlatformRules.Toolchain;
    public TargetConfiguration Configuration { get; }

    public ResolveContext(
        TargetContext targetContext,
        TargetConfiguration configuration)
    {
        this.TargetContext = targetContext;
        this.Configuration = configuration;
    }
}
