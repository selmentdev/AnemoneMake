// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Anemone.Framework;

/// <summary>
///     Represents a module reference type.
/// </summary>
[Flags]
public enum ModuleReferenceType
{
    None = 0,

    Private = 1 << 0,
    Interface = 1 << 1,
    Public = Private | Interface,
}
