// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.Profiling;
using Anemone.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Anemone.GenerateProject;

public static class GenerateRules
{
    public static string Execute(
        IReadOnlyCollection<Assembly> plugins,
        ProjectContext context)
    {
        using var scope = Profiler.Function();

        var pathGenerated = Path.Combine(context.OutputRulesDirectory, "Generated");
        var pathGeneratedSources = Path.Combine(pathGenerated, "Source");
        var pathGeneratedBinaries = Path.Combine(pathGenerated, "Binaries");

        Directory.CreateDirectory(pathGeneratedSources);

        var rulesProjectFilePath = Path.Combine(pathGeneratedSources, "Rules.csproj");
        using (var projectFile = File.Create(rulesProjectFilePath))
        {
            var paths = new HashSet<string>
            {
                context.EngineSourceDirectory,
                context.ProjectSourceDirectory,
            };

            var files = GetFiles(paths);
            WriteProjectFile(projectFile, files, plugins);
        }

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $@"publish {rulesProjectFilePath} --output {pathGeneratedBinaries} --configuration Release --no-dependencies --verbosity quiet --nologo",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        });

        RunDotnetbuild(process);

        return Path.Combine(pathGeneratedBinaries, "Rules.dll");
    }

    private static void RunDotnetbuild(Process? process)
    {
        using var scope = Profiler.Function();

        try
        {
            if (process != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(process.StandardOutput.ReadToEnd());
                Console.ResetColor();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    // And stderr as errors.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(process.StandardError.ReadToEnd());
                    Console.ResetColor();

                    throw new Exception("Failed to compile rules assembly.");
                }
            }
            else
            {
                throw new Exception("Failed to start process 'dotnet'");
            }
        }
        finally
        {
            Console.ResetColor();
        }
    }

    private static IEnumerable<string> GetFiles(IEnumerable<string> paths)
    {
        using var scope = Profiler.Function();

        foreach (var path in paths)
        {
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new Exception($@"Directory '{path}' does not exist");
            }

            foreach (var file in directory.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            {
                var fullPath = file.FullName;

                if (fullPath.EndsWith(".Target.cs", StringComparison.InvariantCultureIgnoreCase) ||
                    fullPath.EndsWith("Module.cs", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return fullPath;
                }
            }
        }
    }
    private static void WriteProjectFile(Stream stream, IEnumerable<string> files, IReadOnlyCollection<Assembly> plugins)
    {
        using var scope = Profiler.Function();

        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Indent = true,
            ConformanceLevel = ConformanceLevel.Fragment,
        });

        writer.WriteStartElement("Project");
        writer.WriteAttributeString("Sdk", "Microsoft.NET.Sdk");

        WriteProjectProperties(writer);
        WriteReferences(writer, plugins);
        WriteFilesList(writer, files);

        writer.WriteEndElement();
    }

    private static void WriteFilesList(XmlWriter writer, IEnumerable<string> files)
    {
        using var scope = Profiler.Function();

        writer.WriteStartElement("ItemGroup");

        foreach (var file in files)
        {
            writer.WriteStartElement("Compile");
            writer.WriteAttributeString("Include", file);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteReferences(XmlWriter writer, IReadOnlyCollection<Assembly> plugins)
    {
        using var scope = Profiler.Function();

        writer.WriteStartElement("ItemGroup");

        foreach (var plugin in plugins)
        {
            writer.WriteStartElement("Reference");
            writer.WriteAttributeString("Include", plugin.Location);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteProjectProperties(XmlWriter writer)
    {
        using var scope = Profiler.Function();

        var configurations = TargetContext.Configurations.Select(x => x.ToString()).ToList();
        if (!configurations.Contains("Release"))
        {
            configurations.Add("Release");
        }

        writer.WriteStartElement("PropertyGroup");
        writer.WriteElementString("TargetFramework", "net8.0");
        writer.WriteElementString("Nullable", "enable");
        writer.WriteElementString("Platforms", string.Join(';', "AnyCPU", "x64", "ARM64"));
        writer.WriteElementString("Configurations", string.Join(';', configurations));
        writer.WriteElementString("Optimize", "True");
        writer.WriteEndElement();
    }
}

