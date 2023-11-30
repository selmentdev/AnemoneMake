// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.Profiling;
using System;
using System.IO;

namespace Anemone.Framework.Generators.Fastbuild;

internal sealed class FastbuildTargetPlatform
{
    public TargetContext Context { get; }

    public FastbuildTargetPlatform(TargetContext context)
    {
        this.Context = context;
    }

    public void WriteRules()
    {
        using var scope = Profiler.Profile($@"Platform: {this.Context.Target.Name}");

        Directory.CreateDirectory(this.Context.RulesDirectory);

        this.WritePlatformBff();
        this.WriteTargetBff();
    }

    private void WritePlatformBff()
    {
        var outputPath = Path.Combine(this.Context.RulesDirectory, "Platform.bff");

        using (var writer = new FastbuildWriter(outputPath))
        {
            writer.WriteLine($@"// Platform: {this.Context.Platform.Moniker}");

            foreach (var descriptor in this.Context.Platform.Compilers)
            {
                WriteCompilerDescriptor(writer, descriptor);
            }

            foreach (var descriptor in this.Context.Platform.Assemblers)
            {
                WriteAssemblerDescriptor(writer, descriptor);
            }

            foreach (var descriptor in this.Context.Platform.ResourceCompilers)
            {
                WriteResourceCompilerDescriptor(writer, descriptor);
            }

            foreach (var descriptor in this.Context.Platform.CodeGenerators)
            {
                WriteCodeGeneratorDescriptor(writer, descriptor);
            }
        }

        void WriteCompilerDescriptor(FastbuildWriter writer, CompilerDescriptor descriptor)
        {
            writer.WriteLine(@"// Compiler");
            writer.WriteLine($@"Compiler( '{descriptor.Id}' ) {{");
            writer.Indent();
            {
                writer.WriteLine($@".Executable = '{descriptor.Executable}'");

                if (descriptor.ExecutableExtraFiles != null)
                {
                    writer.WriteLine(@".ExtraFiles = {");
                    writer.Indent();
                    {
                        foreach (var path in descriptor.ExecutableExtraFiles)
                        {
                            writer.WriteLine($@"'{path}'");
                        }
                    }
                    writer.Unindent();
                    writer.WriteLine("}");
                }
            }
            writer.Unindent();
            writer.WriteLine("}");
        }

        void WriteResourceCompilerDescriptor(FastbuildWriter writer, ResourceCompilerDescriptor descriptor)
        {
            writer.WriteLine(@"// Resource compiler");
            writer.WriteLine($@"Compiler( '{descriptor.Id}' ) {{");
            writer.Indent();
            {
                writer.WriteLine($@".Executable = '{descriptor.Executable}'");
                writer.WriteLine(@".CompilerFamily = 'custom'");
            }
            writer.Unindent();
            writer.WriteLine("}");
        }

        void WriteAssemblerDescriptor(FastbuildWriter writer, CompilerDescriptor descriptor)
        {
            writer.WriteLine(@"// Assembler");
            writer.WriteLine($@"Compiler( '{descriptor.Id}' ) {{");
            writer.Indent();
            {
                writer.WriteLine($@".Executable = '{descriptor.Executable}'");
                writer.WriteLine(@".CompilerFamily = 'custom'");
            }
            writer.Unindent();
            writer.WriteLine("}");
        }

        void WriteCodeGeneratorDescriptor(FastbuildWriter writer, CodeGeneratorDescriptor descriptor)
        {
            writer.WriteLine(@"// Code Generator");
            writer.WriteLine($@"Compiler( '{descriptor.Id}' ) {{");
            writer.Indent();
            {
                writer.WriteLine($@".Executable = '{descriptor.Executable}'");
                writer.WriteLine(@".CompilerFamily = 'custom'");
            }
            writer.Unindent();
            writer.WriteLine("}");
        }
    }

    private void WriteTargetBff()
    {
        var outputPath = Path.Combine(this.Context.RulesDirectory, "Target.bff");

        using (var writer = new FastbuildWriter(outputPath))
        {
            writer.WriteLine(@"#include ""Platform.bff""");

            foreach (var configuration in TargetContext.Configurations)
            {
                writer.WriteLine($@"#include ""Target-{configuration}.bff""");
            }

            if (OperatingSystem.IsWindows())
            {
                writer.WriteLine(@"#include ""Solution.bff""");
            }

            writer.WriteLine(@"Alias( 'Target' ) {");
            writer.Indent();
            {
                writer.WriteLine(".Targets = {");
                writer.Indent();
                {
                    foreach (var configuration in TargetContext.Configurations)
                    {
                        writer.WriteLine($@"'Target-{configuration}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine("}");
            }
            writer.Unindent();
            writer.WriteLine("}");

            // FIXME: This will fail when project contains no unit tests
            writer.WriteLine(@"Alias( 'Tests' ) {");
            writer.Indent();
            {
                writer.WriteLine(".Targets = {");
                writer.Indent();
                {
                    foreach (var configuration in TargetContext.Configurations)
                    {
                        writer.WriteLine($@"'Tests-{configuration}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine("}");
            }
            writer.Unindent();
            writer.WriteLine("}");
        }
    }
}
