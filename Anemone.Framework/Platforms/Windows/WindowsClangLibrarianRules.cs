// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Windows;

internal sealed class WindowsClangLibrarianRules : LinkerRules
{
    private readonly WindowsPlatformRules m_Platform;

    public WindowsClangLibrarianRules(WindowsPlatformRules platform, ResolvedModule module, LinkerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.m_Platform = platform;
        this.Output = $@"lib{this.Module.Name}.lib";
    }

    public override void Link(List<string> args, string output, string input)
    {
        args.Add($@"rcs ""{output}""");
        args.Add($@"""{input}""");
    }
}
