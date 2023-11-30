// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents kind of a module.
/// </summary>
public enum ModuleKind
{
    GameApplication,
    GameLibrary,

    EditorApplication,
    EditorLibrary,

    TestApplication,
    BenchmarkApplication,
    ConsoleApplication,

    RuntimeLibrary,

    ThirdPartyLibrary,
}
