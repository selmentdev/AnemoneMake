// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base;
using Anemone.Base.Profiling;
using Anemone.Framework;
using Anemone.GenerateProject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Loader;
using System.Threading;

namespace Anemone;

public sealed class Application
{
    public string StartupDirectory { get; }
    public string RootDirectory { get; }
    public string EngineDirectory { get; }
    public string PluginsDirectory { get; }

    public IReadOnlyCollection<Assembly> Plugins { get; }
    public ApplicationOptions Options { get; }

    public ProjectContext Context { get; }

    public RulesRegistry RulesAssembly { get; }

    public IReadOnlyDictionary<string, PlatformRules> Platforms { get; }

    public IReadOnlyDictionary<string, TargetGenerator> Generators { get; }

    public Application(ApplicationOptions options)
    {
        // Capture startup directory.
        this.StartupDirectory = Environment.CurrentDirectory;

        // Capture place where plugins are placed.
        this.PluginsDirectory = AppDomain.CurrentDomain.BaseDirectory;

        this.RootDirectory = this.StartupDirectory;
        this.EngineDirectory = Path.Combine(this.RootDirectory, "Engine");

        this.Plugins = Directory
            .EnumerateFiles(this.PluginsDirectory, @"Anemone.*.dll", SearchOption.TopDirectoryOnly)
            .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
            .ToArray();

        this.Options = options;

        if (this.Options.Verbose)
        {
            foreach (var plugin in this.Plugins)
            {
                Trace.TraceInformation($@"Plugin: {plugin}");
            }
        }

        if (this.Options.ProjectFile == null)
        {
            throw new Exception(@"Path to project directory must be specified");
        }

        var projectFilePath = this.Options.ProjectFile.FullName;

        var outputDirectory = this.Options.OutputDirectory?.FullName
            ?? this.Options.ProjectFile.DirectoryName
            ?? throw new Exception("Invalid path");

        this.Platforms = CreatePlatforms(this.Plugins);
        this.Generators = CreateGenerators(this.Plugins);

        this.Context = new ProjectContext(
            engineDirectory: this.EngineDirectory,
            projectFilePath: projectFilePath,
            outputDirectory: outputDirectory);

        this.RulesAssembly = RulesRegistry.FromAssembly(
            AssemblyLoadContext.Default.LoadFromAssemblyPath(
                GenerateRules.Execute(this.Plugins, this.Context)));

        if (this.Options.Verbose)
        {
            Trace.TraceInformation($@"Product name:                   '{this.Context.ProductName}'");
            Trace.TraceInformation($@"Product company:                '{this.Context.ProductCompany}'");
            Trace.TraceInformation($@"Product copyright:              '{this.Context.ProductCopyright}'");
            Trace.TraceInformation($@"Product version:                '{this.Context.ProductVersion}'");

            foreach (var target in this.Context.Targets)
            {
                Trace.TraceInformation($@"Target:                         '{target}'");
            }

            Trace.TraceInformation($@"Engine directory:               '{this.Context.EngineDirectory}'");
            Trace.TraceInformation($@"Engine source directory:        '{this.Context.EngineSourceDirectory}'");

            Trace.TraceInformation($@"Project directory:              '{this.Context.ProjectDirectory}'");
            Trace.TraceInformation($@"Project source directory:       '{this.Context.ProjectSourceDirectory}'");

            Trace.TraceInformation($@"Output directory:               '{this.Context.OutputDirectory}'");
        }
    }

    public void Run()
    {
        if (this.Options.GenerateProjectFiles)
        {
            this.GenerateProjectFiles();
        }
    }

    private void GenerateProjectFiles()
    {
        using var scope = Profiler.Function();

        var supportedPlatforms = new List<PlatformRules>();

        foreach (var platformName in this.Context.Platforms)
        {
            if (this.Platforms.TryGetValue(platformName, out var platformRules))
            {
                supportedPlatforms.Add(platformRules);
            }
            else
            {
                Trace.TraceWarning($@"Platform '{platformName}' was not found.");
            }
        }


        //
        // Find targets supported by this project.
        //

        var supportedTargets = new List<TargetDescriptor>();

        foreach (var targetName in this.Context.Targets)
        {
            if (this.RulesAssembly.Targets.TryGetValue(targetName, out var targetDescriptor))
            {
                supportedTargets.Add(targetDescriptor);
            }
            else
            {
                Trace.TraceWarning($@"Target '{targetName}' was not found.");
            }
        }

        var supportedModules = this.RulesAssembly.Modules.Values.ToArray();

        if (!this.Generators.TryGetValue(this.Context.Generator, out var generator))
        {
            throw new Exception($@"Cannot find '{generator}' generator.");
        }

        foreach (var platform in supportedPlatforms)
        {
            foreach (var target in supportedTargets)
            {
                var context = new TargetContext(
                    this.Context,
                    platform,
                    target,
                    supportedModules);

                //
                // Generate target-specific files.
                //

                GenerateTargetHeaders.GenerateHeaders(context);

                //
                // Generate build files.
                //

                generator.Generate(context);
            }
        }
    }

    private static IReadOnlyDictionary<string, PlatformRules> CreatePlatforms(IEnumerable<Assembly> assemblies)
    {
        using var scope = Profiler.Function();

        var result = new Dictionary<string, PlatformRules>();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if (type is { IsAbstract: false, IsClass: true } && type.IsSubclassOf(typeof(PlatformRulesCreator)))
                {
                    if (type.IsDefined(typeof(PlatformRulesCreatorAttribute)))
                    {
                        var creator = Activator.CreateInstance(type) as PlatformRulesCreator
                            ?? throw new InvalidOperationException($@"Failed to create instance of '{type.FullName}'.");

                        foreach (var platform in creator.Create())
                        {
                            if (!result.TryAdd(platform.Moniker, platform))
                            {
                                throw new InvalidOperationException($@"Platform with '{platform.Moniker}' is already registered.");
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private static IReadOnlyDictionary<string, TargetGenerator> CreateGenerators(IEnumerable<Assembly> assemblies)
    {
        using var scope = Profiler.Function();

        var result = new Dictionary<string, TargetGenerator>();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if (type is { IsAbstract: false, IsClass: true } && type.IsSubclassOf(typeof(TargetGenerator)))
                {
                    if (type.IsDefined(typeof(TargetGeneratorAttribute)))
                    {
                        var generator = Activator.CreateInstance(type) as TargetGenerator
                            ?? throw new InvalidOperationException($@"Failed to create instance of '{type.FullName}'.");

                        if (!result.TryAdd(generator.Moniker, generator))
                        {
                            throw new InvalidOperationException($@"Generator with moniker '{generator.Moniker}' is already registered.");
                        }
                    }
                }
            }
        }

        return result;
    }

    private static int Main(string[] args)
    {
        Profiler.BeginTrace(@"Application");

        // Start time profiling.
        var watch = Stopwatch.StartNew();

        // Setup default invariant culture.
        var culture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // Setup garbage collection.
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        // Parse optiosn
        var options = new ApplicationOptions(args);

        if (!options.NoLogo)
        {
            Console.WriteLine("Anemone {0}, 2022 by Karol Grzybowski", typeof(Application).Assembly.GetName().Version);
        }

        if (options.Verbose)
        {
            Trace.Listeners.Add(new ConsoleTraceListener(useErrorStream: true));
        }

        if (options.Verbose)
        {
            Trace.TraceInformation($@"Started at: {DateTime.Now:s}");
            Trace.TraceInformation($@"Arguments: '{string.Join(' ', args)}");
        }

        try
        {
            using var scope = Profiler.Profile(@"Application.Run");

            // Create instance of application
            var app = new Application(options);

            // Create instance lock to prevent running multiple tools from same directory.
            using var instanceLock = new SingleInstanceLock("Anemone", app.RootDirectory, true);

            app.Run();

            return 0;
        }
        catch (Exception? ex)
        {
            do
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);

                if (options.Verbose)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(ex.StackTrace);
                }

                ex = ex.InnerException;
            } while (ex != null);

            Console.ResetColor();

            return 1;
        }
        finally
        {
            watch.Stop();

            if (options.Verbose)
            {
                Trace.TraceInformation($@"Elapsed: {watch.Elapsed}");
            }

            Profiler.EndTrace(@"Application");

            if (options.TraceProfiler)
            {
                // Just save profiler results
                Profiler.Serialize(File.Create("trace.json"));
            }
        }
    }
}
