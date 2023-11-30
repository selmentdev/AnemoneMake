// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Anemone.Base;

public sealed class ApplicationContext
{
    #region Static Properties
    private static readonly string s_RootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../"));
    public static string? OverrideRootDirectory { get; set; } = null;
    public static string RootDirectory => OverrideRootDirectory ?? s_RootDirectory;
    #endregion

    #region Singleton
    private static readonly object s_Lock = new();

    public static ApplicationContext Current { get; private set; } = null!;
    #endregion

    #region Constructors
    public ApplicationContext()
    {
        lock (s_Lock)
        {
            if (ApplicationContext.Current != null)
            {
                throw new InvalidOperationException("An instance of ApplicationContext already exists.");
            }

            ApplicationContext.Current = this;

            this.Plugins = LoadPlugins();
        }
    }
    #endregion

    #region Plugins support

    private static Assembly LoadAssembly(string path)
    {
        return Assembly.LoadFile(path);
    }
    private static IReadOnlyList<Assembly> LoadPlugins()
    {
        var root = AppDomain.CurrentDomain.BaseDirectory;
        var plugins = Directory.EnumerateFiles(root, @"Anemone.*.dll", SearchOption.TopDirectoryOnly);
        return plugins.Select(LoadAssembly).ToList();
    }
    public IReadOnlyList<Assembly> Plugins { get; }
    #endregion
}
