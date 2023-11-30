// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Linux;

internal sealed class LinuxPlatformRules : PlatformRules
{
    private readonly CompilerDescriptor m_CxxCompiler;
    private readonly CompilerDescriptor m_CCompiler;
    private readonly CompilerDescriptor m_AsmCompiler;
    private readonly LinkerDescriptor m_Linker;
    private readonly LinkerDescriptor m_Librarian;

    // TODO: These values should be loaded from profile section
    public bool EnableAddressSanitizer { get; } = false;

    public bool EnableThreadSanitizer { get; } = false;

    public bool EnableMemorySanitizer { get; } = false;

    public bool EnableUndefinedBehaviorSanitizer { get; } = false;

    public bool EnableDataFlowSanitizer { get; } = false;

    public bool EnableLeakSanitizer { get; } = false;

    private struct ToolchainDescriptor {
        public required string CxxMoniker { get; init; }
        public required string CxxLocation { get; init; }
        public required string CMoniker { get; init; }
        public required string CLocation { get; init; }
        public readonly string AsmMoniker { get; init; }
        public readonly string AsmLocation { get; init; }
        public required string LibrarianMoniker { get; init; }
        public required string LibrarianLocation { get; init; }
        public required string LinkerMoniker { get; init; }
        public required string LinkerLocation { get; init; }
    }

    private readonly IReadOnlyDictionary<TargetToolchain, ToolchainDescriptor> m_Toolchains = new Dictionary<TargetToolchain, ToolchainDescriptor>(){
        [TargetToolchain.Clang] = new ToolchainDescriptor(){
            CxxMoniker = "LLVM_CXX",
            CxxLocation = @"/usr/bin/clang++",
            CMoniker = "LLVM_CC",
            CLocation = @"/usr/bin/clang",
            AsmMoniker = "LLVM_ASM",
            AsmLocation = @"/usr/bin/clang",
            LinkerMoniker = "LLVM_LD",
            LinkerLocation = @"/usr/bin/clang++",
            LibrarianMoniker = "LLVM_AR",
            LibrarianLocation = @"/usr/bin/llvm-ar",
        },
        [TargetToolchain.GCC] = new ToolchainDescriptor(){
            CxxMoniker = "GCC_CXX",
            CxxLocation = @"/usr/bin/g++",
            CMoniker = "GCC_CC",
            CLocation = @"/usr/bin/gcc",
            AsmMoniker = "GCC_ASM",
            AsmLocation = @"/usr/bin/gcc",
            LinkerMoniker = "GCC_LD",
            LinkerLocation = @"/usr/bin/g++",
            LibrarianMoniker = "GCC_AR",
            LibrarianLocation = @"/usr/bin/ar",
        },
    };

    public LinuxPlatformRules(PlatformDescriptor descriptor)
        : base(descriptor)
    {
        var selectedToolchain = this.m_Toolchains[this.Toolchain];

        this.m_CxxCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, selectedToolchain.CxxMoniker)
        {
            Executable = selectedToolchain.CxxLocation,
            AllowDistribution = true,
            InputPatterns = new[] { "*.cxx", "*.cpp", "*.cc", },
            OutputExtension = ".o",
            SourceDirectory = "Source",
        };

        this.m_CCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, selectedToolchain.CMoniker)
        {
            Executable = selectedToolchain.CLocation,
            AllowDistribution = true,
            InputPatterns = new[] { "*.c" },
            OutputExtension = ".o",
            SourceDirectory = "Source",
        };

        this.m_AsmCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, selectedToolchain.AsmMoniker)
        {
            Executable = selectedToolchain.AsmLocation,
            AllowDistribution = true,
            InputPatterns = new[] { "*.S" },
            OutputExtension = ".o",
            SourceDirectory = "Source",
        };

        this.m_Linker = new LinkerDescriptor(this.Platform, this.Architecture, this.Toolchain, selectedToolchain.LinkerMoniker)
        {
            Executable = selectedToolchain.LinkerLocation,
        };

        this.m_Librarian = new LinkerDescriptor(this.Platform, this.Architecture, this.Toolchain, selectedToolchain.LibrarianMoniker)
        {
            Executable = selectedToolchain.LibrarianLocation,
        };

        this.Compilers = new[]
        {
            this.m_CxxCompiler,
            this.m_CCompiler,
        };

        this.Assemblers = new []
        {
            this.m_AsmCompiler
        };

        this.Linkers = new[]
        {
            this.m_Linker,
            this.m_Librarian,
        };
    }

    public override IReadOnlyCollection<CodeGeneratorRules> CreateCodeGenerators(ResolvedModule module)
    {
        return Array.Empty<CodeGeneratorRules>();
    }

    public override CompilerRules CreateCompiler(ResolvedModule module)
    {
        switch (module.Language)
        {
            case ModuleLanguage.Cxx:
                {
                    switch (this.Toolchain)
                    {
                    case TargetToolchain.Clang:
                        {
                            return new LinuxClangCompilerRules(this, module, this.m_CxxCompiler);
                        }
                    case TargetToolchain.GCC:
                        {
                            return new LinuxGccCompilerRules(this, module, this.m_CxxCompiler);
                        }
                    default:
                        {
                            break;
                        }
                    }

                    break;
                }

            case ModuleLanguage.C:
                {
                    switch (this.Toolchain)
                    {
                    case TargetToolchain.Clang:
                        {
                            return new LinuxClangCompilerRules(this, module, this.m_CCompiler);
                        }
                    case TargetToolchain.GCC:
                        {
                            return new LinuxGccCompilerRules(this, module, this.m_CCompiler);
                        }
                    default:
                        {
                            break;
                        }
                    }

                    break;
                }

            default:
                {
                    break;
                }
        }

        throw new NotSupportedException();
    }

    public override CompilerRules? CreateAssembler(ResolvedModule module)
    {
        if (module.EnableAssembly)
        {
            switch (this.Toolchain)
            {
                case TargetToolchain.Clang:
                    {
                        return new LinuxClangAssemblerRules(this, module, this.m_AsmCompiler);
                    }

                case TargetToolchain.GCC:
                    {
                        return new LinuxGccAssemblerRules(this, module, this.m_AsmCompiler);
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        return null;
    }

    public override LinkerRules CreateLinker(ResolvedModule module)
    {
        switch (this.Toolchain)
        {
        case TargetToolchain.Clang:
            {
                if (module.LinkKind == ModuleLinkKind.StaticLibrary)
                {
                    return new LinuxClangLibrarianRules(this, module, this.m_Librarian);
                }

                return new LinuxClangLinkerRules(this, module, this.m_Linker);
            }

        case TargetToolchain.GCC:
            {
                if (module.LinkKind == ModuleLinkKind.StaticLibrary)
                {
                    return new LinuxGccLibrarianRules(this, module, this.m_Librarian);
                }

                return new LinuxGccLinkerRules(this, module, this.m_Linker);
            }

        default:
            {
                break;
            }
        }

        throw new NotSupportedException();
    }

    public override IReadOnlyCollection<ResourceCompilerRules> CreateResourceCompilers(ResolvedModule module)
    {
        return Array.Empty<ResourceCompilerRules>();
    }
}
