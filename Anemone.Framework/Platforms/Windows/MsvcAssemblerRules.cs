// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Windows;

internal sealed class MsvcAssemblerRules : CompilerRules
{
    private readonly WindowsPlatformRules m_Platform;

    public MsvcAssemblerRules(WindowsPlatformRules platform, ResolvedModule module, CompilerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.m_Platform = platform;
    }

    public override void Compile(List<string> args, string output, string input)
    {
        #region Input and output

        args.Add(@"/nologo");
        args.Add(@"/quiet");

        args.Add(@"/Cp");
        args.Add(@"/Zi");
        args.Add($@"/c");
        args.Add($@"/Fo""{output}""");
        #endregion

        #region Module includes and defines
        // Includes
        foreach (var path in this.Module.IncludePaths)
        {
            args.Add($@"/I""{path}""");
        }

        // Defines.
        foreach (var item in this.Module.Defines)
        {
            args.Add($@"/D""{item}""");
        }

        args.Add($@"""{input}""");
        #endregion
    }
}
