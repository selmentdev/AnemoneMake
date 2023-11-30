// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;

namespace Anemone.Framework;

/// <summary>
///     Represents a resource compiler rules.
/// </summary>
public abstract class ResourceCompilerRules : ToolRules
{
    public string SourceDirectory { get; }

    /// <summary>
    ///     Gets descriptor of the current resource compiler.
    /// </summary>
    public ResourceCompilerDescriptor Descriptor { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceCompilerRules" /> class.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <param name="descriptor">
    ///     A descriptor of the current resource compiler.
    /// </param>
    protected ResourceCompilerRules(ResolvedModule module, ResourceCompilerDescriptor descriptor)
        : base(module)
    {
        this.Descriptor = descriptor;

        this.SourceDirectory = Path.Combine(module.Rules.SourceDirectory, descriptor.SourceDirectory);

        this.IsSupported = Directory.Exists(this.SourceDirectory);
    }

    /// <summary>
    ///     Generates command list for given input and output files.
    /// </summary>
    /// <param name="args">
    ///     A list of command line arguments.
    /// </param>
    /// <param name="output">
    ///     A path to output file.
    /// </param>
    /// <param name="input">
    ///     A path to input file.
    /// </param>
    public abstract void Compile(List<string> args, string output, string input);
}
