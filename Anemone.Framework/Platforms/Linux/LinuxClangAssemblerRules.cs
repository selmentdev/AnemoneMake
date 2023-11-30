// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Linux;

internal sealed class LinuxClangAssemblerRules : CompilerRules
{
    private readonly LinuxPlatformRules m_Platform;

    public LinuxClangAssemblerRules(LinuxPlatformRules platform, ResolvedModule module, CompilerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.m_Platform = platform;
    }

    public override void Compile(List<string> args, string output, string input)
    {
        #region  Input and output
        args.Add($@"-c ""{input}""");
        args.Add($@"-o ""{output}""");
        #endregion

        #region Debug info generation
        if (this.Module.EnableDebugInfo)
        {
            args.Add(@"-ggdb");
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

        if (this.Module.EnableLinkTimeCodeGeneration)
        {
            args.Add(@"-fdata-sections");
            args.Add(@"-ffunction-sections");
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
                    args.Add(@"-shared");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    args.Add(@"-fPIC");
                    args.Add(@"-static");
                    break;
                }
        }
        #endregion
    }
}
