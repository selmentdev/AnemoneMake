// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

/// <summary>
///     Enumerates all supported target configurations.
/// </summary>
public enum TargetConfiguration
{
    /// <summary>
    /// Game and engine modules are built without optimizations.
    /// </summary>
    Debug,

    /// <summary>
    /// Game modules are built without optimizations. Engine modules are built with optimizations.
    /// </summary>
    GameDebug,

    /// <summary>
    /// Game modules are built with optimizations. Engine modules are built without optimizations.
    /// </summary>
    EngineDebug,

    /// <summary>
    /// Game and engine modules are built with optimizations allowing debugging.
    /// </summary>
    Development,

    /// <summary>
    /// Game and engine modules are built with full optimizations enabled. Additional profiling support is enabled.
    /// </summary>
    Testing,

    /// <summary>
    /// Game and engine modules are built with full optimizations.
    /// </summary>
    Shipping,
}

internal static class TargetConfigurationExtensions
{
    public static bool IsDebug(this TargetConfiguration configuration)
    {
        return configuration is
            TargetConfiguration.Debug or
            TargetConfiguration.GameDebug or
            TargetConfiguration.EngineDebug;
    }
}
