// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Anemone.Framework.Platforms.Linux;

internal sealed class LinuxGccLinkerRules : LinkerRules
{
    private readonly LinuxPlatformRules m_Platform;

    public LinuxGccLinkerRules(LinuxPlatformRules platform, ResolvedModule module, LinkerDescriptor descriptor)
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
                    return module.Name;
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    return $@"lib{module.Name}.so";
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    return $@"lib{module.Name}.a";
                }

            case ModuleLinkKind.ImportedLibrary:
                {
                    throw new Exception(@"Imported library is already linked.");
                }
        }

        throw new Exception($@"Unsupported module kind: {module.Kind}");
    }

    public override void Link(List<string> args, string output, string input)
    {
        args.Add($@"-o ""{output}""");
        args.Add($@"-Wl,--start-group ""{input}"" -Wl,--end-group");

        foreach (var path in this.Module.LibraryPaths)
        {
            args.Add($@"-L{path}");
        }

        foreach (var item in this.Module.Libraries)
        {
            args.Add($@"-l{item}");
        }

        switch (this.Module.LinkKind)
        {
            case ModuleLinkKind.Application:
            case ModuleLinkKind.ConsoleApplication:
                {
                    args.Add(@"-pie");
                    break;
                }

            case ModuleLinkKind.DynamicLibrary:
                {
                    args.Add(@"-shared");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
                {
                    args.Add(@"-static");
                    break;
                }
        }

        switch (this.Module.LinkKind)
        {
            case ModuleLinkKind.Application:
            case ModuleLinkKind.ConsoleApplication:
            case ModuleLinkKind.DynamicLibrary:
                {
                    args.Add(@"-lpthread");
                    args.Add(@"-lstdc++");
                    args.Add(@"-rdynamic");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
            case ModuleLinkKind.ImportedLibrary:
                {
                    break;
                }
        }

        #region Sanitizers
        // TODO: Verify if these options are valid for GCC
        var enableAddressSanitizer = this.Module.EnableAddressSanitizer && this.m_Platform.EnableAddressSanitizer;
        if (enableAddressSanitizer)
        {
            args.Add(@"-fsanitize=address");
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
        #endregion
    }
}
