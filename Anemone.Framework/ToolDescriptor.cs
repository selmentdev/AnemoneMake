// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;

namespace Anemone.Framework;

/// <summary>
///     Represents a base class for all tool descriptors.
/// </summary>
public abstract class ToolDescriptor
{
    #region Constructors

    protected ToolDescriptor(TargetPlatform platform, TargetArchitecture architecture, TargetToolchain toolchain, string id)
    {
        this.Platform = platform;
        this.Architecture = architecture;
        this.Toolchain = toolchain;
        this.Id = $@"{id}";
    }
    #endregion

    #region Identification
    /// <summary>
    ///     Gets compiler's target platform.
    /// </summary>
    public TargetPlatform Platform { get; }

    /// <summary>
    ///     Gets compiler's target architecture.
    /// </summary>
    public TargetArchitecture Architecture { get; }

    /// <summary>
    ///     Gets compiler's target toolchain.
    /// </summary>
    public TargetToolchain Toolchain { get; }

    /// <summary>
    ///     Gets unique tool id.
    /// </summary>
    public string Id { get; }
    #endregion

    #region Executable
    /// <summary>
    ///     Gets path to executable.
    /// </summary>
    public string Executable { get; set; } = string.Empty;

    /// <summary>
    ///     Gets collection of extra files required to distributed compilation.
    /// </summary>
    public IEnumerable<string>? ExecutableExtraFiles { get; set; }

    /// <summary>
    ///     Gets value indicating whether compiler can be distributed.
    /// </summary>
    public bool AllowDistribution { get; set; } = false;
    #endregion

    #region Properties
    /// <summary>
    ///     Gets collection of input patterns to match source files.
    /// </summary>
    public IEnumerable<string> InputPatterns { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    ///     Gets extension of compiled files.
    /// </summary>
    public string OutputExtension { get; set; } = string.Empty;

    /// <summary>
    ///     Gets name of source directory.
    /// </summary>
    /// <remarks>
    ///     Compiler is used only if source directory is present in the module.
    /// </remarks>
    public string SourceDirectory { get; set; } = string.Empty;
    #endregion
}
