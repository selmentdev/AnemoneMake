// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Anemone.Framework;

/// <summary>
///     Represents a base class for all target rules.
/// </summary>
[DebuggerDisplay("{this.Name}")]
public abstract class TargetRules
{
    #region Target properties
    private readonly ResolveContext m_Context;

    public string SourceLocation { get; }
    public string SourceDirectory { get; }
    public string Name { get; }
    public TargetPlatform Platform => this.m_Context.Platform;
    public TargetArchitecture Architecture => this.m_Context.Architecture;
    public TargetToolchain Toolchain => this.m_Context.Toolchain;
    public TargetConfiguration Configuration => this.m_Context.Configuration;
    #endregion

    #region Target directories
    public string BuildDirectory { get; }
    public string GeneratedFilesDirectory { get; }
    public string BinariesDirectory { get; }
    public string IntermediateDirectory { get; }
    public string ContentDirectory { get; }
    public string RulesDirectory { get; }
    public string ProjectFilesDirectory { get; }
    public string RootDirectory { get; }
    #endregion

    #region Target customization
    public List<string> RequiredModules { get; } = new();
    public string? StartupModule { get; protected init; }
    public TargetLinkKind LinkKind { get; protected init; } = TargetLinkKind.Modular;
    public TargetKind Kind { get; protected init; } = TargetKind.Game;
    public List<string> IncludePaths { get; } = new();
    public List<string> LibraryPaths { get; } = new();
    public List<string> Defines { get; } = new();

    // Note:
    //  Target rules preferences have priority over module preferences.
    //  However, the platform rules have priority over the module rules.

    public bool EnableUnity { get; protected init; } = true;

    // This just allows to enable static runtime in monolithic builds
    public bool EnableStaticRuntime { get; protected init; } = false;

    public bool EnableDebugInfo { get; protected init; } = true;

    public bool EnableRtti { get; protected init; } = true;

    public bool EnableExceptions { get; protected init; } = true;

    // TODO: EditAndContinue requires EnableIncrementalLinking to work
    public bool EnableEditAndContinue { get; protected init; } = false;

    public bool EnableIncrementalLinking { get; protected init; } = false;

    public bool EnableLinkTimeCodeGeneration { get; protected init; } = true;

    public bool EnableAvx { get; protected init; } = true;

    public bool EnableAvx2 { get; protected init; } = false;

    public bool EnableNeon { get; protected init; } = true;

    // Note: All sanitizers are enabled by default on all targets.
    public bool EnableSymbolsCollection { get; protected init; } = true;

    public bool EnableAddressSanitizer { get; protected init; } = true;

    public bool EnableThreadSanitizer { get; protected init; } = true;

    public bool EnableMemorySanitizer { get; protected init; } = true;

    public bool EnableUndefinedBehaviorSanitizer { get; protected init; } = true;

    public bool EnableDataFlowSanitizer { get; protected init; } = true;

    public bool EnableLeakSanitizer { get; protected init; } = true;
    #endregion

    #region Constructors
    protected TargetRules(ResolveContext context)
    {
        var attribute = this.GetType().GetCustomAttribute<TargetRulesAttribute>()
            ?? throw new InvalidOperationException($"Target {this.GetType().Name} does not have a TargetRulesAttribute.");

        var type = this.GetType();

        this.Name = type.Name;

        this.SourceLocation = attribute.Location;

        var targetContext = context.TargetContext;

        this.RootDirectory = targetContext.RootDirectory;
        this.SourceDirectory = targetContext.SourceDirectory;
        this.ContentDirectory = targetContext.ContentDirectory;

        this.BuildDirectory = targetContext.BuildDirectory;
        this.GeneratedFilesDirectory = Path.Combine(targetContext.GeneratedFilesDirectory, context.Configuration.ToString());
        this.BinariesDirectory = Path.Combine(targetContext.BinariesDirectory, context.Configuration.ToString());
        this.IntermediateDirectory = Path.Combine(targetContext.IntermediateDirectory, context.Configuration.ToString());
        this.RulesDirectory = targetContext.RulesDirectory;
        this.ProjectFilesDirectory = targetContext.ProjectFilesDirectory;

        this.m_Context = context;
    }
    #endregion
}
