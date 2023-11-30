// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;

namespace Anemone.Framework.Sdks.GamingDesktop;

public sealed class GamingDesktopSdkRules : SdkRules
{
    public string Version { get; }
    public string RootDirectory { get; }
    public string GameKitPath { get; }
    public string ExtensionLibrariesPath { get; }

    private IReadOnlyCollection<string> Extensions { get; } = new[]
    {
        "PlayFab.Multiplayer.Cpp",
        "PlayFab.Party.Cpp",
        "PlayFab.PartyXboxLive.Cpp",
        "Xbox.Game.Chat.2.Cpp.API",
        "Xbox.Services.API.C",
        "Xbox.XCurl.API"
    };

    internal GamingDesktopSdkRules(string version, string path)
    {
        this.RootDirectory = path;

        this.Version = version;

        this.GameKitPath = Path.Combine(path, "GRDK", "GameKit");

        this.ExtensionLibrariesPath = Path.Combine(path, "GRDK", "ExtensionLibraries");

        var includePaths = new List<string>
        {
            Path.Combine(this.GameKitPath, "Include"),
        };

        var libraryPaths = new List<string>
        {
            Path.Combine(this.GameKitPath, "Lib", "amd64"),
        };

        foreach (var extension in this.Extensions)
        {
            includePaths.Add(Path.Combine(this.ExtensionLibrariesPath, extension, "DesignTime", "CommonConfiguration", "neutral", "Include"));
            libraryPaths.Add(Path.Combine(this.ExtensionLibrariesPath, extension, "DesignTime", "CommonConfiguration", "neutral", "Lib"));
        }
    }

    public override IEnumerable<string> GetDependencyFiles(ResolvedTarget target)
    {
        // TODO: Implement copying files from SDK.
        yield break;
    }
}
