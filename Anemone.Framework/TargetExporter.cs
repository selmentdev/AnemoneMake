// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.IO;

namespace Anemone.Framework;

/// <summary>
///     Represents a base class for all target exporters.
/// </summary>
public abstract class TargetExporter
{
    /// <summary>
    ///     Exports the target to the specified text writer.
    /// </summary>
    /// <param name="writer">
    ///     A text writer to which the target is exported.
    /// </param>
    /// <param name="target">
    ///     A target to export.
    /// </param>
    public abstract void Export(TextWriter writer, ResolvedTarget target);
}
