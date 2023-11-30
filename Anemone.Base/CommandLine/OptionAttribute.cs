// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Anemone.Base.CommandLine;

[AttributeUsage(AttributeTargets.Property)]
public class OptionAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; set; }
    public string? Alias { get; set; }
    public bool Required { get; set; }

    public OptionAttribute(string name)
    {
        this.Name = name;
    }
}
