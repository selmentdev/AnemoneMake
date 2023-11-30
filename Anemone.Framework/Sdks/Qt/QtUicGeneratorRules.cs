// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework.Sdks.Qt;

internal sealed class QtUicGeneratorRules : CodeGeneratorRules
{
    public QtUicGeneratorRules(ResolvedModule module, CodeGeneratorDescriptor descriptor)
        : base(module, descriptor)
    {
    }

    public override void Generate(List<string> args, string output, string input)
    {
        args.Add($@"""{input}""");
        args.Add($@"-o ""{output}""");
    }
}
