// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.Profiling;
using System.IO;

namespace Anemone.Framework.Generators.Fastbuild;

[TargetGenerator]
public sealed class FastbuildTargetGenerator : TargetGenerator
{
    public FastbuildTargetGenerator()
        : base("FASTBuild-v1.0")
    {
    }

    public override void Generate(TargetContext context)
    {
        using var scope = Profiler.Profile($@"TargetGenerator: {context.Target.Name}");

        Directory.CreateDirectory(context.RulesDirectory);

        var fastbuildPlatform = new FastbuildTargetPlatform(context);
        fastbuildPlatform.WriteRules();

        var fastbuildSolution = new FastbuildTargetSolution(fastbuildPlatform);
        fastbuildSolution.WriteRules();

        foreach (var target in context.ResolvedTargets)
        {
            var fastbuildTarget = new FastbuildTargetRules(target);
            fastbuildTarget.WriteRules();
        }
    }
}
