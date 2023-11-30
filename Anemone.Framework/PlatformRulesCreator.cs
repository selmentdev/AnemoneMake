// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Anemone.Framework;

public abstract class PlatformRulesCreator
{
    public abstract IEnumerable<PlatformRules> Create();
}
