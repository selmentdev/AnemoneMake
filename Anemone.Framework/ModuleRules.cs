// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Anemone.Framework;

/// <summary>
///     Provides base class for all module rules.
/// </summary>
[DebuggerDisplay("{this.Name}")]
public abstract class ModuleRules
{
    #region Constructors
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModuleRules" /> class.
    /// </summary>
    /// <param name="target"></param>
    protected ModuleRules(TargetRules target)
    {
        var type = this.GetType();

        this.Target = target;
        this.ExportDefine = $@"{type.Name.ToUpperInvariant()}_EXPORTS";

        this.Name = type.Name;

        var attribute = type.GetCustomAttribute<ModuleRulesAttribute>() ?? throw new InvalidOperationException($"Module {type.Name} does not have a ModuleRulesAttribute.");

        this.Kind = attribute.Kind;
        this.SourceLocation = attribute.Location;
        this.SourceDirectory = Path.GetDirectoryName(this.SourceLocation) ?? string.Empty;

        this.PublicIncludePaths.Add(Path.Combine(this.SourceDirectory, "Public"));
        this.PrivateIncludePaths.Add(Path.Combine(this.SourceDirectory, "Private"));

        this.LinkKind = this.GetDefaultModuleLinkKind();

        this.GeneratedFilesDirectory = Path.Combine(target.IntermediateDirectory, this.Name, "Generated");
        this.ObjectFilesDirectory = Path.Combine(target.IntermediateDirectory, this.Name, "ObjectFiles");
        this.UnityFilesDirectory = Path.Combine(target.IntermediateDirectory, this.Name, "UnityFiles");
        this.BinariesDirectory = target.BinariesDirectory;
    }

    private ModuleLinkKind GetDefaultModuleLinkKind()
    {
        switch (this.Kind)
        {
            case ModuleKind.EditorApplication:
            case ModuleKind.GameApplication:
                {
                    return ModuleLinkKind.Application;
                }

            case ModuleKind.BenchmarkApplication:
            case ModuleKind.TestApplication:
            case ModuleKind.ConsoleApplication:
                {
                    return ModuleLinkKind.ConsoleApplication;
                }

            case ModuleKind.EditorLibrary:
            case ModuleKind.GameLibrary:
            case ModuleKind.RuntimeLibrary:
            case ModuleKind.ThirdPartyLibrary:
                {
                    switch (this.Target.LinkKind)
                    {
                        default:
                        case TargetLinkKind.Modular:
                            {
                                return ModuleLinkKind.DynamicLibrary;
                            }

                        case TargetLinkKind.Monolithic:
                            {
                                return ModuleLinkKind.StaticLibrary;
                            }
                    }
                }
        }

        throw new Exception($@"Unhandled module kind {this.Kind} for module {this.Name}");
    }
    #endregion

    #region Documentation
    //
    // Property inheritance:
    //  - public properties are inherited into private and interface properties of resolved module,
    //  - private properties are inherited into private properties of resolved module,
    //  - interface properties are inherited into interface properties of resolved module.
    //
    // This way, all properties are inherited from base module, but only public and interface ones are visible to other modules.
    #endregion

    #region Module Dependencies
    /// <summary>
    ///     Gets a list of public module dependencies.
    /// </summary>
    public List<string> PublicDependencies { get; } = new();

    /// <summary>
    ///     Gets a list of private module dependencies.
    /// </summary>
    public List<string> PrivateDependencies { get; } = new();

    /// <summary>
    ///     Gets a list of interface module dependencies.
    /// </summary>
    public List<string> InterfaceDependencies { get; } = new();

    /// <summary>
    ///     Gets a list of runtime module dependencies.
    /// </summary>
    public List<string> RuntimeDependencies { get; } = new();
    #endregion

    #region Properties
    /// <summary>
    ///     Gets language of current module.
    /// </summary>
    public ModuleLanguage Language { get; protected init; }

    /// <summary>
    ///     Gets kind of current module.
    /// </summary>
    public ModuleKind Kind { get; protected init; }

    /// <summary>
    ///     Gets link kind of current module.
    /// </summary>
    public ModuleLinkKind LinkKind { get; protected init; }

    /// <summary>
    ///     Gets target rules of current module.
    /// </summary>
    public TargetRules Target { get; }

    /// <summary>
    ///     Gets name of current module.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets source location of file declaring current module.
    /// </summary>
    public string SourceLocation { get; }

    /// <summary>
    ///     Gets source directory of file declaring current module.
    /// </summary>
    /// <value></value>
    public string SourceDirectory { get; }

    public string GeneratedFilesDirectory { get; }
    public string ObjectFilesDirectory { get; }
    public string UnityFilesDirectory { get; }
    public string BinariesDirectory { get; }

    /// <summary>
    ///     Gets a list of public include paths.
    /// </summary>
    public List<string> PublicIncludePaths { get; } = new();

    /// <summary>
    ///     Gets a list of private include paths.
    /// </summary>
    public List<string> PrivateIncludePaths { get; } = new();

    /// <summary>
    ///     Gets a list of interface include paths.
    /// </summary>
    public List<string> InterfaceIncludePaths { get; } = new();

    /// <summary>
    ///     Gets a list of public library paths.
    /// </summary>
    public List<string> PublicLibraryPaths { get; } = new();

    /// <summary>
    ///     Gets a list of private library paths.
    /// </summary>
    public List<string> PrivateLibraryPaths { get; } = new();

    /// <summary>
    ///     Gets a list of interface library paths.
    /// </summary>
    public List<string> InterfaceLibraryPaths { get; } = new();

    /// <summary>
    ///     Gets a list of public libraries.
    /// </summary>
    public List<string> PublicLibraries { get; } = new();

    /// <summary>
    ///     Gets a list of private libraries.
    /// </summary>
    public List<string> PrivateLibraries { get; } = new();

    /// <summary>
    ///    Gets a list of interface libraries.
    /// </summary>
    public List<string> InterfaceLibraries { get; } = new();

    /// <summary>
    ///     Gets a list of public preprocessor definitions.
    /// </summary>
    public List<string> PublicDefines { get; } = new();

    /// <summary>
    ///     Gets a list of private preprocessor definitions.
    /// </summary>
    public List<string> PrivateDefines { get; } = new();

    /// <summary>
    ///     Gets a list of interface preprocessor definitions.
    /// </summary>
    public List<string> InterfaceDefines { get; } = new();

    /// <summary>
    ///     Gets name of define used to mark class or function as exported.
    /// </summary>
    public string ExportDefine { get; protected init; }

    /// <summary>
    ///     Gets value indicating whether current module is a external source library.
    /// </summary>
    /// <remarks>
    ///     This option may disable some features which may prevent successful compilation.
    /// </remarks>
    public bool ExternalSource { get; protected init; }

    public bool? EnableUnity { get; protected init; }

    public bool? EnableDebugInfo { get; protected init; }

    public bool? EnableRtti { get; protected init; }

    public bool? EnableExceptions { get; protected init; }

    public bool? EnableAddressSanitizer { get; protected init; }

    public bool? EnableThreadSanitizer { get; protected init; }

    public bool? EnableMemorySanitizer { get; protected init; }

    public bool? EnableUndefinedBehaviorSanitizer { get; protected init; }

    public bool? EnableDataFlowSanitizer { get; protected init; }

    public bool? EnableLeakSanitizer { get; protected init; }

    public bool? EnableEditAndContinue { get; protected init; }

    public bool? EnableSymbolsCollection { get; protected init; }

    public bool EnableAssembly { get; protected init; }
    #endregion

    #region Tests
    public sealed class TestRun
    {
        public required string Name { get; init; } = string.Empty;
        public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);
        public string? WorkingDirectory { get; init; } = null;
        public string[] Arguments { get; init; } = Array.Empty<string>();
    }

    public List<TestRun> TestRuns { get; } = new();
    #endregion
}
