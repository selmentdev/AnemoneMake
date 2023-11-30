// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Reflection;

namespace Anemone.Framework;

/// <summary>
///     Represents a target rules descriptor.
/// </summary>
public sealed class TargetDescriptor
{
    private readonly Type m_Type;

    /// <summary>
    ///     Gets name of target.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets target source file location.
    /// </summary>
    public string SourceLocation { get; }

    /// <summary>
    ///     Creates new instance of <see cref="TargetDescriptor" />.
    /// </summary>
    /// <param name="type">
    ///     A type of target.
    /// </param>
    /// <param name="attribute">
    ///     A target rules attribute.
    /// </param>
    internal TargetDescriptor(Type type, TargetRulesAttribute attribute)
    {
        this.m_Type = type;
        this.Name = type.Name;
        this.SourceLocation = attribute.Location;
    }

    /// <summary>
    ///     Creates new instance of <see cref="TargetRules" /> from current descriptor.
    /// </summary>
    /// <param name="context">
    ///     An evaluation context.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="TargetRules" />.
    /// </returns>
    public TargetRules Create(ResolveContext context)
    {
        return Activator.CreateInstance(this.m_Type, context) as TargetRules ??
            throw new InvalidOperationException($@"Failed to create target rules for type '{this.Name}'");
    }

    /// <summary>
    ///     Determines if specified platform is supported by current target.
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
