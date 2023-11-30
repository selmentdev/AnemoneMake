// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;

namespace Anemone.Framework.Platforms.Windows;

internal sealed class WindowsClangCLCompilerRules : CompilerRules
{
    private readonly WindowsPlatformRules m_Platform;

    public WindowsClangCLCompilerRules(WindowsPlatformRules platform, ResolvedModule module, CompilerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.m_Platform = platform;
    }

    public override void Compile(List<string> args, string output, string input)
    {
        #region Inputs and output
        //
        // Just ignore compiler banner.
        //

        args.Add(@"/nologo");


        //
        // Handle inputs and outputs.
        //

        args.Add($@"/c ""{input}""");
        args.Add($@"/Fo""{output}""");
        #endregion

        #region Math
        //args.Add($@"/fp:contract"); // Consider floating-point contractions when generating code.
        //args.Add($@"/fp:except");   // Consider floating-point exceptions when generating code.
        args.Add(@"/fp:fast");     // "fast" floating-point model; results are less predictable.
        //args.Add($@"/fp:precise");  // "precise" floating-point model; results are predictable.
        //args.Add($@"/fp:strict");   // "strict" floating-point model (implies `/fp:except`).
        //args.Add($@"/fpcvt:BC");    // Backward-compatible floating-point to unsigned integer conversions.
        //args.Add($@"/fpcvt:IA");    // Intel native floating-point to unsigned integer conversion behavior.
        #endregion

        #region Debug info generation
        if (this.Module.EnableDebugInfo)
        {
            var createPdbFiles = true;

            if (createPdbFiles || this.Module.EnableEditAndContinue)
            {
                if (this.Module.EnableEditAndContinue)
                {
                    // Includes debug information in a program database compatible with Edit and Continue.
                    args.Add(@"/ZI");
                }
                else
                {
                    // Generates complete debugging information.
                    args.Add(@"/Zi");
                }

                //if (this.Module.EnableIncrementalLinking)
                {
                    // Synchronize access to PDB files.
                    args.Add(@"/FS");
                }

                var pdbFileLocation = Path.Combine(this.Module.Rules.ObjectFilesDirectory, $@"{this.Module.Name}.pdb");

                args.Add($@"/Fd""{pdbFileLocation}""");

                // Improves PDB generation time in parallel builds.
                //args.Add($@"/Zf");
            }
            else
            {
                args.Add(@"/Z7");
            }
        }
        #endregion

        #region Miscellaneous
        // Use caret `^` to indicate error location.
        args.Add(@"/diagnostics:caret");

        // Full paths in code diagnostics.
        args.Add(@"/FC");

        // Use big objects.
        args.Add(@"/bigobj");

        // Disable Just-My-Code. We need performance.
        //args.Add($@"/JMC-");

        // Enable multiprocess compilation
        args.Add(@"/MP");

        // All files are UTF-8 encoded.
        args.Add(@"/utf-8");

        // Enable fast-fail mode
        //args.Add($@"/fastfail");

        //if (this.Module.EnableCodeAnalyze)
        //{
        //    args.Add($@"/analyze");
        //}
        #endregion

        #region Precompiled headers
        // TODO: Implement this one.
        // `/Zm900` -- precompiled headers memory limit.
        #endregion

        #region Modules support
        #endregion

        #region SDK support
        // Ignore warnings in external headers
        //args.Add($@"-Wno-nonportable-include-path");
        //args.Add($@"/external:W0");

        // Add paths to system SDKs
        foreach (var sdk in this.m_Platform.Sdks)
        {
            foreach (var path in sdk.IncludePaths)
            {
                args.Add($@"/external:I""{path}""");
            }
        }

        // Target to Windows 10 SDK.
        args.Add(@"/D_WIN32_WINNT=0x0A00");

        // WinAPI desktop family partition.
        args.Add(@"/DWINAPI_FAMILY=WINAPI_FAMILY_DESKTOP_APP");

        // Toolchain SDK defines.
        args.Add(@"/D__STDC_WANT_LIB_EXT1__=1");
        args.Add(@"/D__STDINT_MACROS");
        args.Add(@"/D__STDINT_LIMITS");
        args.Add(@"/D__STDC_CONSTANT_MACROS");
        args.Add(@"/D__STDC_FORMAT_MACROS");
        args.Add(@"/D__STDC_LIMIT_MACROS");

        // TODO:
        //
        // These should be defined in shipping / testing configurations

        switch (this.Module.OptimizationLevel)
        {
            case OptimizationLevel.Debug:
            case OptimizationLevel.Development:
                {
                    break;
                }

            case OptimizationLevel.Optimized:
            case OptimizationLevel.Full:
                {
                    args.Add(@"/D_ITERATOR_DEBUG_LEVEL=0");
                    args.Add(@"/D_CRT_SECURE_INVALID_PARAMETER=");
                    break;
                }
        }
        #endregion

        #region Address Sanitizer
        if (this.Module.EnableAddressSanitizer && this.m_Platform.EnableAddressSanitizer)
        {
            args.Add(@"/fsanitize=address");
        }
        #endregion

        #region Module includes and defines
        // Includes
        foreach (var path in this.Module.IncludePaths)
        {
            args.Add($@"/I""{path}""");
        }

        // Defines.
        foreach (var item in this.Module.Defines)
        {
            args.Add($@"/D""{item}""");
        }
        #endregion

        #region Language support
        if (this.Module.Language == ModuleLanguage.C)
        {
            // Compile as C
            args.Add(@"/TC");

            // Latest supported C standard.
            args.Add(@"/std:c17");
        }
        else if (this.Module.Language == ModuleLanguage.Cxx)
        {
            // Compile as C++
            args.Add(@"/TP");

            // Enable latest C++ standard.
            args.Add(@"/std:c++latest");

            // Enable the `__cplusplus` macro to report the supported standard
            args.Add(@"/Zc:__cplusplus");

            // Enable new lambda processor for conformance-mode syntactic checks in generic lambdas.
            //args.Add($@"/Zc:lambda");

            // A UDT temporary won't bind to a non-const lvalue reference
            //args.Add($@"/Zc:referenceBinding");

            // Enforce Standard C++ explicit type conversion rules
            args.Add(@"/Zc:rvalueCast");

            // ClangCL does not provide this define by default.
            args.Add(@"-D""__cpp_consteval""");
        }

        // Remove unreferenced functions or data if they're COMDAT or have internal linkage only
        args.Add(@"/Zc:inline");

        // Disable string-literal to `char*` or `wchar_t*` conversion
        args.Add(@"/Zc:strictStrings");

        // Enforce conditional operator rules on operand types.
        args.Add(@"/Zc:ternary");

        // Use the new conforming preprocessor.
        //args.Add($@"/Zc:preprocessor");

        // Permissive mode.
        args.Add(@"/permissive-");

        // Enable standard C++ 20 coroutines support.
        //args.Add($@"/await:strict");
        #endregion

        #region Constexpr settings
        // `/constexpr:backtrace<N>` Show N `constexpr` evaluations in diagnostics.
        // `/constexpr:depth<N>` Recursion depth limit for `constexpr` evaluation.
        // `/constexpr:steps<N>` Terminate `constexpr` evaluation after N steps.
        #endregion

        #region Unicode support
        // Use Unicode APIs by default.
        args.Add(@"/D_UNICODE=1");
        args.Add(@"/DUNICODE=1");
        #endregion

        #region Warnings
        if (this.Module.ExternalSource)
        {
            // This is external module, so we can skip warnings reported by compiler.
            args.Add(@"/w");
        }
        else
        {
            // Enable some warnings.
            args.Add(@"/W4");

            // Treat warnings as errors.
            args.Add(@"/WX");
        }
        #endregion

        #region Module specific defines
        switch (this.Module.LinkKind)
        {
            case ModuleLinkKind.Application:
                {
                    args.Add(@"/D_WINDOWS");
                    break;
                }

            case ModuleLinkKind.ConsoleApplication:
                {
                    args.Add(@"/D_CONSOLE");
                    break;
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    args.Add($@"/D""{this.Module.ExportDefine}=1""");
                    args.Add(@"/D_WINDLL");
                    args.Add(@"/D_USRDLL");
                    args.Add(@"/D_WINDOWS");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    args.Add(@"/D_LIB");
                    break;
                }

            case ModuleLinkKind.ImportedLibrary:
                {
                    throw new Exception("Imported library is not built by this tool.");
                }
        }
        #endregion

        #region Runtime support
        switch (this.Module.Target.Rules.Configuration)
        {
            case TargetConfiguration.Debug:
                {
                    args.Add(@"/D_DEBUG");

                    if (this.Module.EnableStaticRuntime)
                    {
                        args.Add(@"/MTd");
                    }
                    else
                    {
                        args.Add(@"/MDd");
                    }
                    break;
                }

            case TargetConfiguration.GameDebug:
            case TargetConfiguration.EngineDebug:
            case TargetConfiguration.Development:
            case TargetConfiguration.Testing:
            case TargetConfiguration.Shipping:
                {
                    args.Add(@"/DNDEBUG");

                    if (this.Module.EnableStaticRuntime)
                    {
                        args.Add(@"/MT");
                    }
                    else
                    {
                        args.Add(@"/MD");
                    }
                    break;
                }
        }
        #endregion

        #region Optimizations
        switch (this.m_Platform.Platform)
        {
            case TargetPlatform.Windows:
            case TargetPlatform.UniversalWindows:
            case TargetPlatform.GamingDesktop:
                {
                    if (this.Module.EnableAvx)
                    {
                        args.Add(@"/arch:AVX");
                    }
                    else if (this.Module.EnableAvx2)
                    {
                        args.Add(@"/arch:AVX2");
                    }

                    break;
                }

            case TargetPlatform.XboxGaming:
                {
                    args.Add(@"/arch:AVX2");
                    break;
                }

            default:
                {
                    throw new Exception($@"{this.m_Platform.Platform} is not supported.");
                }
        }

        // Acquire/release semantics not guaranteed on volatile accesses.
        // Bad news for everyone relying that volatile accesses are atomic.
        args.Add(@"/volatile:iso");

        // Exception handling
        args.Add(@"/EHsc");

        switch (this.Module.OptimizationLevel)
        {
            case OptimizationLevel.Debug:
                {
                    // Disable optimizations.
                    args.Add(@"/Od");
                    // Enable fast runtime checks.
                    args.Add(@"/RTC1");
                    break;
                }

            case OptimizationLevel.Development:
                {
                    // Create small code.
                    args.Add(@"/O1");
                    // Allow inlining of functions marked as inline.
                    args.Add(@"/Ob1");
                    // Enable intrinsic functions.
                    args.Add(@"/Oi");
                    break;
                }

            case OptimizationLevel.Optimized:
                {
                    // Enable whole program optimization.
                    //args.Add(@"-flto");
                    // FIXME: this requires to use llvm-link
                    // Create fast code.
                    args.Add(@"/O2");
                    // Allow inlining of any suitable functions.
                    args.Add(@"/Ob2");
                    // Enable intrinsic functions.
                    args.Add(@"/Oi");
                    break;
                }

            case OptimizationLevel.Full:
                {
                    // Enable whole program optimization.
                    //args.Add(@"-flto");
                    // FIXME: this requires to use llvm-link
                    // Optimizes for Windows applications.
                    args.Add(@"/GA");
                    // Create fast code.
                    args.Add(@"/O2");
                    // Omit frame pointers
                    args.Add(@"/Oy");
                    // Even more aggresive inlining.
                    args.Add(@"/Ob2");
                    // Enable intrinsic functions.
                    args.Add(@"/Oi");
                    break;
                }
        }

        // TODO:
        //
        // consider using `/homeparams`.

        switch (this.Module.OptimizationLevel)
        {
            case OptimizationLevel.Debug:
            case OptimizationLevel.Development:
            case OptimizationLevel.Optimized:
                {
                    // Enable security checks.
                    args.Add(@"/sdl");
                    // Adds control flow guard security checks.
                    //args.Add(@"/guard:cf");
                    break;
                }

            case OptimizationLevel.Full:
                {
                    // Disable security checks
                    args.Add(@"/sdl-");
                    // Disable control flow guard security checks.
                    //args.Add(@"/guard:cf-");
                    // Enables whole-program global data optimization.
                    args.Add(@"/Gw");
                    // Enables function-level linking.
                    args.Add(@"/Gy");
                    break;
                }
        }
        #endregion

        #region Common properties
        // Eliminate duplicated strings.
        args.Add(@"/GF");

        // Enable/Disable RTTI support.
        if (this.Module.EnableRtti)
        {
            args.Add(@"/GR");
        }
        else
        {
            args.Add(@"/GR-");
        }
        #endregion

        #region ClangCL specific options
        args.Add(@"-Wno-#pragma-messages");
        #endregion
    }
}
