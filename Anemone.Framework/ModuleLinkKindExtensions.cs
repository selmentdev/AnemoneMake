// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

internal static class ModuleLinkKindExtensions
{
    public static bool IsApplication(ModuleLinkKind kind)
    {
        return kind is ModuleLinkKind.Application or ModuleLinkKind.ConsoleApplication;
    }
}
