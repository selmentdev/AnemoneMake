// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base;
using Anemone.Base.Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Anemone.Framework.Generators.Fastbuild;

internal sealed class FastbuildTargetSolution
{
    // TODO: Discover solution files from Platform+SDKs
    private static readonly string[] s_SolutionFileExtensions = {
        // C++
        "*.cxx", "*.hxx",
        "*.cc", "*.hh",
        "*.cpp", "*.hpp",
        "*.inl", "*.tpl",
        "*.tpp", "*.txx",
        "*.def", "*.inc",
        // C
        "*.c", "*.h",
        // Windows Resource Compiler
        "*.rc", "*.resx",
        // Manifest files
        "*.manifest",
        // Markdown and documentation
        "*.md", "*.txt",
        // QT
        "*.qrc", "*.ui",
        // C#
        "*.cs",
        // FastBuild
        "*.bff",
        // Assembly
        "*.asm", "*.s", "*.S",
        // Visual Studio
        "*.natvis", "*.editorconfig",
    };

    private static readonly Guid s_CSharpProjectGuid = new("{FAE04EC0-F103-D311-BF4B-00C04FCBFE97}");

    private static readonly Dictionary<(TargetPlatform, TargetArchitecture, TargetToolchain), string> s_ArchitectureNames = new()
    {
        { (TargetPlatform.Windows, TargetArchitecture.AArch64, TargetToolchain.MSVC), "ARM64" },
        { (TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.MSVC), "x64" },
        { (TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.ClangCL), "x64" },
        { (TargetPlatform.Windows, TargetArchitecture.X64, TargetToolchain.Clang), "x64" },
        { (TargetPlatform.GamingDesktop, TargetArchitecture.X64, TargetToolchain.MSVC), "Gaming.Desktop.x64" },
    };

    private FastbuildTargetPlatform FastbuildTargetPlatform { get; }

    public FastbuildTargetSolution(FastbuildTargetPlatform fastbuildPlatform)
    {
        this.FastbuildTargetPlatform = fastbuildPlatform;
    }

    public void WriteRules()
    {
        using var scope = Profiler.Profile(@"Solution");

        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var platformRules = this.FastbuildTargetPlatform.Context.Platform;

        if (!s_ArchitectureNames.TryGetValue(new(platformRules.Platform, platformRules.Architecture, platformRules.Toolchain), out var vsArchitecture))
        {
            Trace.TraceWarning($@"Platform {platformRules.Platform}.{platformRules.Architecture}.{platformRules.Toolchain} does not support Visual Studio solutions");
            return;
        }

        var fastbuildExecutable = Path.Combine(ApplicationContext.RootDirectory, "Tools", "ThirdParty", "FastBuild", "Windows-x64", "fbuild.exe");

        Directory.CreateDirectory(this.FastbuildTargetPlatform.Context.RulesDirectory);

        var location = Path.Combine(this.FastbuildTargetPlatform.Context.RulesDirectory, "Solution.bff");
        using (var writer = new FastbuildWriter(location))
        {
            writer.WriteLine(@"VSSolution( 'Solution' ) {");
            writer.Indent();
            {
                writer.WriteLine($@"// {vsArchitecture}");
                writer.WriteLine(@".ProjectAllowedFileExtensions = {");
                writer.Indent();
                {
                    foreach (var extension in s_SolutionFileExtensions)
                    {
                        writer.WriteLine($@"'{extension}'");
                    }
                }
                writer.Unindent();
                writer.WriteLine(@"}");

                var targetName = this.FastbuildTargetPlatform.Context.Target.Name;
                var platformMoniker = this.FastbuildTargetPlatform.Context.Platform.Moniker;

                var targetRulesPath = Path.Combine(
                    this.FastbuildTargetPlatform.Context.RulesDirectory,
                    "Target.bff");

                var solutionLocation = Path.Combine(
                    this.FastbuildTargetPlatform.Context.RootDirectory,
                    targetName + "-" + platformMoniker + ".sln");

                writer.WriteLine($@".SolutionOutput = '{solutionLocation}'");
                writer.WriteLine(".SolutionProjects = { }");

                // TODO: wrap with function
                var optionUnity = false;
                var optionDistributed = false;
                var optionUseCache = false;
                var optionUseMonitor = true;
                var optionVerbose = false;

                var fastbuildOptions = new StringBuilder();

                if (!optionUnity)
                {
                    fastbuildOptions.Append(" -nounity");
                }

                if (optionDistributed)
                {
                    fastbuildOptions.Append(" -dist");
                }

                if (optionUseCache)
                {
                    fastbuildOptions.Append(" -cache");
                }

                if (optionUseMonitor)
                {
                    fastbuildOptions.Append(" -monitor");
                }

                if (optionVerbose)
                {
                    fastbuildOptions.Append(" -verbose");
                }

                writer.WriteLine($@".CommandBuildProject = '{fastbuildExecutable} -config ""{targetRulesPath}"" {fastbuildOptions}'");
                writer.WriteLine($@".CommandRebuildProject = '{fastbuildExecutable} -config ""{targetRulesPath}"" {fastbuildOptions} -clean'");

                var modules = this.FastbuildTargetPlatform.Context.Modules;

                foreach (var descriptor in modules)
                {
                    var projectFileLocation = Path.Combine(this.FastbuildTargetPlatform.Context.ProjectFilesDirectory, $@"{descriptor.Name}.vcxproj");
                    var projectAlias = $@"Project-{descriptor.Name}";

                    writer.WriteLine($@"VCXProject( '{projectAlias}' ) {{");
                    writer.Indent();
                    {
                        writer.WriteLine($@".ProjectOutput = '{projectFileLocation}'");
                        writer.WriteLine(@".ProjectConfigs = { }");

                        foreach (var configuration in TargetContext.Configurations)
                        {
                            writer.WriteLine("{");
                            writer.Indent();
                            {
                                writer.WriteLine(".Entry = [");
                                writer.Indent();
                                {
                                    writer.WriteLine($@".SolutionPlatform = '{vsArchitecture}'");
                                    writer.WriteLine($@".SolutionConfig = '{configuration}'");
                                    writer.WriteLine($@".Platform = '{vsArchitecture}'");
                                    writer.WriteLine($@".Config = '{configuration}'");
                                    writer.WriteLine($@".Target = 'Module-{descriptor.Name}-{configuration}'");
                                    writer.WriteLine(@".ProjectBuildCommand = 'cd ^$(SolutionDir) &amp; $CommandBuildProject$ $Target$'");
                                    writer.WriteLine(@".ProjectRebuildCommand = 'cd ^$(SolutionDir) &amp; $CommandRebuildProject$ $Target$'");
                                    writer.WriteLine($@".OutputDirectory = '{this.FastbuildTargetPlatform.Context.BinariesDirectory}'");
                                    writer.WriteLine($@".IntermediateDirectory = '{this.FastbuildTargetPlatform.Context.IntermediateDirectory}'");
                                    writer.WriteLine($@".LocalDebuggerWorkingDirectory = '{Path.Join(this.FastbuildTargetPlatform.Context.BinariesDirectory, configuration.ToString())}'");
                                }
                                writer.Unindent();
                                writer.WriteLine("]");
                                writer.WriteLine("^ProjectConfigs + .Entry");
                            }
                            writer.Unindent();
                            writer.WriteLine("}");
                        }

                        writer.WriteLine(@".ProjectInputPaths = {");
                        writer.Indent();
                        {
                            writer.WriteLine($@"'{descriptor.SourceDirectory}'");
                        }
                        writer.Unindent();
                        writer.WriteLine(@"}");
                        writer.WriteLine($@".ProjectBasePath = '{descriptor.SourceDirectory}'");

                        writer.WriteLine($@"^SolutionProjects + '{projectAlias}'");
                    }
                    writer.Unindent();
                    writer.WriteLine("}");
                }

                {
                    var projectFileLocation = Path.Combine(this.FastbuildTargetPlatform.Context.ProjectFilesDirectory, @"BuildAll.vcxproj");
                    const string projectAlias = @"Project-BuildAll";

                    writer.WriteLine($@"VCXProject( '{projectAlias}' ) {{");
                    writer.Indent();
                    {
                        writer.WriteLine($@".ProjectOutput = '{projectFileLocation}'");
                        writer.WriteLine(@".ProjectConfigs = { }");

                        foreach (var configuration in TargetContext.Configurations)
                        {
                            writer.WriteLine("{");
                            writer.Indent();
                            {
                                writer.WriteLine(".Entry = [");
                                writer.Indent();
                                {
                                    writer.WriteLine($@".SolutionPlatform = '{vsArchitecture}'");
                                    writer.WriteLine($@".SolutionConfig = '{configuration}'");
                                    writer.WriteLine($@".Platform = '{vsArchitecture}'");
                                    writer.WriteLine($@".Config = '{configuration}'");
                                    writer.WriteLine($@".Target = 'Target-{configuration}'");
                                    writer.WriteLine(@".ProjectBuildCommand = 'cd ^$(SolutionDir) &amp; $CommandBuildProject$ $Target$'");
                                    writer.WriteLine(@".ProjectRebuildCommand = 'cd ^$(SolutionDir) &amp; $CommandRebuildProject$ $Target$'");
                                    writer.WriteLine($@".OutputDirectory = '{this.FastbuildTargetPlatform.Context.IntermediateDirectory}'");
                                    writer.WriteLine($@".IntermediateDirectory = '{this.FastbuildTargetPlatform.Context.IntermediateDirectory}'");
                                }
                                writer.Unindent();
                                writer.WriteLine("]");
                                writer.WriteLine("^ProjectConfigs + .Entry");
                            }
                            writer.Unindent();
                            writer.WriteLine("}");
                        }
                    }
                    writer.Unindent();
                    writer.WriteLine("}");

                    writer.WriteLine($@".SolutionBuildProject = '{projectAlias}'");
                }

                {
                    var projectFileLocation = Path.Combine(
                        this.FastbuildTargetPlatform.Context.ProjectFilesDirectory,
                        @"RunTests.vcxproj");
                    const string projectAlias = "Project-RunTests";

                    writer.WriteLine($@"VCXProject( '{projectAlias}' ) {{");
                    writer.Indent();
                    {
                        writer.WriteLine($@".ProjectOutput = '{projectFileLocation}'");
                        writer.WriteLine(@".ProjectConfigs = { }");

                        foreach (var configuration in TargetContext.Configurations)
                        {
                            writer.WriteLine("{");
                            writer.Indent();
                            {
                                writer.WriteLine(".Entry = [");
                                writer.Indent();
                                {
                                    writer.WriteLine($@".SolutionPlatform = '{vsArchitecture}'");
                                    writer.WriteLine($@".SolutionConfig = '{configuration}'");
                                    writer.WriteLine($@".Platform = '{vsArchitecture}'");
                                    writer.WriteLine($@".Config = '{configuration}'");
                                    writer.WriteLine($@".Target = 'Tests-{configuration}'");
                                    writer.WriteLine(@".ProjectBuildCommand = 'cd ^$(SolutionDir) &amp; $CommandBuildProject$ $Target$'");
                                    writer.WriteLine(@".ProjectRebuildCommand = 'cd ^$(SolutionDir) &amp; $CommandRebuildProject$ $Target$'");
                                    writer.WriteLine($@".OutputDirectory = '{this.FastbuildTargetPlatform.Context.IntermediateDirectory}'");
                                    writer.WriteLine($@".IntermediateDirectory = '{this.FastbuildTargetPlatform.Context.IntermediateDirectory}'");
                                }
                                writer.Unindent();
                                writer.WriteLine("]");
                                writer.WriteLine("^ProjectConfigs + .Entry");
                            }
                            writer.Unindent();
                            writer.WriteLine("}");
                        }
                    }
                    writer.Unindent();
                    writer.WriteLine("}");
                }

                void WriteExternalProject(Guid guid, string alias, string path)
                {
                    writer.WriteLine($@"VSProjectExternal( '{alias}' ) {{");
                    writer.Indent();
                    {

                        writer.WriteLine($@".ExternalProjectPath = '{path}'");
                        writer.WriteLine($@".ProjectGuid = '{guid:B}'");
                        writer.WriteLine($@".ProjectTypeGuid = '{s_CSharpProjectGuid:B}'");
                        writer.WriteLine(@".ProjectConfigs = { }");

                        foreach (var configuration in TargetContext.Configurations)
                        {
                            writer.WriteLine("{");
                            writer.Indent();
                            {
                                writer.WriteLine(".Entry = [");
                                writer.Indent();
                                {
                                    writer.WriteLine($@".Platform = '{vsArchitecture}'");
                                    writer.WriteLine($@".Config = '{configuration}'");
                                }
                                writer.Unindent();
                                writer.WriteLine("]");
                                writer.WriteLine("^ProjectConfigs + .Entry");
                            }
                            writer.Unindent();
                            writer.WriteLine("}");
                        }
                    }

                    writer.Unindent();
                    writer.WriteLine("}");
                }

                WriteExternalProject(
                    alias: "Project-Rules",
                    guid: new("{E2D3B0B4-0814-4A6A-AB8B-A9F2BEDC4324}"),
                    path: Path.Combine(this.FastbuildTargetPlatform.Context.Context.OutputRulesDirectory, "Generated", "Source", @"Rules.csproj"));

                {
                    writer.WriteLine(@".SolutionConfigs = { }");

                    foreach (var configuration in TargetContext.Configurations)
                    {
                        writer.WriteLine("{");
                        writer.Indent();
                        {
                            writer.WriteLine(@".Entry = [");
                            writer.Indent();
                            {
                                writer.WriteLine($@".SolutionPlatform = '{vsArchitecture}'");
                                writer.WriteLine($@".SolutionConfig = '{configuration}'");
                                writer.WriteLine($@".Platform = '{vsArchitecture}'");
                                writer.WriteLine($@".Config = '{configuration}'");
                            }
                            writer.Unindent();
                            writer.WriteLine("]");

                            writer.WriteLine(@"^SolutionConfigs + .Entry");
                        }
                        writer.Unindent();
                        writer.WriteLine("}");
                    }
                }
                {
                    void WriteFolder(string name, string logicalPath, IEnumerable<string> projects, IEnumerable<string>? items = null)
                    {
                        writer.WriteLine("{");
                        writer.Indent();
                        {
                            writer.WriteLine($@".Folder_{name} = [");
                            writer.Indent();
                            {
                                writer.WriteLine($@".Path = '{logicalPath}'");
                                writer.WriteLine(@".Projects = {");
                                writer.Indent();
                                {
                                    foreach (var project in projects)
                                    {
                                        writer.WriteLine($@"'{project}'");
                                    }
                                }
                                writer.Unindent();
                                writer.WriteLine("}");

                                if (items != null)
                                {
                                    writer.WriteLine(@".Items = {");
                                    writer.Indent();
                                    {
                                        foreach (var item in items)
                                        {
                                            writer.WriteLine($@"'{item}'");
                                        }
                                    }
                                    writer.Unindent();
                                    writer.WriteLine("}");
                                }
                            }
                            writer.Unindent();
                            writer.WriteLine("]");

                            writer.WriteLine($@"^SolutionFolders + .Folder_{name}");
                        }
                        writer.Unindent();
                        writer.WriteLine("}");
                    }

                    writer.WriteLine(@".SolutionFolders = { }");

                    var mapFolderToModules = new Dictionary<string, List<string>>();

                    foreach (var descriptor in modules)
                    {
                        var path = Path.GetRelativePath(ApplicationContext.RootDirectory, Path.Combine(descriptor.SourceDirectory, ".."));

                        if (mapFolderToModules.TryGetValue(path, out var items))
                        {
                            items.Add($@"Project-{descriptor.Name}");
                        }
                        else
                        {
                            mapFolderToModules.Add(path, new List<string>() { $@"Project-{descriptor.Name}" });
                        }
                    }

                    foreach (var (path, content) in mapFolderToModules)
                    {
                        var fbVariableName = path.Replace('\\', '_');
                        WriteFolder(fbVariableName, path, content);
                    }

                    WriteFolder("Targets", "Targets", new[] {
                    "Project-BuildAll",
                    "Project-RunTests",
                });

                    var solutionFiles = new List<string>()
                {
                    Path.Combine(ApplicationContext.RootDirectory, ".clang-format")
                };

                    WriteFolder("Rules", "Rules", new[] {
                    "Project-Rules"
                }, solutionFiles);
                }
                {
                    writer.WriteLine(@".SolutionDependencies = [");
                    writer.Indent();
                    {
                        writer.WriteLine(@".Projects = .SolutionProjects");
                        writer.WriteLine(@".Dependencies = { 'Project-BuildAll' }");
                    }
                    writer.Unindent();
                    writer.WriteLine(@"]");

                    //writer.WriteLine(@"Print(.SolutionDependencies)");
                }
            }
            writer.Unindent();
            writer.WriteLine(@"}");
        }
    }
}
