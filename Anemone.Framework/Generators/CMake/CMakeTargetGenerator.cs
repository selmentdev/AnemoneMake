// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base;
using Anemone.Base.Profiling;
using System;
using System.IO;

namespace Anemone.Framework.Generators.CMake;

using Anemone.Framework.Generators.Fastbuild;

[TargetGenerator]
public class CmakeTargetGenerator : TargetGenerator
{
    public CmakeTargetGenerator()
        : base("CMake-v1.0")
    {
    }

    private static string EscapePath(string path)
    {
        return path.Replace("\\", "/");
    }

    public override void Generate(TargetContext context)
    {
        using var scope = Profiler.Profile($@"CMake: {context.Target.Name}");

        // Generate fastbuild rules
        var fastbuildPlatform = new FastbuildTargetPlatform(context);
        fastbuildPlatform.WriteRules();

        var fastbuildSolution = new FastbuildTargetSolution(fastbuildPlatform);
        fastbuildSolution.WriteRules();

        foreach (var target in context.ResolvedTargets)
        {
            var fastbuildTarget = new FastbuildTargetRules(target);
            fastbuildTarget.WriteRules();
        }

        // Generate CMake rules to run fastbuild.
        Directory.CreateDirectory(context.RulesDirectory);
        Console.WriteLine($@"CMAKE: {context.RulesDirectory}");

        var outputPath = Path.Combine(context.RulesDirectory, "CMakeLists.txt");
        using var writer = new CMakeFileWriter(outputPath);

        // Write minimum required version
        writer.WriteLine($@"cmake_minimum_required(VERSION 3.25)");

        writer.WriteLine($@"project(");
        writer.Indent();
        {
            writer.WriteLine($@"{context.Target.Name}");
            writer.WriteLine($@"VERSION 1.0.0");    // FIXME: Add version to target
            writer.WriteLine($@"DESCRIPTION ""{context.Target.Name}""");    // FIXME: Add description to target
            writer.WriteLine($@"HOMEPAGE_URL ""https://some-url.com""");    // FIXME: Add homepage to target
            writer.WriteLine($@"LANGUAGES ");  // FIXME: Add languages to target
            //writer.WriteLine($@"PROJECT_SOURCE_DIR ""{EscapePath(context.SourceDirectory)}""");
            //writer.WriteLine($@"PROJECT_BINARY_DIR ""{EscapePath(context.BinariesDirectory)}""");
            //writer.WriteLine($@"PROJECT_IS_TOP_LEVEL TRUE");
        }
        writer.Unindent();
        writer.WriteLine($@")");
        writer.WriteLine();

        // Setup target configurations
        writer.WriteLine($@"# Target configurations");
        writer.WriteLine($@"set(CMAKE_CONFIGURATION_TYPES Debug GameDebug EngineDebug Development Testing Shipping)");

        // Write out fastbuild executable location
        var rootDirectory = ApplicationContext.RootDirectory;
        var fastbuildExecutable = Path.Combine(rootDirectory, "Tools", "ThirdParty", "FastBuild", "Windows-x64", "fbuild.exe");
        writer.WriteLine($@"set(FASTBUILD_EXECUTABLE ""{EscapePath(fastbuildExecutable)}"")");

        writer.WriteLine($@"# Runtime Core Library");
        writer.WriteLine($@"add_custom_target(");
        writer.Indent();
        {
            writer.WriteLine($@"{context.Target.Name}");
            writer.WriteLine($@"COMMAND ${{FASTBUILD_EXECUTABLE}} -config ""{EscapePath(context.RulesDirectory)}/Target.bff"" Target-$<CONFIG>");
            writer.WriteLine($@"WORKING_DIRECTORY ""{EscapePath(rootDirectory)}""");
            writer.WriteLine($@"COMMAND_EXPAND_LISTS");
            writer.WriteLine($@"VERBATIM");
        }
        writer.Unindent();
        writer.WriteLine($@")");
        writer.WriteLine();
    }
}
