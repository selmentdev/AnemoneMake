// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework;

public abstract class TargetGenerator
{
    public string Moniker { get; }

    protected TargetGenerator(string moniker)
    {
        this.Moniker = moniker;
    }

    public abstract void Generate(TargetContext context);
}
