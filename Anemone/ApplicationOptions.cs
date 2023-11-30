// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.CommandLine;
using System.Collections.Generic;
using System.IO;

namespace Anemone;

public sealed class ApplicationOptions
{
    [Option("project", Required = true)]
    public FileInfo? ProjectFile { get; set; } = null;

    [Option("generate")]
    public bool GenerateProjectFiles { get; set; } = false;

    [Option("output", Required = true)]
    public DirectoryInfo? OutputDirectory { get; set; } = null;

    [Option("verbose")]
    public bool Verbose { get; set; } = false;

    [Option("nologo")]
    public bool NoLogo { get; set; } = false;

    [Option("dot")]
    public bool GenerateDotGraph { get; set; } = false;

    [Option("trace")]
    public bool TraceProfiler { get; set; } = false;

    public IEnumerable<string> Positional { get; }

    public ApplicationOptions(string[] args)
    {
        this.Positional = OptionParser.Parse(this, args);
    }
}
