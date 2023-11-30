// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.Profiling;
using System.IO;
using System.Linq;

namespace Anemone.Framework.Generators.Fastbuild;

internal sealed class FastbuildTargetRules
{
    public ResolvedTarget ResolvedTarget { get; }

    public string Alias { get; }
    public string Moniker { get; }

    public FastbuildTargetRules(
        ResolvedTarget target)
    {
        this.Moniker = $@"{target.Rules.Configuration}";

        this.ResolvedTarget = target;


        this.Alias = $@"{this.ResolvedTarget.Platform.Moniker}-{this.Moniker}";
    }

    public void WriteRules()
    {
        using var scope = Profiler.Profile($@"TargetRules: {this.Alias}");

        var location = Path.Combine(this.ResolvedTarget.Rules.RulesDirectory, $@"Target-{this.Moniker}.bff");
        using (var writer = new FastbuildWriter(location))
        {
            this.WriteCopyBinaryDependencies(writer);

            this.WriteModules(writer);

            this.WriteTestAlias(writer);

            this.WriteTargetAlias(writer);
        }
    }

    private void WriteCopyBinaryDependencies(FastbuildWriter writer)
    {
        writer.WriteLine($@"Copy( 'Copy-Binary-Dependencies-{this.ResolvedTarget.Rules.Configuration}' ) {{");
        writer.Indent();
        {
            writer.WriteLine(".Source = {");
            writer.Indent();
            {
                foreach (var sdk in this.ResolvedTarget.Platform.Sdks)
                {
                    foreach (var path in sdk.GetDependencyFiles(this.ResolvedTarget))
                    {
                        writer.WriteLine($@"'{path}'");
                    }
                }
            }
            writer.Unindent();
            writer.WriteLine("}");

            // Apparently fastbuild requires leading directory separator for some reason...
            writer.WriteLine($@".Dest = '{Path.TrimEndingDirectorySeparator(this.ResolvedTarget.Rules.BinariesDirectory) + Path.DirectorySeparatorChar}'");
        }
        writer.Unindent();
        writer.WriteLine("}");
    }

    private void WriteModules(FastbuildWriter writer)
    {
        foreach (var module in this.ResolvedTarget.Sorted)
        {
            if (module.LinkKind == ModuleLinkKind.ImportedLibrary)
            {
                //
                // Skip imported libraries because they are pre-compiled.
                //
                // Properties of such library were already propagated to dependencies.
                //

                continue;
            }

            using var scope = Profiler.Profile(module.Name);

            var fastbuildModuleRules = new FastbuildModuleRules(this, module);
            fastbuildModuleRules.WriteRules(writer);
        }
    }

    private void WriteTestAlias(FastbuildWriter writer)
    {
        var testApps = this
            .ResolvedTarget
            .Transitive
            .Where(x => x.Kind is ModuleKind.TestApplication);

        writer.WriteLine($@"Alias( 'Tests-{this.ResolvedTarget.Rules.Configuration}' ) {{");
        writer.Indent();
        {
            writer.WriteLine(".Targets = {");
            writer.Indent();
            {
                writer.WriteLine("// Test applications");
                // Emit explicit test applications as tests
                foreach (var module in testApps)
                {
                    var moduleAlias = this.GetModuleAlias(module);
                    writer.WriteLine($@"'Test-{moduleAlias}'");
                }

                writer.WriteLine("// Test runs");
                foreach (var module in this.ResolvedTarget.Transitive)
                {
                    var moduleAlias = this.GetModuleAlias(module);
                    // Emit all other available test runs
                    foreach (var testRun in module.Rules.TestRuns)
                    {
                        writer.WriteLine($@"'Test-{moduleAlias}-{testRun.Name}'");
                    }
                }
            }
            writer.Unindent();
            writer.WriteLine("}");
        }
        writer.Unindent();
        writer.WriteLine(@"}");
    }

    private void WriteTargetAlias(FastbuildWriter writer)
    {
        writer.WriteLine($@"Alias( 'Target-{this.ResolvedTarget.Rules.Configuration}' ) {{");
        writer.Indent();
        {
            writer.WriteLine(".Targets = {");
            writer.Indent();
            {
                writer.WriteLine($@"'Copy-Binary-Dependencies-{this.ResolvedTarget.Rules.Configuration}'");

                foreach (var module in this.ResolvedTarget.Transitive)
                {
                    if (module.LinkKind == ModuleLinkKind.ImportedLibrary)
                    {
                        // Imported library won't be included as dependency, because nothing was compiled for it.
                    }
                    else
                    {
                        writer.WriteLine($@"'{this.GetModuleAlias(module)}'");
                    }
                }
            }
            writer.Unindent();
            writer.WriteLine("}");
        }
        writer.Unindent();
        writer.WriteLine("}");
    }

    public string GetModuleAlias(ResolvedModule module)
    {
        return $@"Module-{module.Name}-{this.ResolvedTarget.Rules.Configuration}";
    }
}
