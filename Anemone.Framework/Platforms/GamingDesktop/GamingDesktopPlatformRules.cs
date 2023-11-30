// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.GamingDesktop;

public sealed class GamingDesktopPlatformRules : PlatformRules
{
    public GamingDesktopPlatformRules(PlatformDescriptor descriptor)
        : base(descriptor)
    {
    }

    public override IReadOnlyCollection<CodeGeneratorRules> CreateCodeGenerators(ResolvedModule module)
    {
        throw new NotImplementedException();
    }

    public override CompilerRules CreateCompiler(ResolvedModule module)
    {
        throw new NotImplementedException();
    }

    public override LinkerRules CreateLinker(ResolvedModule module)
    {
        throw new NotImplementedException();
    }

    public override IReadOnlyCollection<ResourceCompilerRules> CreateResourceCompilers(ResolvedModule module)
    {
        throw new NotImplementedException();
    }
}
