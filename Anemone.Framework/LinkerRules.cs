// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework;

/// <summary>
///     Represents a linker rules.
/// </summary>
public abstract class LinkerRules : ToolRules
{
    /// <summary>
    ///     Gets descriptor of the current linker.
    /// </summary>
    public LinkerDescriptor Descriptor { get; }

    /// <summary>
    ///     Creates new instance of <see cref="LinkerRules" />.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <param name="descriptor">
    ///     A descriptor of the current linker.
    /// </param>
    protected LinkerRules(ResolvedModule module, LinkerDescriptor descriptor)
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
    public abstract void Link(List<string> args, string output, string input);

    /// <summary>
    ///     Gets output file name.
    /// </summary>
    public string Output { get; protected init; } = string.Empty;
}
