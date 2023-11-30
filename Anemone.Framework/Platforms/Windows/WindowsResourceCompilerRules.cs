// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Windows;

internal sealed class WindowsResourceCompilerRules : ResourceCompilerRules
{
    private readonly PlatformRules m_Platform;

    public WindowsResourceCompilerRules(PlatformRules platform, ResolvedModule module, ResourceCompilerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.m_Platform = platform;
    }

    public override void Compile(List<string> args, string output, string input)
    {
        args.Add(@"/nologo");
        args.Add($@"/Fo""{output}""");

        foreach (var sdk in this.m_Platform.Sdks)
        {
            foreach (var path in sdk.IncludePaths)
            {
                args.Add($@"/I""{path}""");
            }
        }

        foreach (var path in this.Module.IncludePaths)
        {
            args.Add($@"/I""{path}""");
        }

        args.Add($@"""{input}""");
    }
}
