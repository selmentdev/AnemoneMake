// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents a code generator descriptor.
/// </summary>
public sealed class CodeGeneratorDescriptor : ToolDescriptor
{
    public CodeGeneratorDescriptor(TargetPlatform platform, TargetArchitecture architecture, TargetToolchain toolchain, string id)
        : base(platform, architecture, toolchain, id)
    {
    }

    /// <summary>
    ///     Gets name of sub-directory for generated files.
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    ///     Gets value indicating whether generated code should be compiled.
    /// </summary>
    public bool RequiresCompilation { get; set; } = false;

    /// <summary>
    ///     Gets value indicating whether generated code should be added to module include paths.
    /// </summary>
    public bool RequiresInclude { get; set; } = false;
}
