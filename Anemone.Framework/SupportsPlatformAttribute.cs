// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Anemone.Framework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SupportsPlatformAttribute : Attribute
{
    public readonly TargetPlatform[] Platforms;

    public SupportsPlatformAttribute(params TargetPlatform[] platforms)
    {
        this.Platforms = platforms;
    }

    public SupportsPlatformAttribute(TargetPlatformKind kind)
    {
        this.Platforms = kind.ToPlatforms();
    }
}
