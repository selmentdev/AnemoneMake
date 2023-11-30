// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Enumerates supported target architectures.
/// </summary>
/// <remarks>
///     All supported target architectures are 64-bit.
/// </remarks>
public enum TargetArchitecture
{
    X64,
    AArch64,
    RiscV64,
}
