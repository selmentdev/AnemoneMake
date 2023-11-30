// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Anemone.Framework.Sdks.VisualStudio;

public sealed class VisualStudioInstance
{
    public readonly string Root;
    public readonly string ProductId;
    public readonly string ProductLine;
    public readonly string? Toolkit;
    public readonly Version ToolsetVersion;
    public readonly Version ProductVersion;
    public readonly bool IsPrerelease;

    public VisualStudioInstance(
        string root,
        string productId,
        string productLine,
        string? toolkit,
        Version toolsetVersion,
        Version productVersion,
        bool isPrerelease)
    {
        this.Root = root;
        this.ProductId = productId;
        this.ProductLine = productLine;
        this.Toolkit = toolkit;
        this.ToolsetVersion = toolsetVersion;
        this.ProductVersion = productVersion;
        this.IsPrerelease = isPrerelease;
    }
}
