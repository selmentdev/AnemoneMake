// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework.Platforms.Windows;

#if false
public sealed class WindowsClangPlatformRules : PlatformRules
{
    public WindowsSdkRules WindowsSdk { get; }
    public VisualStudioSdkRules VisualStudioSdk { get; }

    private readonly CompilerDescriptor m_CxxCompiler;
    private readonly CompilerDescriptor m_ClangClCxxCompiler;
    private readonly CompilerDescriptor m_CCompiler;
    private readonly LinkerDescriptor m_Linker;
    private readonly LibrarianDescriptor m_Librarian;

    private readonly List<SdkRules> m_Sdks = new();
    public override IReadOnlyCollection<SdkRules> Sdks => this.m_Sdks;
    private readonly ResourceCompilerDescriptor m_WinResCompiler;

    public WindowsClangPlatformRules(TargetArchitecture architecture, WindowsSdkRules windowsSdkRules, VisualStudioSdkRules visualStudioSdkRules)
        : base(TargetPlatform.Windows, architecture, TargetToolchain.Clang)
    {
        this.WindowsSdk = windowsSdkRules;
        this.VisualStudioSdk = visualStudioSdkRules;

        this.m_Sdks.AddRange(new SdkRules[] {
            this.WindowsSdk,
            this.VisualStudioSdk,
        });

        var llvmRoot = @"C:\Program Files\LLVM";

        this.m_CxxCompiler = new CompilerDescriptor()
        {
            Id = $@"Clang-CXX-Compiler-{architecture}",
            Name = $@"Clang C++ Compiler {architecture}",
            Executable = Path.Combine(llvmRoot, "bin", "clang++.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.cpp", "*.cxx", },
            OutputExtension = ".obj",
            SourceDirectory = "Private",
            Architecture = architecture,
            Platform = TargetPlatform.Windows,
            Toolchain = TargetToolchain.Clang,
        };

        this.m_ClangClCxxCompiler = new CompilerDescriptor()
        {
            Id = $@"Clang-CL-CXX-Compiler-{architecture}",
            Name = $@"Clang CL C++ Compiler {architecture}",
            Executable = Path.Combine(llvmRoot, "bin", "clang-cl.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.cpp", "*.cxx", },
            OutputExtension = ".obj",
            SourceDirectory = "Private",
            Architecture = architecture,
            Platform = TargetPlatform.Windows,
            Toolchain = TargetToolchain.ClangCL,
        };

        this.m_CCompiler = new CompilerDescriptor()
        {
            Id = $@"Clang-CC-Compiler-{architecture}",
            Name = $@"Clang C Compiler {architecture}",
            Executable = Path.Combine(llvmRoot, "bin", "clang.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.c", },
            OutputExtension = ".obj",
            SourceDirectory = "Private",
            Architecture = architecture,
            Platform = TargetPlatform.Windows,
            Toolchain = TargetToolchain.Clang,
        };

        this.m_WinResCompiler = new ResourceCompilerDescriptor()
        {
            Id = $@"Windows-Resource-Compiler-{this.Toolchain}-{architecture}",
            Name = $@"Windows Resource Compiler {this.Toolchain} {architecture}",
            Executable = this.WindowsSdk.ResourceCompiler,
            InputPatterns = new[] { "*.rc", },
            OutputExtension = ".res",
            SourceDirectory = "Resources",
        };

        this.Compilers = new[]
        {
            this.m_CxxCompiler,
            this.m_CCompiler,
            this.m_ClangClCxxCompiler,
        };

        this.ResourceCompilers = new[]
        {
            this.m_WinResCompiler,
        };

        this.m_Linker = new LinkerDescriptor()
        {
            Id = $@"Clang-Linker-{architecture}",
            Name = $@"Clang Linker {architecture}",
            Executable = Path.Combine(llvmRoot, "bin", "clang++.exe"),
        };

        this.Linkers = new[]
        {
            this.m_Linker,
        };

        this.m_Librarian = new LibrarianDescriptor()
        {
            Id = $@"Clang-Librarian-{architecture}",
            Name = $@"Clang Librarian {architecture}",
            Executable = Path.Combine(llvmRoot, "bin", "llvm-ar.exe"),
        };

        this.Librarians = new[]
        {
            this.m_Librarian,
        };

    }

    public override IReadOnlyCollection<CodeGeneratorRules> CreateCodeGenerators(BuildContext context)
    {
        return Array.Empty<CodeGeneratorRules>();
    }

    public override CompilerRules CreateCompiler(BuildContext context)
    {
        switch (context.Module.Language)
        {
            case ModuleLanguage.Cxx:
                return new WindowsClangCLCompilerRules(this, context, this.m_ClangClCxxCompiler);

            case ModuleLanguage.C:
                return new WindowsClangCLCompilerRules(this, context, this.m_ClangClCxxCompiler);

            default:
                throw new NotImplementedException();
        }
    }

    public override LibrarianRules CreateLibrarian(BuildContext context)
    {
        //return new WindowsClangLibrarianRules(this, context, this.m_Librarian);
        return new MsvcLibrarianRules(this, context, this.m_Librarian);
    }

    public override LinkerRules CreateLinker(BuildContext context)
    {
        return new WindowsClangLinkerRules(this, context, this.m_Linker);
    }

    public override IReadOnlyCollection<ResourceCompilerRules> CreateResourceCompilers(BuildContext context)
    {
        return new[]
        {
            new WindowsResourceCompilerRules(this, context, this.m_WinResCompiler),
        };
    }
}
#endif
