// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;

namespace Anemone.Framework;

/// <summary>
///     Represents a code generator rules.
/// </summary>
public abstract class CodeGeneratorRules : ToolRules
{
    public string SourceDirectory { get; }
    public string GeneratedFilesDirectory { get; }

    /// <summary>
    ///     Gets descriptor of the current code generator.
    /// </summary>
    public CodeGeneratorDescriptor Descriptor { get; }

    /// <summary>
    ///     Creates new instance of <see cref="CodeGeneratorRules" />.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <param name="descriptor">
    ///     A descriptor of the current code generator.
    /// </param>
    protected CodeGeneratorRules(ResolvedModule module, CodeGeneratorDescriptor descriptor)
        : base(module)
    {
        this.Descriptor = descriptor;

        this.SourceDirectory = Path.Combine(module.Rules.SourceDirectory, descriptor.SourceDirectory);
        this.GeneratedFilesDirectory = Path.Combine(module.Rules.GeneratedFilesDirectory, descriptor.OutputDirectory);

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
    public abstract void Generate(List<string> args, string output, string input);
}
