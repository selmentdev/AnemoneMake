// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;

namespace Anemone.Framework.Sdks.VisualStudio;

public sealed class VisualStudioSdkRules : SdkRules
{
    private readonly VisualStudioInstance m_Instance;
    public string CompilerPath { get; }
    public string AssemblerPath { get; }
    public string LinkerPath { get; }
    public string ArchiverPath { get; }
    public string Id { get; }
    private readonly FileInfo m_ReleaseAddressSanitizer;
    private readonly FileInfo m_DebugAddressSanitizer;
    private readonly FileInfo m_LlvmSymbolizer;

#if ENABLE_COPY_MSDIA
    private readonly FileInfo m_MsdiaDll;
#endif

    public IEnumerable<string> CompilerExtraFiles { get; }

    internal VisualStudioSdkRules(
        string id,
        VisualStudioInstance instance,
        TargetArchitecture architecture,
        string host)
    {
        this.m_Instance = instance;
        this.Id = id;

        var targetPrefix = GetTargetPrefix(architecture);
        var msdiaPrefix = GetMsdiaPrefix(architecture);

        var toolsRoot = Path.Combine(this.m_Instance.Root, "VC", "Tools", "MSVC", this.m_Instance.ToolsetVersion.ToString());

        this.IncludePaths = new[]
        {
            Path.Combine(toolsRoot, "atlmfc", "include"),
            Path.Combine(toolsRoot, "include"),
            Path.Combine(toolsRoot, "DIA SDK", "include"),
        };

        this.LibraryPaths = new[]
        {
            Path.Combine(toolsRoot, "atlmfc", "lib", targetPrefix),
            Path.Combine(toolsRoot, "lib", targetPrefix),
            Path.Combine(toolsRoot, "DIA SDK", "lib", msdiaPrefix),
        };

        var binariesPath = Path.Combine(toolsRoot, "bin", host, targetPrefix);

        this.AssemblerPath = Path.Combine(binariesPath, "ml64.exe");
        this.CompilerPath = Path.Combine(binariesPath, "cl.exe");
        this.LinkerPath = Path.Combine(binariesPath, "link.exe");
        this.ArchiverPath = Path.Combine(binariesPath, "lib.exe");

        this.CompilerExtraFiles = new[]
        {
            $@"{binariesPath}\1033\clui.dll",
            $@"{binariesPath}\1033\mspft140ui.dll",
            $@"{binariesPath}\atlprov.dll",
            $@"{binariesPath}\c1.dll",
            $@"{binariesPath}\c1xx.dll",
            $@"{binariesPath}\c2.dll",
            $@"{binariesPath}\msobj140.dll",
            $@"{binariesPath}\mspdb140.dll",
            $@"{binariesPath}\mspdbcore.dll",
            $@"{binariesPath}\mspdbsrv.exe",
            $@"{binariesPath}\mspft140.dll",
            $@"{binariesPath}\msvcp140.dll",
            $@"{binariesPath}\tbbmalloc.dll",
            $@"{binariesPath}\vcruntime140.dll",
        };

        this.m_ReleaseAddressSanitizer = new FileInfo(Path.Combine(binariesPath, "clang_rt.asan_dynamic-x86_64.dll"));
        this.m_DebugAddressSanitizer = new FileInfo(Path.Combine(binariesPath, "clang_rt.asan_dbg_dynamic-x86_64.dll"));
        this.m_LlvmSymbolizer = new FileInfo(Path.Combine(binariesPath, "llvm-symbolizer.exe"));
#if ENABLE_COPY_MSDIA
        this.m_MsdiaDll = new FileInfo(Path.Combine(this.m_Instance.Root, "DIA SDK", "bin", msdiaPrefix, "msdia140.dll"));
#endif
    }

    private static string GetTargetPrefix(TargetArchitecture architecture)
    {
        return architecture switch
        {
            TargetArchitecture.X64 => "x64",
            TargetArchitecture.AArch64 => "arm64",
            _ => throw new NotSupportedException("Unsupported architecture"),
        };
    }

    private static string GetMsdiaPrefix(TargetArchitecture architecture)
    {
        return architecture switch
        {
            TargetArchitecture.X64 => "amd64",
            TargetArchitecture.AArch64 => "arm64",
            _ => throw new NotSupportedException("Unsupported architecture"),
        };
    }

    public override IEnumerable<string> GetDependencyFiles(ResolvedTarget target)
    {
        if (target.Rules.EnableAddressSanitizer)
        {
            if (this.m_ReleaseAddressSanitizer.Exists && this.m_LlvmSymbolizer.Exists && this.m_DebugAddressSanitizer.Exists)
            {
                yield return this.m_ReleaseAddressSanitizer.FullName;
                yield return this.m_DebugAddressSanitizer.FullName;
                yield return this.m_LlvmSymbolizer.FullName;
            }
        }

#if ENABLE_COPY_MSDIA
        if (this.m_MsdiaDll.Exists)
        {
            yield return this.m_MsdiaDll.FullName;
        }
#endif
    }
}
