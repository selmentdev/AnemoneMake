// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Anemone.Framework.Sdks.Qt;

internal sealed class QtSdkInstance
{
    public readonly string Root;
    public readonly Version Version;
    public readonly TargetPlatform Platform;
    public readonly TargetArchitecture Architecture;

    public QtSdkInstance(string root, Version version, TargetPlatform platform, TargetArchitecture architecture)
    {
        this.Root = root;
        this.Version = version;
        this.Platform = platform;
        this.Architecture = architecture;
    }
}
