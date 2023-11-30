// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Anemone.Framework.Generators.Fastbuild;

internal sealed class FastbuildModuleRules
{
    private FastbuildTargetRules FastbuildTargetRules { get; }
    private ResolvedModule ResolvedModule { get; }

    public string Alias { get; }

    private List<string> GeneratedIncludeTargets { get; } = new();
    private List<string> GeneratedCompileTargets { get; } = new();
    private List<string> GeneratedLinkTargets { get; } = new();

    public FastbuildModuleRules(FastbuildTargetRules target, ResolvedModule module)
    {
        this.FastbuildTargetRules = target;
        this.ResolvedModule = module;

        this.Alias = this.FastbuildTargetRules.GetModuleAlias(this.ResolvedModule);
    }

    public void WriteRules(FastbuildWriter writer)
    {
        switch (this.ResolvedModule.LinkKind)
        {
            case ModuleLinkKind.Application:
            case ModuleLinkKind.ConsoleApplication:
                {
                    writer.WriteLine($@"Executable( '{this.Alias}' ) {{");
                    break;
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    writer.WriteLine($@"DLL( '{this.Alias}' ) {{");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    writer.WriteLine($@"Library( '{this.Alias}' ) {{");
                    break;
                }

            case ModuleLinkKind.ImportedLibrary:
                {
                    throw new Exception($@"Imported library '{this.ResolvedModule.Name}' is not supposed to be compiled");
                }
            default:
                {
                    throw new Exception("Unknown module kind");
                }
        }

        writer.Indent();
        {
            // TODO:
            //
            // Change API to force proper order.
            //  - compiler rules must come first as fastbuild expects them to be set globally for whole module,
            //  - linker rules must come last, because other tools may emit link-dependent rules as well.

            this.WriteCompilerRules(writer, this.ResolvedModule.Compiler);
            this.WriteResourceCompilerRules(writer);
            this.WriteCodeGeneratorRules(writer);
            this.WriteLinkerRules(writer);

            this.WriteModuleObjectFiles(writer);
        }
        writer.Unindent();
        writer.WriteLine("}");
        writer.WriteLine();

        if (this.ResolvedModule.Kind is ModuleKind.TestApplication)
        {
            writer.WriteLine($@"// Test rules for {this.Alias}");
            writer.WriteLine($@"Test( 'Test-{this.Alias}' ) {{");
            writer.Indent();
            {
                var testOutput = Path.Combine(this.ResolvedModule.Rules.BinariesDirectory, $@"{this.Alias}.test.log");
                writer.WriteLine($@".TestExecutable = '{this.Alias}'");
                writer.WriteLine($@".TestOutput = '{testOutput}'");
                writer.WriteLine(@".TestAlwaysShowOutput = true");
            }
            writer.Unindent();
            writer.WriteLine("}");
            writer.WriteLine();
        }

        foreach (var testRun in this.ResolvedModule.Rules.TestRuns)
        {
            writer.WriteLine($@"// Additional test rules for {this.Alias}-{testRun.Name}");
            writer.WriteLine($@"Exec( 'Test-{this.Alias}-{testRun.Name}' ) {{");
            writer.Indent();
            {
                var testOutput = Path.Combine(this.ResolvedModule.Rules.BinariesDirectory, $@"{this.Alias}-{testRun.Name}.test.log");
                writer.WriteLine($@".ExecExecutable = '{this.Alias}'");
                writer.WriteLine($@".ExecArguments = '{string.Join(' ', testRun.Arguments)}'");
                writer.WriteLine($@".ExecWorkingDir = '{testRun.WorkingDirectory}'");
                writer.WriteLine($@".ExecOutput = '{testOutput}'");
                writer.WriteLine(@".ExecUseStdOutAsOutput = true");
                writer.WriteLine(@".ExecAlwaysShowOutput = true");
                writer.WriteLine(@".ExecAlways = true");
            }
            writer.Unindent();
            writer.WriteLine("}");
            writer.WriteLine();
        }
    }

    private void WriteCompilerRules(FastbuildWriter writer, CompilerRules compiler)
    {
        var args = new List<string>();
        compiler.Compile(args, "%2", "%1");

        writer.WriteLine($@".Compiler = '{compiler.Descriptor.Id}'");
        writer.WriteLine(@".CompilerOptions = ''");
        writer.WriteCommandList(args);
    }

    private void WriteResourceCompilerRules(FastbuildWriter writer)
    {
        var args = new List<string>();

        foreach (var compiler in this.ResolvedModule.ResourceCompilers)
        {
            if (compiler.IsSupported)
            {
                var compilerOutputPath = this.ResolvedModule.Rules.ObjectFilesDirectory;

                var name = $@"{compiler.Descriptor.Id}-{this.Alias}";

                writer.WriteLine($@"ObjectList( '{name}' ) {{");
                writer.Indent();
                {
                    args.Clear();
                    compiler.Compile(args, "%2", "%1");

                    writer.WriteLine($@".Compiler = '{compiler.Descriptor.Id}'");
                    writer.WriteLine(@".CompilerOptions = ''");
                    writer.WriteCommandList(args);

                    writer.WriteLine($@".CompilerInputPath = '{compiler.SourceDirectory}'");
                    writer.WriteLine(@".CompilerInputPattern = {");
                    writer.Indent();
                    {
                        foreach (var patter in compiler.Descriptor.InputPatterns)
                        {
                            writer.WriteLine($@"'{patter}'");
                        }
                    }
                    writer.Unindent();
                    writer.WriteLine(@"}");
                    writer.WriteLine($@".CompilerOutputPath = '{compilerOutputPath}'");
                    writer.WriteLine($@".CompilerOutputExtension = '{compiler.Descriptor.OutputExtension}'");
                }
                writer.Unindent();
                writer.WriteLine(@"}");

                this.GeneratedLinkTargets.Add(name);
            }
        }
    }

    private void WriteCodeGeneratorRules(FastbuildWriter writer)
    {
        var generators = this.ResolvedModule.CodeGenerators.Where(x => x.IsSupported);

        foreach (var generator in generators)
        {
            var name = $@"{this.Alias}-{generator.Descriptor.Id}";
            var generatedOutputPath = generator.GeneratedFilesDirectory;// Path.Combine(this.ResolvedModule.Rules.GeneratedFilesDirectory, generator.Descriptor.OutputDirectory);
            var targetCodeGen = $@"{name}-Generate-Code";

            writer.WriteLine($@"// Code generator rules {generator.Descriptor.Id}");
            writer.WriteLine($@"ObjectList( '{targetCodeGen}' ) {{");
            writer.Indent();
            {
                writer.WriteLine($@".Compiler = '{generator.Descriptor.Id}'");

                var args = new List<string>();
                generator.Generate(args, "%2", "%1");

                writer.WriteLine(@".CompilerOptions = ''");
                writer.WriteCommandList(args);


                writer.WriteLine($@".CompilerInputPath = '{generator.SourceDirectory}'");
                writer.WriteLine(@".CompilerInputPattern = {");
                writer.Indent();
                {
                    foreach (var patter in generator.Descriptor.InputPatterns)
                    {
                        writer.WriteLine($@"'{patter}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine(@"}");
                writer.WriteLine($@".CompilerOutputPath = '{generatedOutputPath}'");
                writer.WriteLine($@".CompilerOutputExtension = '{generator.Descriptor.OutputExtension}'");
            }
            writer.Unindent();
            writer.WriteLine(@"}");

            if (generator.Descriptor.RequiresInclude)
            {
                // This generator outputs headers
                this.GeneratedIncludeTargets.Add(targetCodeGen);
            }

            if (generator.Descriptor.RequiresCompilation)
            {
#if false
                var targetCodeCompile = $@"{name}-Generate-Code-Compile";
                // TODO: Emit separate compilation unit in order to control where generated sources are put.

                writer.WriteLine($@"// Code generator compile {generator.Descriptor.Id}");
                writer.WriteLine($@"ObjectList( '{targetCodeCompile}' ) {{");
                writer.Indent();
                {
                    writer.WriteLine(@".Compiler")
                }
                writer.Unindent();
                writer.WriteLine("}");
                this.GeneratedLinkTargets.Add(targetCodeCompile);
#else
                this.GeneratedCompileTargets.Add(targetCodeGen);
#endif
            }
        }
    }

    private void WriteLinkerRules(FastbuildWriter writer)
    {
        var isStatic = this.ResolvedModule.LinkKind == ModuleLinkKind.StaticLibrary;

        var args = new List<string>();
        this.ResolvedModule.Linker.Link(args, "%2", "%1");

        var linkerOutputPath = Path.Combine(this.ResolvedModule.Rules.BinariesDirectory, this.ResolvedModule.Linker.Output);

        if (isStatic)
        {
            writer.WriteLine($@".Librarian = '{this.ResolvedModule.Linker.Descriptor.Executable}'");
            writer.WriteLine($@".LibrarianOutput = '{linkerOutputPath}'");

            writer.WriteLine(@".LibrarianAdditionalInputs = {");
            writer.Indent();
            {
                writer.WriteLine($@"'{this.Alias}-Objects'");
                if (this.ResolvedModule.EnableAssembly)
                {
                    writer.WriteLine($@"'{this.Alias}-Asm-Objects'");
                }
            }
            writer.Unindent();
            writer.WriteLine(@"}");

            writer.WriteLine(@".LibrarianOptions = ''");
            writer.WriteCommandList(args);
        }
        else
        {
            writer.WriteLine($@".Linker = '{this.ResolvedModule.Linker.Descriptor.Executable}'");
            writer.WriteLine($@".LinkerOutput = '{linkerOutputPath}'");

            writer.WriteLine(@".LinkerOptions = ''");
            writer.WriteCommandList(args);

            writer.WriteLine(@".Libraries = {");
            writer.Indent();
            {
                writer.WriteLine($@"'{this.Alias}-Objects'");

                if (this.ResolvedModule.EnableAssembly)
                {
                    writer.WriteLine($@"'{this.Alias}-Asm-Objects'");
                }

                foreach (var target in this.GeneratedLinkTargets)
                {
                    writer.WriteLine($@"'{target}'");
                }

                foreach (var dependency in this.ResolvedModule.Dependencies)
                {
                    if (dependency.LinkKind == ModuleLinkKind.ImportedLibrary)
                    {
                        // Imported library won't be included as dependency, because nothing was compiled for it.
                    }
                    else
                    {
                        writer.WriteLine($@"'{this.FastbuildTargetRules.GetModuleAlias(dependency)}'");
                    }
                }
            }
            writer.Unindent();
            writer.WriteLine(@"}");
        }
    }

    private void WriteModuleObjectFiles(FastbuildWriter writer)
    {
        var sourceInputPath = Path.Combine(this.ResolvedModule.Rules.SourceDirectory, this.ResolvedModule.Compiler.Descriptor.SourceDirectory);

        var compilerOutputPath = Path.Combine(this.ResolvedModule.Rules.ObjectFilesDirectory, "Source");
        var compilerOutputExtension = this.ResolvedModule.Compiler.Descriptor.OutputExtension;

        var objectTargetName = $@"{this.Alias}-Objects";

        var unityTargetName = this.ResolvedModule.EnableUnity ? $@"{this.Alias}-Unity" : null;

        if (unityTargetName != null)
        {
            //
            // Create unity section for sources.
            //

            writer.WriteLine($@"Unity( '{unityTargetName}' ) {{");
            writer.Indent();
            {
                writer.WriteLine($@".UnityInputPath = '{sourceInputPath}'");
                writer.WriteLine(@".UnityInputPattern = {");
                writer.Indent();
                {
                    foreach (var pattern in this.ResolvedModule.Compiler.Descriptor.InputPatterns)
                    {
                        writer.WriteLine($@"'{pattern}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine(@"}");
                writer.WriteLine($@".UnityOutputPath = '{this.ResolvedModule.Rules.UnityFilesDirectory}'");
                writer.WriteLine($@".UnityOutputPattern = '{this.ResolvedModule.Name}-*.cxx'");
            }
            writer.Unindent();
            writer.WriteLine(@"}");
        }

        // Emit main object list for compiler files
        writer.WriteLine($@"ObjectList( '{objectTargetName}' ) {{");
        writer.Indent();
        {
            if (unityTargetName != null)
            {
                writer.WriteLine($@".CompilerInputUnity = '{unityTargetName}'");
            }
            else
            {
                writer.WriteLine($@".CompilerInputPath = '{sourceInputPath}'");
                writer.WriteLine(@".CompilerInputPattern = {");
                writer.Indent();
                {
                    foreach (var pattern in this.ResolvedModule.Compiler.Descriptor.InputPatterns)
                    {
                        writer.WriteLine($@"'{pattern}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine(@"}");
            }

            writer.WriteLine($@".CompilerOutputPath = '{compilerOutputPath}'");
            writer.WriteLine($@".CompilerOutputExtension = '{compilerOutputExtension}'");

            if (this.GeneratedCompileTargets.Count > 0)
            {
                writer.WriteLine(@".CompilerInputObjectLists = {");
                writer.Indent();
                {
                    foreach (var target in this.GeneratedCompileTargets)
                    {
                        writer.WriteLine($@"'{target}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine("}");
            }

            if (this.GeneratedIncludeTargets.Count > 0)
            {
                writer.WriteLine(@".PreBuildDependencies = {");
                writer.Indent();
                {
                    foreach (var target in this.GeneratedIncludeTargets)
                    {
                        writer.WriteLine($@"'{target}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine(@"}");
            }
        }
        writer.Unindent();
        writer.WriteLine(@"}");

        // Emit optional assembly-specific object list
        if (this.ResolvedModule.EnableAssembly)
        {
            var assembler = this.ResolvedModule.Assembler ?? throw new Exception("Platform does not support assembly language");
            var asmTargetName = $@"{this.Alias}-Asm-Objects";

            writer.WriteLine($@"ObjectList( '{asmTargetName}' ) {{");
            writer.Indent();
            {
                writer.WriteLine(@".CompilerInputAllowNoFiles = true");

                this.WriteCompilerRules(writer, assembler);

                writer.WriteLine($@".CompilerInputPath = '{sourceInputPath}'");
                writer.WriteLine(@".CompilerInputPattern = {");
                writer.Indent();
                {
                    foreach (var pattern in assembler.Descriptor.InputPatterns)
                    {
                        writer.WriteLine($@"'{pattern}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine(@"}");

                writer.WriteLine($@".CompilerOutputPath = '{compilerOutputPath}'");
                writer.WriteLine($@".CompilerOutputExtension = '{compilerOutputExtension}'");
            }
            writer.Unindent();
            writer.WriteLine(@"}");
        }
    }
}
