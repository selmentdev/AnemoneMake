// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Linux;

internal sealed class LinuxGccLibrarianRules : LinkerRules
{
    public LinuxGccLibrarianRules(LinuxPlatformRules _, ResolvedModule module, LinkerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.Output = $@"lib{this.Module.Name}.a";
    }

    public override void Link(List<string> args, string output, string input)
    {
        args.Add($@"rcs ""{output}""");
        args.Add($@"""{input}""");
    }
}
