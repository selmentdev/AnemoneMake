// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Anemone.Base.CommandLine;

public sealed class Option
{
    public OptionAttribute Attribute { get; }
    public PropertyInfo Property { get; }
    public bool RequiresValue { get; }

    public Option(PropertyInfo property, OptionAttribute attribute)
    {
        this.Attribute = attribute;
        this.Property = property;

        this.RequiresValue = property.PropertyType.GetUnderlyingType() != typeof(bool);
    }

    public void SetValue(object? instance, string value)
    {
        if (typeof(ICollection<string>).IsAssignableFrom(this.Property.PropertyType))
        {
            if (this.Property.GetValue(instance, index: null) is ICollection<string> collection)
            {
                foreach (var item in value.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    collection.Add(item.Trim());
                }
            }
            else
            {
                throw new Exception("Collection is null");
            }
        }
        else if (typeof(IDictionary<string, string>).IsAssignableFrom(this.Property.PropertyType))
        {
            if (this.Property.GetValue(instance, index: null) is IDictionary<string, string> collection)
            {
                foreach (var item in value.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var separator = item.IndexOf('=', StringComparison.OrdinalIgnoreCase);

                    if (separator >= 0)
                    {
                        var itemKey = item[..separator].Trim();
                        var itemValue = item[(separator + 1)..].Trim();
                        collection.Add(itemKey, itemValue);
                    }
                    else
                    {
                        throw new Exception($@"Dictionary item without value: '{item}'");
                    }
                }
            }
            else
            {
                throw new Exception("Collection is null");
            }
        }
        else if (TryParseValue(this.Property.PropertyType, value, out var parsed))
        {
            this.Property.SetValue(instance, parsed, index: null);
        }
        else
        {
            throw new Exception($@"Unable to parse value '{value}'");
        }
    }

    private static bool TryParseValue(Type type, string value, out object? result)
    {
        if (type.IsEnum)
        {
            try
            {
                result = Enum.Parse(type, value, true);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }
        else if (type == typeof(FileInfo))
        {
            try
            {
                result = new FileInfo(value);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        else if (type == typeof(DirectoryInfo))
        {
            try
            {
                result = new DirectoryInfo(value);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        else
        {
            try
            {
                result = TypeExtensions.ConvertValue(value, type);
                return true;
            }
            catch (InvalidCastException)
            {
                result = null;
                return false;
            }
        }
    }
}
