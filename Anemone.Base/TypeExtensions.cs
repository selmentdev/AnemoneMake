// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Anemone.Base;

public static class TypeExtensions
{
    public static Type GetUnderlyingType(this Type self)
    {
        return Nullable.GetUnderlyingType(self) ?? self;
    }

    public static bool AllowsNull(this Type self)
    {
        return Nullable.GetUnderlyingType(self) != null || !self.IsValueType;
    }

    public static Type? GetInterfaceType(this Type self, Type interfaceType)
    {
        if (self.IsGenericType && self.GetGenericTypeDefinition() == interfaceType)
        {
            return self;
        }

        return self.GetInterfaces().SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);
    }

    public static Type? GetCollectionType(this Type self)
    {
        return GetInterfaceType(self, typeof(ICollection<>));
    }

    public static Type? GetDictionaryType(this Type self)
    {
        return GetInterfaceType(self, typeof(IDictionary<,>));
    }

    public static object? ConvertValue(object? value, Type type)
    {
        if (value == null)
        {
            if (type.AllowsNull())
            {
                return null;
            }

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        type = type.GetUnderlyingType();

        if (value.GetType() == type)
        {
            return value;
        }

        var fromConverter = TypeDescriptor.GetConverter(type);
        if (fromConverter.CanConvertFrom(value.GetType()))
        {
            return fromConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
        }

        var toConverter = TypeDescriptor.GetConverter(value.GetType());
        if (toConverter.CanConvertTo(type))
        {
            return toConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, type);
        }

        throw new InvalidCastException($"Cannot convert {value.GetType()} to {type}");
    }
}
