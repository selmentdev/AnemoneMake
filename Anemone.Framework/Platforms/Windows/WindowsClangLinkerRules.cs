// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Windows;

internal sealed class WindowsClangLinkerRules : LinkerRules
{
    private readonly WindowsPlatformRules m_Platform;

    public WindowsClangLinkerRules(WindowsPlatformRules platform, ResolvedModule module, LinkerDescriptor descriptor)
        : base(module, descriptor)
    {
        this.m_Platform = platform;
        this.Output = GetOutputFileName(this.Module);
    }

    internal static string GetOutputFileName(ResolvedModule module)
    {
        switch (module.LinkKind)
        {
            case ModuleLinkKind.Application:
            case ModuleLinkKind.ConsoleApplication:
                {
                    return $@"{module.Name}.exe";
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    return $@"{module.Name}.dll";
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    return $@"{module.Name}.lib";
                }

            case ModuleLinkKind.ImportedLibrary:
                {
                    return $@"{module.Name}";
                }
        }

        throw new Exception($@"Unsupported module kind: {module.Kind}");
    }

    public override void Link(List<string> args, string output, string input)
    {
        args.Add(@"/nologo");

        #region Debug support
        if (this.Module.EnableDebugInfo || this.Module.EnableAddressSanitizer)
        {
            args.Add(@"/DEBUG:FULL");
        }
        else
        {
            args.Add(@"/DEBUG:NONE");
        }

        if (this.Module.OptimizationLevel == OptimizationLevel.Full)
        {
            // Set binary file checksum.
            args.Add(@"/RELEASE");
        }

        args.Add(@"/PDBALTPATH:%_PDB%");
        #endregion

        #region Link Time Optimizations
        if (this.Module.EnableSymbolsCollection)
        {
            // Remove unreferenced symbols.
            args.Add(@"/OPT:REF");
            // Merge identical symbols.
            args.Add(@"/OPT:ICF=10");
        }
        else
        {
            // Don't remove unreferenced symbols.
            args.Add(@"/OPT:NOREF");
            // Don't merge identical symbols.
            args.Add(@"/OPT:NOICF");
        }

        if (this.Module.EnableLinkTimeCodeGeneration)
        {
            // Enable link-time optimization.
            args.Add(@"/LTCG");
        }
        #endregion

        #region Incremental linking
        if (this.Module is { EnableIncrementalLinking: true, EnableSymbolsCollection: false, EnableLinkTimeCodeGeneration: false })
        {
            args.Add(@"/INCREMENTAL");
            args.Add(@"/verbose:incr");
        }
        else
        {
            args.Add(@"/INCREMENTAL:NO");
        }
        #endregion

        #region Output binary properties
        switch (this.Module.LinkKind)
        {
            case ModuleLinkKind.Application:
                {
                    args.Add(@"/SUBSYSTEM:WINDOWS,10.0");
                    break;
                }

            case ModuleLinkKind.ConsoleApplication:
                {
                    args.Add(@"/SUBSYSTEM:CONSOLE,10.0");
                    break;
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    args.Add(@"/DLL");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    throw new Exception(@"Static library requires librarian instead of linker");
                }

            case ModuleLinkKind.ImportedLibrary:
                {
                    throw new Exception(@"Imported library is already linked.");
                }
        }

        switch (this.Module.Target.Rules.Architecture)
        {
            case TargetArchitecture.X64:
                {
                    args.Add(@"/MACHINE:x64");
                    break;
                }

            case TargetArchitecture.AArch64:
                {
                    args.Add(@"/MACHINE:arm64");
                    break;
                }

            case TargetArchitecture.RiscV64:
                {
                    throw new Exception(@"RISC-V is not supported by Visual Studio.");
                }
        }

        // Don't create side-by-side manifests
        args.Add(@"/MANIFEST:NO");

        // Don't use Fixed Base Address
        args.Add(@"/FIXED:NO");

        // Enable Data Execution Prevention (DEP)
        args.Add("/NXCOMPAT");

        args.Add(@"-nodefaultlib:libcmt");
        #endregion

        #region Libraries
        // Import library paths from SDKs
        foreach (var sdk in this.m_Platform.Sdks)
        {
            foreach (var path in sdk.LibraryPaths)
            {
                args.Add($@"/LIBPATH:""{path}""");
            }
        }

        // Import library paths from module
        foreach (var path in this.Module.LibraryPaths)
        {
            args.Add($@"/LIBPATH:""{path}""");
        }

        foreach (var library in this.Module.Libraries)
        {
            args.Add($@"{library}");
        }

        switch (this.Module.LinkKind)
        {
            case ModuleLinkKind.Application:
            case ModuleLinkKind.ConsoleApplication:
            case ModuleLinkKind.DynamicLibrary:
            case ModuleLinkKind.StaticLibrary:
                {
                    args.Add(@"ntdll.lib");
                    args.Add(@"comctl32.lib");
                    args.Add(@"dbghelp.lib");
                    args.Add(@"iphlpapi.lib");
                    args.Add(@"dwmapi.lib");
                    args.Add(@"xinput.lib");
                    args.Add(@"xaudio2.lib");
                    args.Add(@"gdi32.lib");
                    args.Add(@"Faultrep.lib");
                    args.Add(@"Bcrypt.lib");
                    args.Add(@"Mincore.lib");

                    args.Add ("kernel32.lib");
                    args.Add ("version.lib");
                    args.Add ("ws2_32.lib");
                    args.Add ("mswsock.lib");
                    args.Add ("psapi.lib");
                    args.Add ("shell32.lib");
                    args.Add ("oleaut32.lib");
                    args.Add ("ole32.lib");
                    args.Add ("winmm.lib");
                    args.Add ("user32.lib");
                    args.Add ("advapi32.lib");


                    if (this.Module.Target.Rules.Configuration == TargetConfiguration.Debug)
                    {
                        args.Add ("ucrtd.lib");
                        args.Add ("vcruntimed.lib");
                        args.Add ("msvcrtd.lib");
                    }
                    else
                    {
                        args.Add ("ucrt.lib");
                        args.Add ("vcruntime.lib");
                        args.Add ("msvcrt.lib");
                    }
                    break;
                }

            case ModuleLinkKind.ImportedLibrary:
                {
                    break;
                }
        }
        #endregion

        #region Input and output
        args.Add($@"/OUT:""{output}""");
        args.Add($@"""{input}""");
        #endregion
    }
}
