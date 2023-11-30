// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;

namespace Anemone.Framework;

/// <summary>
///     Implements attribute used to mark a module.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ModuleRulesAttribute : Attribute
{
    /// <summary>
    ///     Gets source location of file containing module rules.
    /// </summary>
    public readonly string Location;

    /// <summary>
    ///     Gets project kind.
    /// </summary>
    public readonly ModuleKind Kind;

    /// <summary>
    ///     Creates new instance of <see cref="ModuleRulesAttribute" />.
    /// </summary>
    /// <param name="kind">
    ///     A module kind.
    /// </param>
    /// <param name="location">
    ///     A source location of file containing module rules, filled by the compiler.
    /// </param>
    public ModuleRulesAttribute(ModuleKind kind, [CallerFilePath] string location = "")
    {
        this.Location = location;
        this.Kind = kind;
    }
}
