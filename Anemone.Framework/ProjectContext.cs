// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base;
using Anemone.Base.Profiling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Anemone.Framework;

public class ProjectContext
{
    #region Project properties
    public string ProductName { get; }
    public string ProductCompany { get; }
    public string ProductCopyright { get; }
    public Guid ProductGuid { get; }
    public Version ProductVersion { get; }
    public IReadOnlyCollection<string> Targets { get; }
    public IReadOnlyCollection<string> Platforms { get; }
    public string Generator { get; }
    #endregion

    #region Directories
    public string EngineDirectory { get; }
    public string EngineSourceDirectory { get; }

    public string ProjectDirectory { get; }
    public string ProjectSourceDirectory { get; }
    public string ProjectContentDirectory { get; }

    public string OutputDirectory { get; }
    public string OutputGeneratedDirectory { get; }
    public string OutputBuildDirectory { get; }
    public string OutputBinariesDirectory { get; }
    public string OutputIntermediateDirectory { get; }
    public string OutputRulesDirectory { get; }
    public string OutputProjectFilesDirectory { get; }
    #endregion

    #region Constructors
    public ProjectContext(string engineDirectory, string projectFilePath, string outputDirectory)
    {
        using var scope = Profiler.Function();

        //
        // Engine directories.
        //

        this.EngineDirectory = engineDirectory;
        this.EngineSourceDirectory = Path.Combine(engineDirectory, "Source");


        //
        // Project directories.
        //

        this.ProjectDirectory = Path.GetDirectoryName(projectFilePath)
            ?? throw new Exception($@"Invalid project file path: {projectFilePath}");

        this.ProjectSourceDirectory = Path.Combine(this.ProjectDirectory, "Source");
        this.ProjectContentDirectory = Path.Combine(this.ProjectDirectory, "Content");


        //
        // Output directories.
        //

        this.OutputDirectory = outputDirectory;
        this.OutputGeneratedDirectory = Path.Combine(outputDirectory, "Generated");
        this.OutputBuildDirectory = Path.Combine(outputDirectory, "Build");
        this.OutputBinariesDirectory = Path.Combine(outputDirectory, "Binaries");
        this.OutputIntermediateDirectory = Path.Combine(outputDirectory, "Intermediate");
        this.OutputRulesDirectory = Path.Combine(outputDirectory, "Rules");
        this.OutputProjectFilesDirectory = Path.Combine(outputDirectory, "ProjectFiles");


        //
        // Load project descriptor.
        //

        var descriptor = Load(projectFilePath);

        //
        // Validate properties.
        //

        this.ProductName = descriptor.Name
            ?? throw new Exception($@"Product name required in '{projectFilePath}'");

        this.ProductCompany = descriptor.Company
            ?? throw new Exception($@"Product company required in '{projectFilePath}'");

        this.ProductCopyright = descriptor.Copyright
            ?? throw new Exception($@"Product copyright required in '{projectFilePath}'");

        this.ProductVersion = descriptor.Version
            ?? throw new Exception($@"Product version required in '{projectFilePath}'");

        this.Targets = descriptor.Targets
            ?? throw new Exception($@"Targets list required in '{projectFilePath}'");

        this.Platforms = descriptor.Platforms
            ?? throw new Exception($@"Platforms list required in '{projectFilePath}'");

        this.Generator = descriptor.Generator
            ?? throw new Exception($@"Generator required in '{projectFilePath}'");


        //
        // Generate predictable unique ID.
        //

        var sb = new StringBuilder();
        sb.Append(this.ProductName);
        sb.Append(this.ProductCompany);
        this.ProductGuid = GuidExtensions.FromString(sb.ToString());
    }
    #endregion

    #region Project JSON deserialization
    private sealed class Descriptor
    {
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Copyright { get; set; }
        public Version? Version { get; set; }
        public List<string>? Targets { get; set; }
        public List<string>? Platforms { get; set; }

        // TODO: Should this be specified here?
        public string? Generator { get; set; }
    }

    private static readonly JsonSerializerOptions s_JsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };

    private static Descriptor Load(string path)
    {
        using var scope = Profiler.Function();

        var bytes = File.ReadAllBytes(path);
        return JsonSerializer.Deserialize<Descriptor>(bytes, s_JsonSerializerOptions)
            ?? throw new Exception($@"Unable to parse project file '{path}'");
    }
    #endregion
}
