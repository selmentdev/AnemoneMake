// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.IO;

namespace Anemone.Framework.Sdks.Windows;

public sealed class WindowsSdkRules : SdkRules
{
    #region Properties
    public string ResourceCompiler { get; }
    public string Root { get; }
    public string Version { get; }
    #endregion

    #region Constructors
    internal WindowsSdkRules(string root, string version, string host, string target)
    {
        this.Root = root;
        this.Version = version;

        var binariesRoot = Path.Combine(root, "bin", version, host);
        this.ResourceCompiler = Path.Combine(binariesRoot, "rc.exe");

        var includePathsRoot = Path.Combine(root, "Include", version);

        this.IncludePaths = new[]
        {
            Path.Combine(includePathsRoot, "shared"),
            Path.Combine(includePathsRoot, "ucrt"),
            Path.Combine(includePathsRoot, "um"),
            Path.Combine(includePathsRoot, "winrt"),
            Path.Combine(includePathsRoot, "cppwinrt"),
        };

        var libraryPathsRoot = Path.Combine(root, "Lib", version);

        this.LibraryPaths = new[]
        {
            Path.Combine(libraryPathsRoot, "um", target),

            // This is Universal CRT runtime library - should it be visible to the target?
            Path.Combine(libraryPathsRoot, "ucrt", target),
        };
    }
    #endregion
}
