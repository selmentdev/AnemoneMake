// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Anemone.Base.CommandLine;

public static class OptionParser
{
    private static void GetOptions(Type type, out Dictionary<string, Option> options)
    {
        options = new();

        foreach (var property in type.GetProperties())
        {
            var attribute = property.GetCustomAttribute<OptionAttribute>();

            if (attribute != null)
            {
                if (options.ContainsKey(attribute.Name))
                {
                    throw new Exception($@"Duplicated option: '{attribute.Name}'");
                }

                var option = new Option(property, attribute);
                options.Add(attribute.Name, option);

                if (attribute.Alias != null)
                {
                    if (options.ContainsKey(attribute.Alias))
                    {
                        throw new Exception($@"Duplicated option: '{attribute.Name}' with alias '{attribute.Alias}'");
                    }

                    options.Add(attribute.Alias, option);
                }
            }
        }
    }

    private static bool TryParseOptionName(string argument, out string name)
    {
        if (argument.StartsWith("--"))
        {
            name = argument[2..];
            return true;
        }

        if (argument.StartsWith('-') || argument.StartsWith('/'))
        {
            name = argument[1..];
            return true;
        }

        name = string.Empty;
        return false;
    }

    private static int ProcessArgument(
        IReadOnlyList<string> args,
        int current,
        object? instance,
        Dictionary<string, Option> options,
        HashSet<Option> processed,
        string arg,
        string name)
    {
        if (options.TryGetValue(name, out var option))
        {
            if (!processed.Add(option))
            {
                throw new Exception($@"Duplicate option: '{arg}'");
            }

            if (option.RequiresValue)
            {
                if (current < args.Count)
                {
                    option.SetValue(instance, args[current]);
                    ++current;
                }
                else
                {
                    throw new Exception($@"Option '{arg}' requires a value");
                }
            }
            else
            {
                option.SetValue(instance, true.ToString());
            }
        }
        else
        {
            throw new Exception($@"Unknown option '{arg}'");
        }

        return current;
    }

    private static void ValidateRequiredOptions(
        IReadOnlyDictionary<string, Option> options,
        IReadOnlySet<Option> processed)
    {
        foreach (var (name, option) in options)
        {
            if (option.Attribute.Required && !processed.Contains(option))
            {
                throw new Exception($@"Missing required option: {name}");
            }
        }
    }

    public static string[] Parse(Type type, object? instance, IReadOnlyList<string> args)
    {
        GetOptions(type, out var options);

        var positional = new List<string>(args.Count);
        var processed = new HashSet<Option>(args.Count);

        var index = 0;

        while (index < args.Count)
        {
            var arg = args[index];

            if (TryParseOptionName(arg, out var name))
            {
                index = ProcessArgument(
                    args,
                    index + 1,
                    instance,
                    options,
                    processed,
                    arg,
                    name);
            }
            else
            {
                ++index;
                positional.Add(arg);
            }
        }

        ValidateRequiredOptions(options, processed);

        return positional.ToArray();
    }

    public static string[] Parse<T>(T? instance, IReadOnlyList<string> args)
    {
        return Parse(typeof(T), instance, args);
    }
}
