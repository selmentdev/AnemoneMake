// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Windows;

internal sealed class MsvcLibrarianRules : LinkerRules
{
    public MsvcLibrarianRules(WindowsPlatformRules _, ResolvedModule module, LinkerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.Output = $@"{this.Module.Name}.lib";
    }

    public override void Link(List<string> args, string output, string input)
    {
        args.Add(@"/nologo");

        args.Add($@"/OUT:""{output}""");
        args.Add($@"""{input}""");

        switch (this.Module.LinkKind)
        {
            case ModuleLinkKind.Application:
                {
                    throw new Exception(@"Application requires linker instead of archiver");
                }

            case ModuleLinkKind.ConsoleApplication:
                {
                    throw new Exception(@"Console application requires linker instead of archiver");
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    throw new Exception(@"Dynamic library requires linker instead of archiver");
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    break;
                }

            case ModuleLinkKind.ImportedLibrary:
                {
                    throw new Exception(@"Imported library is already linked.");
                }
        }
    }
}
