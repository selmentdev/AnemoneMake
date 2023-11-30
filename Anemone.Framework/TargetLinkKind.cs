// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Enumerates all supported target link kinds.
/// </summary>
public enum TargetLinkKind
{
    /// <summary>
    ///     Link modules into a separate binary.
    /// </summary>
    Modular,

    /// <summary>
    ///     Link all modules into a single binary.
    /// </summary>
    Monolithic,
}
