// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;

namespace Anemone.Framework;

/// <summary>
///     Represents a base class for all SDK rules.
/// </summary>
public abstract class SdkRules
{
    /// <summary>
    ///     Enumerates all include paths provided by the current Sdk.
    /// </summary>
    public IEnumerable<string> IncludePaths { get; protected init; } = Enumerable.Empty<string>();

    /// <summary>
    ///     Enumerates all library paths provided by the current Sdk.
    /// </summary>
    public IEnumerable<string> LibraryPaths { get; protected init; } = Enumerable.Empty<string>();

    /// <summary>
    ///     Enumerates all dependency files required to run binaries built using the current Sdk.
    /// </summary>
    public virtual IEnumerable<string> GetDependencyFiles(ResolvedTarget target)
    {
        yield break;
    }
}
