// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework.Tests;

internal class MockPlatformRules : PlatformRules
{
    public MockPlatformRules()
        : base(new PlatformDescriptor("Mock", TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.MSVC))
    {
    }

    public override IReadOnlyCollection<CodeGeneratorRules> CreateCodeGenerators(ResolvedModule module)
    {
        throw new System.NotImplementedException();
    }

    public override CompilerRules CreateCompiler(ResolvedModule module)
    {
        throw new System.NotImplementedException();
    }

    public override LinkerRules CreateLinker(ResolvedModule module)
    {
        throw new System.NotImplementedException();
    }

    public override IReadOnlyCollection<ResourceCompilerRules> CreateResourceCompilers(ResolvedModule module)
    {
        throw new System.NotImplementedException();
    }
}
