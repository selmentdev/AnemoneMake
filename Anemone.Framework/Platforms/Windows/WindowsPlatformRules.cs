// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Framework.Sdks.VisualStudio;
using Anemone.Framework.Sdks.Vulkan;
using Anemone.Framework.Sdks.Windows;
using System;
using System.Collections.Generic;
using System.IO;

namespace Anemone.Framework.Platforms.Windows;

public sealed class WindowsPlatformRules : PlatformRules
{
    public WindowsSdkRules WindowsSdk { get; }
    public VisualStudioSdkRules VisualStudioSdk { get; }

    private readonly CompilerDescriptor m_CxxCompiler;
    private readonly CompilerDescriptor m_CCompiler;
    private readonly CompilerDescriptor m_ClangClCxxCompiler;
    private readonly CompilerDescriptor m_ClangClCCCompiler;
    private readonly CompilerDescriptor m_ClangCxxCompiler;
    private readonly CompilerDescriptor m_ClangCCCompiler;
    private readonly CompilerDescriptor m_Assembler;
    private readonly ResourceCompilerDescriptor m_WinResCompiler;
    private readonly LinkerDescriptor m_Linker;
    private readonly LinkerDescriptor m_LLVMLinker;
    private readonly LinkerDescriptor m_Librarian;
    private readonly LinkerDescriptor m_LLVMLibrarian;

    // TODO: sanitizers should be specified in separate profile section.
    public bool EnableAddressSanitizer { get; }
    public bool EnableThreadSanitizer => false;
    public bool EnableMemorySanitizer => false;
    public bool EnableLeakSanitizer => false;
    public bool EnableDataFlowSanitizer => false;
    public bool EnableUndefinedBehaviorSanitizer => false;
    public bool EnableCodeAnalyze { get; } = false;

#if ENABLE_QT_INTEGRATION
    internal readonly QtSdkRules? m_QtSdk;
#endif

    private readonly VulkanSdkRules? m_VulkanSdk;

    public WindowsPlatformRules(
        PlatformDescriptor descriptor,
        WindowsSdkRules windowsSdkRules,
        VisualStudioSdkRules visualStudioSdkRules)
        : base(descriptor)
    {
        this.EnableAddressSanitizer = this.Architecture == TargetArchitecture.X64;

        this.WindowsSdk = windowsSdkRules;
        this.VisualStudioSdk = visualStudioSdkRules;

        this.m_Sdks.Add(this.WindowsSdk);

        //if (this.Toolchain == TargetToolchain.MSVC)
        {
            // FIXME: Clang and ClangCL goes bonkers
            this.m_Sdks.Add(this.VisualStudioSdk);
        }

        this.m_VulkanSdk = VulkanSdkLocator.Locate(this.Platform, this.Architecture, this.Toolchain);

        if (this.m_VulkanSdk != null)
        {
            this.m_Sdks.Add(this.m_VulkanSdk);
        }

#if ENABLE_QT_INTEGRATION
        this.m_QtSdk = QtSdkLocator.Locate(this.Platform, this.Architecture, this.Toolchain, "6.3.0");

        if (this.m_QtSdk != null)
        {
            this.m_Sdks.Add(this.m_QtSdk);
        }
#endif
        this.m_Assembler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "MSVC_ASM")
        {
            Executable = this.VisualStudioSdk.AssemblerPath,
            AllowDistribution = true,
            InputPatterns = new[] { "*.asm", },
            OutputExtension = ".obj",
            SourceDirectory = "Source",
        };

        this.m_CxxCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "MSVC_CXX")
        {
            Executable = this.VisualStudioSdk.CompilerPath,
            AllowDistribution = true,
            ExecutableExtraFiles = this.VisualStudioSdk.CompilerExtraFiles,
            InputPatterns = new[] { "*.cpp", "*.cxx", "*.cc", },
            OutputExtension = ".obj",
            SourceDirectory = "Source",
        };

        const string llvmRoot = @"C:\Program Files\LLVM";

        this.m_ClangClCxxCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "CLANGCL_CXX")
        {
            Executable = Path.Combine(llvmRoot, "bin", "clang-cl.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.cpp", "*.cxx", "*.cc", },
            OutputExtension = ".obj",
            SourceDirectory = "Source",
        };

        this.m_ClangClCCCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "CLANGCL_CC")
        {
            Executable = Path.Combine(llvmRoot, "bin", "clang-cl.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.c", },
            OutputExtension = ".obj",
            SourceDirectory = "Source",
        };

        this.m_ClangCxxCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "CLANG_CXX")
        {
            Executable = Path.Combine(llvmRoot, "bin", "clang.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.cpp", "*.cxx", "*.cc", },
            OutputExtension = ".obj",
            SourceDirectory = "Source",
        };

        this.m_ClangCCCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "CLANG_CC")
        {
            Executable = Path.Combine(llvmRoot, "bin", "clang.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.c", },
            OutputExtension = ".obj",
            SourceDirectory = "Source",
        };

        this.m_CCompiler = new CompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "MSVC_CC")
        {
            Executable = this.VisualStudioSdk.CompilerPath,
            AllowDistribution = true,
            ExecutableExtraFiles = this.VisualStudioSdk.CompilerExtraFiles,
            InputPatterns = new[] { "*.c", },
            OutputExtension = ".obj",
            SourceDirectory = "Source",
        };

        this.m_WinResCompiler = new ResourceCompilerDescriptor(this.Platform, this.Architecture, this.Toolchain, "WINSDK_RC")
        {
            Executable = this.WindowsSdk.ResourceCompiler,
            InputPatterns = new[] { "*.rc", },
            OutputExtension = ".res",
            SourceDirectory = "Resources",
        };


        var compilers = new List<CompilerDescriptor>
        {
            this.m_CxxCompiler,
            this.m_CCompiler,
        };

        if (this.Architecture == TargetArchitecture.X64)
        {
            compilers.Add(this.m_ClangClCxxCompiler);
            compilers.Add(this.m_ClangClCCCompiler);
            compilers.Add(this.m_ClangCxxCompiler);
            compilers.Add(this.m_ClangCCCompiler);
        }

        this.Compilers = compilers.ToArray();

        this.Assemblers = new[]
        {
            this.m_Assembler,
        };

        this.ResourceCompilers = new[]
        {
            this.m_WinResCompiler,
        };

#if ENABLE_QT_INTEGRATION
        if (this.m_QtSdk != null)
        {
            this.CodeGenerators = this.m_QtSdk.CodeGenerators;
        }
#endif

        this.m_Linker = new LinkerDescriptor(this.Platform, this.Architecture, this.Toolchain, "MSVC_LINK")
        {
            Executable = this.VisualStudioSdk.LinkerPath,
        };

        this.m_Librarian = new LinkerDescriptor(this.Platform, this.Architecture, this.Toolchain, "MSVC_LIB")
        {
            Executable = this.VisualStudioSdk.ArchiverPath,
        };

        this.m_LLVMLinker = new LinkerDescriptor(this.Platform, this.Architecture, this.Toolchain, "LLVM_LINK")
        {
            Executable = Path.Combine(llvmRoot, "bin", "lld-link.exe"),
        };

        this.m_LLVMLibrarian = new LinkerDescriptor(this.Platform, this.Architecture, this.Toolchain, "LLVM_LIB")
        {
            Executable = Path.Combine(llvmRoot, "bin", "llvm-ar.exe"),
        };

        this.Linkers = new[]
        {
            this.m_Linker,
            this.m_Librarian,
        };
    }

    public override IReadOnlyCollection<CodeGeneratorRules> CreateCodeGenerators(ResolvedModule module)
    {
#if ENABLE_QT_INTEGRATION
        if (this.m_QtSdk != null)
        {
            return this.m_QtSdk.CreateGenerators(module);
        }
#endif
        return Array.Empty<CodeGeneratorRules>();
    }

    public override CompilerRules CreateCompiler(ResolvedModule module)
    {
        switch (module.Language)
        {
            case ModuleLanguage.Cxx:
                {
                    if (this.Toolchain == TargetToolchain.MSVC)
                    {
                        return new MsvcCompilerRules(this, module, this.m_CxxCompiler);
                    }
                    else if (this.Toolchain == TargetToolchain.ClangCL)
                    {
                        return new WindowsClangCLCompilerRules(this, module, this.m_ClangClCxxCompiler);
                    }
                    else if (this.Toolchain == TargetToolchain.Clang)
                    {
                        return new WindowsClangCompilerRules(this, module, this.m_ClangCxxCompiler);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

            case ModuleLanguage.C:
                {
                    if (this.Toolchain == TargetToolchain.MSVC)
                    {
                        return new MsvcCompilerRules(this, module, this.m_CCompiler);
                    }
                    else if (this.Toolchain == TargetToolchain.ClangCL)
                    {
                        return new WindowsClangCLCompilerRules(this, module, this.m_ClangClCCCompiler);
                    }
                    else if (this.Toolchain == TargetToolchain.Clang)
                    {
                        return new WindowsClangCompilerRules(this, module, this.m_ClangCCCompiler);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

            default:
                {
                    throw new NotImplementedException();
                }
        }
    }

    public override CompilerRules? CreateAssembler(ResolvedModule module)
    {
        if (module.EnableAssembly)
        {
            return new MsvcAssemblerRules(this, module, this.m_Assembler);
        }

        return null;
    }

    public override LinkerRules CreateLinker(ResolvedModule module)
    {
        if (this.Toolchain is TargetToolchain.Clang or TargetToolchain.ClangCL)
        {
            if (module.LinkKind == ModuleLinkKind.StaticLibrary)
            {
                return new WindowsClangLibrarianRules(this, module, this.m_LLVMLibrarian);
            }

            return new WindowsClangLinkerRules(this, module, this.m_LLVMLinker);
        }
        else if (this.Toolchain == TargetToolchain.MSVC)
        {
            if (module.LinkKind == ModuleLinkKind.StaticLibrary)
            {
                return new MsvcLibrarianRules(this, module, this.m_Librarian);
            }

            return new MsvcLinkerRules(this, module, this.m_Linker);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public override IReadOnlyCollection<ResourceCompilerRules> CreateResourceCompilers(ResolvedModule module)
    {
        return new[]
        {
            new WindowsResourceCompilerRules(this, module, this.m_WinResCompiler),
        };
    }
}
