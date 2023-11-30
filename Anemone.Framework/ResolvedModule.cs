// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Anemone.Framework;

/// <summary>
///     Represents a resolved module.
/// </summary>
[DebuggerDisplay("{this.Rules.Name}")]
public sealed class ResolvedModule
{
    /// <summary>
    ///     Creates new instance of <see cref="ResolvedModule" />.
    /// </summary>
    /// <param name="target">
    ///     A target rules to resolve.
    /// </param>
    /// <param name="rules">
    ///     A module rules to resolve.
    /// </param>
    public ResolvedModule(ResolvedTarget target, ModuleRules rules)
    {
        this.Target = target;
        this.Rules = rules;

        this.Language = ResolveModuleLanguage(rules.Language);

        this.OptimizationLevel = ResolveOptimizationLevel(target.Rules.Configuration, this.Rules.Kind);

        var optimized = this.OptimizationLevel is OptimizationLevel.Optimized or OptimizationLevel.Full;

        var enableSanitizers = CanEnableSanitizers(target);

        this.ExportDefine = this.Rules.ExportDefine;

        this.ExternalSource = this.Rules.ExternalSource;

        // Unity files are explicitely disabled in C as many third party libraries are not compatible.
        this.EnableUnity =
            (this.Rules.EnableUnity ?? this.Target.Rules.EnableUnity) &&
            this.Language != ModuleLanguage.C;

        // Static runtime makes sense only in monolithic builds.
        this.EnableStaticRuntime =
            this.Target.Rules is { EnableStaticRuntime: true, LinkKind: TargetLinkKind.Monolithic };

        // Enable assembly for module.
        this.EnableAssembly = this.Rules.EnableAssembly;

        this.EnableDebugInfo =
            this.Rules.EnableDebugInfo ?? this.Target.Rules.EnableDebugInfo;

        this.EnableRtti =
            this.Rules.EnableRtti ?? this.Target.Rules.EnableRtti;

        this.EnableExceptions =
            this.Rules.EnableExceptions ?? this.Target.Rules.EnableExceptions;

        this.EnableAddressSanitizer =
            enableSanitizers &&
            (this.Rules.EnableAddressSanitizer ?? this.Target.Rules.EnableAddressSanitizer);

        this.EnableThreadSanitizer =
            enableSanitizers &&
            (this.Rules.EnableThreadSanitizer ?? this.Target.Rules.EnableThreadSanitizer);

        this.EnableMemorySanitizer =
            enableSanitizers &&
            (this.Rules.EnableMemorySanitizer ?? this.Target.Rules.EnableMemorySanitizer);

        this.EnableUndefinedBehaviorSanitizer =
            enableSanitizers &&
            (this.Rules.EnableUndefinedBehaviorSanitizer ?? this.Target.Rules.EnableUndefinedBehaviorSanitizer);

        this.EnableDataFlowSanitizer =
            enableSanitizers &&
            (this.Rules.EnableDataFlowSanitizer ?? this.Target.Rules.EnableDataFlowSanitizer);

        this.EnableLeakSanitizer =
            enableSanitizers &&
            (this.Rules.EnableLeakSanitizer ?? this.Target.Rules.EnableLeakSanitizer);

        this.EnableEditAndContinue =
            this.Rules.EnableEditAndContinue ?? this.Target.Rules.EnableEditAndContinue;

        this.EnableIncrementalLinking =
            this.Target.Rules.EnableIncrementalLinking;

        this.EnableLinkTimeCodeGeneration =
            this.Target.Rules.EnableLinkTimeCodeGeneration &&
            optimized;

        this.EnableSymbolsCollection =
            (this.Rules.EnableSymbolsCollection ?? this.Target.Rules.EnableSymbolsCollection) &&
            this.OptimizationLevel == OptimizationLevel.Full;

        this.EnableAvx =
            this.Target.Rules.EnableAvx;

        this.EnableAvx2 =
            this.Target.Rules.EnableAvx2;

        this.EnableNeon =
            this.Target.Rules.EnableNeon;

        this.Compiler = this.Target.Platform.CreateCompiler(this);
        this.Linker = this.Target.Platform.CreateLinker(this);
        this.Assembler = this.Target.Platform.CreateAssembler(this);
        this.CodeGenerators = this.Target.Platform.CreateCodeGenerators(this);
        this.ResourceCompilers = this.Target.Platform.CreateResourceCompilers(this);


        //
        // Validation part.
        // TODO: move it to separate phase.
        //

        if ((rules.TestRuns.Count > 0) && (!ModuleLinkKindExtensions.IsApplication(this.LinkKind)))
        {
            throw new Exception($"Module {this.Name} is not an application, but has test runs defined.");
        }
    }

    private readonly HashSet<ResolvedModule> m_IncomingReferences = new();

    private readonly Dictionary<ResolvedModule, ModuleReferenceType> m_OutgoingReferences = new();

    /// <summary>
    ///     Gets collection of nodes that reference current module.
    /// </summary>
    public IReadOnlyCollection<ResolvedModule> IncomingReferences => this.m_IncomingReferences;

    /// <summary>
    ///     Gets collection of modules referenced by currnet module.
    /// </summary>
    public IReadOnlyDictionary<ResolvedModule, ModuleReferenceType> OutgoingReferences => this.m_OutgoingReferences;

    /// <summary>
    ///     Adds reference to another module.
    /// </summary>
    /// <param name="other">
    ///     A module to reference.
    /// </param>
    /// <param name="type">
    ///     A type of the reference.
    /// </param>
    /// <param name="edges">
    ///     A collection of edges.
    /// </param>
    internal void AddReference(
        ResolvedModule other,
        ModuleReferenceType type,
        Dictionary<ResolvedTarget.Edge, ModuleReferenceType> edges)
    {
        if (this.m_OutgoingReferences.ContainsKey(other))
        {
            throw new InvalidOperationException($"Module {this.Name} already has a reference to {other.Name}.");
        }

        this.m_OutgoingReferences.Add(other, type);
        other.m_IncomingReferences.Add(this);


        //
        // Propagate dependency by type.
        //

        switch (type)
        {
            case ModuleReferenceType.Public:
                {
                    this.Dependencies.Add(other);
                    this.InterfaceDependencies.Add(other);
                    break;
                }

            case ModuleReferenceType.Interface:
                {
                    this.InterfaceDependencies.Add(other);
                    break;
                }

            case ModuleReferenceType.Private:
                {
                    this.Dependencies.Add(other);
                    break;
                }

            default:
                {
                    throw new Exception($"Unknown reference type {type}.");
                }
        }

        edges.Add(new(this, other), type);
    }

    #region Resolved Properties
    //
    // Resolved properties:
    //
    //      - regular, resolved properties are private to the current modules,
    //      - interface resolved properties are part of property propagation and contain all interface properties of dependent modules.
    //


    // TODO:
    //      Preprocessor defines should be parsed and stored in dictionary instead.
    //      This way we can check for duplicated defines with different values, or even represent lack of value.
    //
    //      `Dictionary<string, string?>` should be used instead.

    public CompilerRules Compiler { get; }
    public LinkerRules Linker { get; }
    public CompilerRules? Assembler { get; }
    public IReadOnlyCollection<CodeGeneratorRules> CodeGenerators { get; }
    public IReadOnlyCollection<ResourceCompilerRules> ResourceCompilers { get; }

    /// <summary>
    ///     Gets collcetion of resolved module preprocessor defines.
    /// </summary>
    public HashSet<string> Defines { get; } = new();

    /// <summary>
    ///     Gets collcetion of resolved module interface preprocessor defines.
    /// </summary>
    public HashSet<string> InterfaceDefines { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module libraries.
    /// </summary>
    public HashSet<string> Libraries { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module interface libraries.
    /// </summary>
    public HashSet<string> InterfaceLibraries { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module include paths.
    /// </summary>
    public HashSet<string> IncludePaths { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module interface include paths.
    /// </summary>
    public HashSet<string> InterfaceIncludePaths { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module library paths.
    /// </summary>
    public HashSet<string> LibraryPaths { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module interface library paths.
    /// </summary>
    public HashSet<string> InterfaceLibraryPaths { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module dependencies.
    /// </summary>
    public HashSet<ResolvedModule> Dependencies { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module interface dependencies.
    /// </summary>
    public HashSet<ResolvedModule> InterfaceDependencies { get; } = new();

    /// <summary>
    ///     Gets collection of resolved module runtime dependencies.
    /// </summary>
    public HashSet<ResolvedModule> RuntimeDependencies { get; } = new();

    /// <summary>
    ///     Gets module kind.
    /// </summary>
    public ModuleKind Kind => this.Rules.Kind;

    /// <summary>
    ///     Gets link kind of current module.
    /// </summary>
    public ModuleLinkKind LinkKind => this.Rules.LinkKind;

    /// <summary>
    ///     Gets resolved module language.
    /// </summary>
    public ModuleLanguage Language { get; }

    /// <summary>
    ///     Gets resolved module optimization level.
    /// </summary>
    public OptimizationLevel OptimizationLevel { get; }

    /// <summary>
    ///     Gets resolved module name.
    /// </summary>
    public string Name => this.Rules.Name;

    /// <summary>
    ///     Gets resolved target rules that current module belongs to.
    /// </summary>
    public ResolvedTarget Target { get; }

    /// <summary>
    ///     Gets resolved module rules.
    /// </summary>
    public ModuleRules Rules { get; }

    /// <summary>
    ///     Gets exported module preprocessor define.
    /// </summary>
    /// <remarks>
    ///     This property defines name of `MODULE_API` macro used to export symbols from dynamic library.
    /// </remarks>
    public string ExportDefine { get; }

    /// <summary>
    ///     Gets value indicating whether current module is built from external sources.
    /// </summary>
    public bool ExternalSource { get; }

    /// <summary>
    ///     Gets value indicating whether current module may be built using unity files.
    /// </summary>
    public bool EnableUnity { get; }

    /// <summary>
    ///     Gets value indicating whether module can be built using static runtime.
    /// </summary>
    public bool EnableStaticRuntime { get; }

    public bool EnableAssembly { get; }

    public bool EnableDebugInfo { get; }

    public bool EnableRtti { get; }

    public bool EnableExceptions { get; }

    public bool EnableEditAndContinue { get; }

    public bool EnableIncrementalLinking { get; }

    public bool EnableLinkTimeCodeGeneration { get; }

    public bool EnableSymbolsCollection { get; }

    public bool EnableAvx { get; }

    public bool EnableAvx2 { get; }

    public bool EnableNeon { get; }

    // Sanitizers
    public bool EnableAddressSanitizer { get; }

    public bool EnableThreadSanitizer { get; }

    public bool EnableMemorySanitizer { get; }

    public bool EnableUndefinedBehaviorSanitizer { get; }

    public bool EnableDataFlowSanitizer { get; }

    public bool EnableLeakSanitizer { get; }
    #endregion

    #region Property Resolving
    private static ModuleLanguage ResolveModuleLanguage(ModuleLanguage language)
    {
        switch (language)
        {
            case ModuleLanguage.C:
            case ModuleLanguage.Cxx:
                {
                    //
                    // Forward language as-is.
                    //

                    return language;
                }

            case ModuleLanguage.Default:
                {
                    //
                    // Default language is C++.
                    //

                    return ModuleLanguage.Cxx;
                }
        }

        throw new Exception($"Unhandled module language {language}.");
    }

    /// <summary>
    ///     Gets optimization level from base module properties.
    /// </summary>
    /// <param name="configuration">
    ///     A configuration.
    /// </param>
    /// <param name="kind">
    ///     A kind of the module.
    /// </param>
    /// <returns>
    ///     An optimization level.
    /// </returns>
    private static OptimizationLevel ResolveOptimizationLevel(TargetConfiguration configuration, ModuleKind kind)
    {
        // TODO: should we compile third party modules with full optimizations enabled?

        switch (configuration)
        {
            case TargetConfiguration.Debug:
                {
                    //
                    // Debug configuration is always debug.
                    //

                    return OptimizationLevel.Debug;
                }

            case TargetConfiguration.GameDebug:
                {
                    if (kind is ModuleKind.GameLibrary or ModuleKind.GameApplication)
                    {
                        //
                        // Game modules are always debug in GameDebug configuration.
                        //

                        return OptimizationLevel.Debug;
                    }

                    //
                    // Other modules are always development in GameDebug configuration.
                    //

                    return OptimizationLevel.Development;
                }

            case TargetConfiguration.EngineDebug:
                {
                    if (kind is ModuleKind.RuntimeLibrary or ModuleKind.EditorLibrary or ModuleKind.EditorApplication)
                    {
                        //
                        // Engine and editor modules are always debug in EngineDebug configuration.
                        //

                        return OptimizationLevel.Debug;
                    }

                    //
                    // Other modules are always development in EngineDebug configuration.
                    //

                    return OptimizationLevel.Development;
                }

            case TargetConfiguration.Development:
                {
                    //
                    // Development configuration is always development.
                    //

                    return OptimizationLevel.Development;
                }

            case TargetConfiguration.Testing:
                {
                    //
                    // Testing configuration is always optimized.
                    //

                    return OptimizationLevel.Optimized;
                }

            case TargetConfiguration.Shipping:
                {
                    //
                    // Only shipping configuration uses all optimizations.
                    //

                    return OptimizationLevel.Full;
                }

            default:
                {
                    //
                    // Unknown configuration.
                    //

                    throw new Exception($@"Unknown configuration: {configuration}");
                }
        }
    }

    private static bool CanEnableSanitizers(ResolvedTarget target)
    {
        var configuration = target.Rules.Configuration;

        switch (configuration)
        {
            case TargetConfiguration.Debug:
            case TargetConfiguration.GameDebug:
            case TargetConfiguration.EngineDebug:
                {
                    //
                    // These configurations built modules using debug runtime.
                    //

                    return false;
                }

            case TargetConfiguration.Development:
            case TargetConfiguration.Testing:
            case TargetConfiguration.Shipping:
                {
                    //
                    // Sanitizers works best when optimizations are enabled.
                    //

                    return true;
                }

            default:
                {
                    throw new Exception($@"Unknown configuration: {configuration}");
                }
        }
    }

    internal void ImportProperties(IReadOnlyDictionary<string, ResolvedModule> lookup)
    {
        //
        // Module-specific generated files path.
        //

        this.IncludePaths.Add(this.Rules.GeneratedFilesDirectory);


        //
        // Target-specific generated files path.
        //

        this.IncludePaths.Add(this.Target.Rules.GeneratedFilesDirectory);


        //
        // Add include paths form code generators.
        //

        foreach (var generator in this.CodeGenerators)
        {
            var descriptor = generator.Descriptor;

            if (descriptor.RequiresInclude && generator.IsSupported)
            {
                var path = Path.Combine(this.Rules.GeneratedFilesDirectory, descriptor.OutputDirectory);
                this.IncludePaths.Add(path);
                this.InterfaceIncludePaths.Add(path);
            }
        }

        //
        // Defines.
        //

        foreach (var item in this.Rules.PublicDefines)
        {
            this.Defines.Add(item);
            this.InterfaceDefines.Add(item);
        }

        foreach (var item in this.Rules.PrivateDefines)
        {
            this.Defines.Add(item);
        }

        foreach (var item in this.Rules.InterfaceDefines)
        {
            this.InterfaceDefines.Add(item);
        }

        //
        // Libraries.
        //

        foreach (var item in this.Rules.PublicLibraries)
        {
            this.Libraries.Add(item);
            this.InterfaceLibraries.Add(item);
        }

        foreach (var item in this.Rules.PrivateLibraries)
        {
            this.Libraries.Add(item);
        }

        foreach (var item in this.Rules.InterfaceLibraries)
        {
            this.InterfaceLibraries.Add(item);
        }


        //
        // Include paths.
        //

        foreach (var item in this.Rules.PublicIncludePaths)
        {
            this.IncludePaths.Add(item);
            this.InterfaceIncludePaths.Add(item);
        }

        foreach (var item in this.Rules.PrivateIncludePaths)
        {
            this.IncludePaths.Add(item);
        }

        foreach (var item in this.Rules.InterfaceIncludePaths)
        {
            this.InterfaceIncludePaths.Add(item);
        }


        //
        // Library paths.
        //

        foreach (var item in this.Rules.PublicLibraryPaths)
        {
            this.LibraryPaths.Add(item);
            this.InterfaceLibraryPaths.Add(item);
        }

        foreach (var item in this.Rules.PrivateLibraryPaths)
        {
            this.LibraryPaths.Add(item);
        }

        foreach (var item in this.Rules.InterfaceLibraryPaths)
        {
            this.InterfaceLibraryPaths.Add(item);
        }


        //
        // Runtime dependencies
        //

        foreach (var item in this.Rules.RuntimeDependencies)
        {
            this.RuntimeDependencies.Add(lookup[item]);
        }


        //
        // Apply target link type to modules.
        //

        switch (this.Rules.Kind)
        {
            case ModuleKind.GameApplication:
            case ModuleKind.EditorApplication:
            case ModuleKind.TestApplication:
            case ModuleKind.BenchmarkApplication:
            case ModuleKind.ConsoleApplication:
            case ModuleKind.RuntimeLibrary:
            case ModuleKind.EditorLibrary:
            case ModuleKind.GameLibrary:
                {
                    //
                    // These module kinds are dependant on target link kind.
                    //

                    switch (this.Target.Rules.LinkKind)
                    {
                        case TargetLinkKind.Modular:
                            {
                                break;
                            }

                        case TargetLinkKind.Monolithic:
                            {
                                this.Defines.Add("ANEMONE_TARGET_MONOLITHIC=1");
                                break;
                            }
                    }

                    break;
                }

            case ModuleKind.ThirdPartyLibrary:
                {
                    //
                    // Modules are not dependant on the link kind.
                    //

                    break;
                }
        }


        //
        // Application module defines.
        //

        switch (this.Rules.LinkKind)
        {
            case ModuleLinkKind.Application:
                {
                    this.Defines.Add("ANEMONE_MODULE_PRIVATE_APPLICATION=1");
                    break;
                }

            case ModuleLinkKind.ConsoleApplication:
                {
                    this.Defines.Add("ANEMONE_MODULE_PRIVATE_CONSOLE_APPLICATION=1");
                    break;
                }

            case ModuleLinkKind.StaticLibrary:
            case ModuleLinkKind.DynamicLibrary:
            case ModuleLinkKind.ImportedLibrary:
                {
                    break;
                }
        }


        //
        // Import target properties.
        //

        foreach (var item in this.Target.Rules.Defines)
        {
            this.Defines.Add(item);
        }

        foreach (var path in this.Target.Rules.IncludePaths)
        {
            this.IncludePaths.Add(path);
        }

        foreach (var path in this.Target.Rules.LibraryPaths)
        {
            this.LibraryPaths.Add(path);
        }
    }
    #endregion
}
