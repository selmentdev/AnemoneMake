// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Represents a base class for all tool rules.
/// </summary>
public abstract class ToolRules
{
    /// <summary>
    ///     Gets resolved module for the current rules.
    /// </summary>
    protected ResolvedModule Module { get; }

    /// <summary>
    ///     Gets resolved target for the current rules.
    /// </summary>
    protected ResolvedTarget Target => this.Module.Target;

    /// <summary>
    ///     Creates new instance of <see cref="ToolRules" />.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    protected ToolRules(ResolvedModule module)
    {
        this.Module = module;
    }

    public bool IsSupported { get; protected init; } = true;
}
