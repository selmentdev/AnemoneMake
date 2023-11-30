// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Linux;

internal sealed class LinuxClangCompilerRules : CompilerRules
{
    private readonly LinuxPlatformRules m_Platform;

    public LinuxClangCompilerRules(LinuxPlatformRules platform, ResolvedModule module, CompilerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.m_Platform = platform;
    }

    public override void Compile(List<string> args, string output, string input)
    {
        #region Language support
        if (this.Module.Language == ModuleLanguage.C)
        {
            args.Add(@"-x c");
            args.Add(@"-std=c18");
        }
        else if (this.Module.Language == ModuleLanguage.Cxx)
        {
            args.Add(@"-x c++");
            args.Add(@"-std=c++2b");
        }
        else
        {
            throw new Exception($@"Unsupported language: {this.Module.Language}");
        }
        #endregion

        #region  Input and output
        args.Add($@"-c ""{input}""");
        args.Add($@"-o ""{output}""");
        #endregion

        #region Debug info generation
        if (this.Module.EnableDebugInfo)
        {
            args.Add(@"-g");
        }
        #endregion

        #region SDK support
        foreach (var sdk in this.m_Platform.Sdks)
        {
            foreach (var path in sdk.IncludePaths)
            {
                args.Add($@"-system ""{path}""");
            }
        }
        #endregion

        #region Module includes and defines
        foreach (var path in this.Module.IncludePaths)
        {
            args.Add($@"-I{path}");
        }

        foreach (var item in this.Module.Defines)
        {
            args.Add($@"-D{item}");
        }
        #endregion

        #region Address sanitizer
        var enableAddressSanitizer = this.Module.EnableAddressSanitizer && this.m_Platform.EnableAddressSanitizer;
        if (enableAddressSanitizer)
        {
            args.Add(@"-fsanitize=address");
            args.Add(@"-fsanitize-address-use-after-return=runtime"); //=always
            args.Add(@"-fsanitize-address-use-after-scope");
        }

        var enableThreadSanitizer = this.Module.EnableThreadSanitizer && this.m_Platform.EnableThreadSanitizer;
        if (enableThreadSanitizer)
        {
            args.Add(@"-fsanitize=thread");
        }

        var enableMemorySanitizer = this.Module.EnableMemorySanitizer && this.m_Platform.EnableMemorySanitizer;
        if (enableMemorySanitizer)
        {
            args.Add(@"-fsanitize=memory");
            args.Add(@"-fno-optimize-sibling-calls");
            args.Add(@"-fsanitize-memory-track-origins=2");
        }

        var enableUndefinedBehaviorSanitizer = this.Module.EnableUndefinedBehaviorSanitizer && this.m_Platform.EnableUndefinedBehaviorSanitizer;
        if (enableUndefinedBehaviorSanitizer)
        {
            args.Add(@"-fsanitize=undefined");
            args.Add(@"-fsanitize=float-divide-by-zero");
            args.Add(@"-fsanitize=integer");
            args.Add(@"-fsanitize=nullability");
        }

        var enableDataFlowSanitizer = this.Module.EnableDataFlowSanitizer && this.m_Platform.EnableDataFlowSanitizer;
        if (enableDataFlowSanitizer)
        {
            args.Add(@"-fsanitize=dataflow");
        }

        var enableLeakSanitizer = this.Module.EnableLeakSanitizer && this.m_Platform.EnableLeakSanitizer;
        if (enableLeakSanitizer && !enableAddressSanitizer)
        {
            // Standalone Leak Sanitizer
            args.Add(@"-fsanitize=leak");
        }

        var enableSanitizers = enableAddressSanitizer
            || enableThreadSanitizer
            || enableMemorySanitizer
            || enableUndefinedBehaviorSanitizer
            || enableDataFlowSanitizer
            || enableLeakSanitizer;
        #endregion

        #region Constexpr settings
        // args.Add($@"-fconstexpr-depth=N");
        // args.Add($@"-fconstexpr-steps=N");
        #endregion

        #region Warnings
        if (this.Module.ExternalSource)
        {
            args.Add(@"-Wno-everything");
        }
        else
        {
            // TODO: disable until we provide header disabling selected warnings
            // args.Add($@"-Weverything");
            args.Add(@"-Werror");
        }
        #endregion

        #region Optimizations
        //args.Add($@"-m64");
        args.Add(@"-msse2");
        args.Add(@"-msse3");
        args.Add(@"-mssse3");
        args.Add(@"-msse4.1");

        if (this.Module.EnableAvx || this.Module.EnableAvx2)
        {
            args.Add(@"-mavx");
        }

        if (this.Module.EnableAvx2)
        {
            args.Add(@"-mavx2");
        }


        switch (this.Module.OptimizationLevel)
        {
            case OptimizationLevel.Debug:
                {
                    args.Add(@"-O0");
                    break;
                }

            case OptimizationLevel.Development:
                {
                    args.Add(@"-O1");
                    break;
                }

            case OptimizationLevel.Optimized:
                {
                    args.Add(@"-O2");
                    break;
                }

            case OptimizationLevel.Full:
                {
                    args.Add(@"-O3");
                    break;
                }
        }

        if (this.Module.Target.Rules.Configuration == TargetConfiguration.Debug)
        {
            args.Add(@"-D_DEBUG");
        }
        else
        {
            args.Add(@"-DNDEBUG");
        }

        args.Add("-D_GNU_SOURCE=1");
        args.Add("-D__USE_XOPEN2K8=1");

        var optimize = this.Module.OptimizationLevel is OptimizationLevel.Optimized or OptimizationLevel.Full;

        if (optimize)
        {
            if (enableSanitizers)
            {
                // For better results, sanitizers require frame pointers
                args.Add(@"-fno-omit-frame-pointer");
            }
            else
            {
                args.Add(@"-fomit-frame-pointer");
            }
        }
        else
        {
            args.Add(@"-fno-omit-frame-pointer");
        }

        if (this.Module.EnableLinkTimeCodeGeneration)
        {
            args.Add(@"-flto=full");
            args.Add(@"-fdata-sections");
            args.Add(@"-ffunction-sections");
        }

        if (this.Module.EnableExceptions)
        {
            args.Add(@"-fexceptions");
        }
        else
        {
            args.Add(@"-fno-exceptions");
        }

        if (this.Module.Language == ModuleLanguage.Cxx)
        {
            if (this.Module.EnableRtti)
            {
                args.Add(@"-frtti");
            }
            else
            {
                args.Add(@"-fno-rtti");
            }
        }
        #endregion

        #region Position independent code
        switch (this.Module.LinkKind)
        {
            case ModuleLinkKind.Application:
            case ModuleLinkKind.ConsoleApplication:
                {
                    args.Add(@"-fPIE");
                    break;
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    args.Add(@"-fPIC");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    args.Add(@"-fPIC");
                    break;
                }
        }
        #endregion

        #region Miscellaneous
        args.Add(@"-fno-math-errno");
        #endregion
    }
}
