// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Enumerates all supported target toolchains.
/// </summary>
public enum TargetToolchain
{
    Default,
    Clang,
    ClangCL,
    MSVC,
    Intel,
    GCC,
}
