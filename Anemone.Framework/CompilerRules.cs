// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework;

/// <summary>
///     Represents a compiler rules.
/// </summary>
public abstract class CompilerRules : ToolRules
{
    /// <summary>
    ///     Gets descriptor of the current compiler.
    /// </summary>
    public CompilerDescriptor Descriptor { get; }

    /// <summary>
    ///     Creates new instance of <see cref="CompilerRules" />.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <param name="descriptor">
    ///     A descriptor of the current compiler.
    /// </param>
    protected CompilerRules(ResolvedModule module, CompilerDescriptor descriptor)
        : base(module)
    {
        this.Descriptor = descriptor;
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
