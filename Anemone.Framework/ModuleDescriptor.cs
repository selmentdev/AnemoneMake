// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Anemone.Framework;

/// <summary>
///     Represents a module rules descriptor.
/// </summary>
public sealed class ModuleDescriptor
{
    private readonly Type m_Type;

    /// <summary>
    ///     Gets name of module.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets module source file location.
    /// </summary>
    public string SourceLocation { get; }

    /// <summary>
    ///     Gets module source file directory.
    /// </summary>
    public string SourceDirectory { get; }

    /// <summary>
    ///     Gets module project kind for IDE generator.
    /// </summary>
    public ModuleKind Kind { get; }

    /// <summary>
    ///     Creates new instance of <see cref="ModuleDescriptor" />.
    /// </summary>
    /// <param name="type">
    ///     A type of module.
    /// </param>
    /// <param name="attribute">
    ///     A module rules attribute.
    /// </param>
    internal ModuleDescriptor(Type type, ModuleRulesAttribute attribute)
    {
        this.m_Type = type;

        this.Name = type.Name;

        this.SourceLocation = attribute.Location;
        this.Kind = attribute.Kind;

        this.SourceDirectory = Path.GetDirectoryName(this.SourceLocation) ?? string.Empty;
    }

    /// <summary>
    ///     Creates new instance of <see cref="ModuleRules" /> from current descriptor.
    /// </summary>
    /// <param name="target">
    ///     A target rules.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="ModuleRules" />.
    /// </returns>
    public ModuleRules Create(TargetRules target)
    {
        return Activator.CreateInstance(this.m_Type, target) as ModuleRules ??
            throw new InvalidOperationException($@"Failed to create module rules for type '{this.Name}'");
    }

    /// <summary>
    ///     Determines if specified platform is supported by current module.
    /// </summary>
    /// <param name="platform">
    ///     A target platform.
    /// </param>
    /// <returns>
    ///     The value indicating if target platform is supported.
    /// </returns>
    public bool IsSupported(TargetPlatform platform)
    {
        var attribute = this.m_Type.GetCustomAttribute<SupportsPlatformAttribute>();

        if (attribute != null)
        {
            //
            // Check if platform is on list.
            //

            return attribute.Platforms.Contains(platform);
        }


        //
        // All platforms are supported otherwise.
        //

        return true;
    }
}
