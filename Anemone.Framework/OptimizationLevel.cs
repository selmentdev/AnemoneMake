// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents optimization level.
/// </summary>
public enum OptimizationLevel
{
    /// <summary>
    ///    No optimizations.
    /// </summary>
    Debug,

    /// <summary>
    ///     Some optimizations allowing to debug code.
    /// </summary>
    Development,

    /// <summary>
    ///    Optimizations for release.
    /// </summary>
    Optimized,

    /// <summary>
    ///     All available optimizations.
    /// </summary>
    Full,
}
