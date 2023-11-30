// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.Profiling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Anemone.Framework;

public sealed class TargetContext
{
    public ProjectContext Context { get; }
    public PlatformRules Platform { get; }
    public TargetDescriptor Target { get; }
    public IReadOnlyCollection<ModuleDescriptor> Modules { get; }

    public string RootDirectory { get; }
    public string ContentDirectory { get; }
    public string SourceDirectory { get; }
    public string BuildDirectory { get; }
    public string GeneratedFilesDirectory { get; }
    public string BinariesDirectory { get; }
    public string IntermediateDirectory { get; }
    public string RulesDirectory { get; }
    public string ProjectFilesDirectory { get; }

    public static IReadOnlyCollection<TargetConfiguration> Configurations { get; } = Enum.GetValues<TargetConfiguration>();

    public IReadOnlyCollection<ResolvedTarget> ResolvedTargets { get; }

    public TargetContext(
        ProjectContext context,
        PlatformRules platform,
        TargetDescriptor target,
        IReadOnlyCollection<ModuleDescriptor> modules)
    {
        this.Context = context;
        this.Platform = platform;
        this.Target = target;
        this.Modules = modules;

        this.ContentDirectory = context.ProjectContentDirectory;
        this.SourceDirectory = context.ProjectSourceDirectory;

        var platformMoniker = $@"{this.Target.Name}-{this.Platform.Moniker}";

        this.RootDirectory = context.OutputDirectory;
        this.BuildDirectory = Path.Combine(context.OutputBuildDirectory, platformMoniker);
        this.GeneratedFilesDirectory = Path.Combine(context.OutputGeneratedDirectory, platformMoniker);
        this.BinariesDirectory = Path.Combine(context.OutputBinariesDirectory, platformMoniker);
        this.IntermediateDirectory = Path.Combine(context.OutputIntermediateDirectory, platformMoniker);
        this.RulesDirectory = Path.Combine(context.OutputRulesDirectory, platformMoniker);
        this.ProjectFilesDirectory = Path.Combine(context.OutputProjectFilesDirectory, platformMoniker);

        this.ResolvedTargets = this.Resolve().ToArray();
    }

    private IEnumerable<ResolvedTarget> Resolve()
    {
        using var scope = Profiler.Profile(@"Resolve Configurations");

        foreach (var configuration in Configurations)
        {
            using var scope2 = Profiler.Profile(configuration.ToString());

            var resolveContext = new ResolveContext(this, configuration);
            var target = this.CreateTarget(resolveContext);

            var modules = this.CreateModules(target);

            var resolvedTarget = new ResolvedTarget(this.Platform, target, modules);
            yield return resolvedTarget;
        }
    }

    private ModuleRules[] CreateModules(TargetRules targetRules)
    {
        using var scope = Profiler.Function();

        return this.Modules
            .Where(x => x.IsSupported(this.Platform.Platform))
            .Select(x => x.Create(targetRules))
            .ToArray();
    }

    private TargetRules CreateTarget(ResolveContext resolveContext)
    {
        using var scope = Profiler.Function();

        return this.Target.Create(resolveContext);
    }
}
