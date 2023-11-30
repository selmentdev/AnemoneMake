// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;

namespace Anemone.Framework;

/// <summary>
///     Implements attribute used to mark target rules.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TargetRulesAttribute : Attribute
{
    /// <summary>
    ///     Gets source location of file containing target rules.
    /// </summary>
    public readonly string Location;

    /// <summary>
    ///     Creates new instance of <see cref="TargetRulesAttribute" />.
    /// </summary>
    /// <param name="location">
    ///     A source location of file containing target rules, filled by the compiler.
    /// </param>
    public TargetRulesAttribute([CallerFilePath] string location = "")
    {
        this.Location = location;
    }
}
