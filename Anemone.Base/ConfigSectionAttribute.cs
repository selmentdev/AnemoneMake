// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Anemone.Base;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ConfigSectionAttribute : Attribute
{
    public ConfigSectionAttribute(string path)
    {
        this.Path = path;
    }

    public readonly string Path;
}
