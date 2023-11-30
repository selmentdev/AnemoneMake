// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Anemone.Base;

public static class ArrayExtensions
{
    public static bool IsUnique<TSource>(this IReadOnlyList<TSource> self)
    {
        var comparer = EqualityComparer<TSource>.Default;

        var count = self.Count;

        for (var i = 0; i < count - 1; ++i)
        {
            var current = self[i];

            for (var j = i + 1; j < count; ++j)
            {
                var next = self[j];

                if (comparer.Equals(current, next))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool IsUnique<TValue, TResult>(this IReadOnlyList<TValue> self, Func<TValue, TResult> selector)
    {
        var comparer = EqualityComparer<TResult>.Default;

        var count = self.Count;

        for (var i = 0; i < count - 1; ++i)
        {
            var current = selector(self[i]);

            for (var j = i + 1; j < count; ++j)
            {
                var next = selector(self[j]);

                if (comparer.Equals(current, next))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool IsSortedUnique<T>(this IReadOnlyList<T> self)
    {
        var comparer = EqualityComparer<T>.Default;

        var count = self.Count;

        for (var i = 0; i < count - 1; ++i)
        {
            var current = self[i];
            var next = self[i + 1];

            if (comparer.Equals(current, next))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsSortedUnique<TSource, TResult>(this IReadOnlyList<TSource> self, Func<TSource, TResult> selector)
    {
        var comparer = EqualityComparer<TResult>.Default;

        var count = self.Count;

        for (var i = 0; i < count - 1; ++i)
        {
            var current = selector(self[i]);
            var next = selector(self[i + 1]);

            if (comparer.Equals(current, next))
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToCharUpper(int value)
    {
        value &= 0xF;
        value += 48;

        if (value > 57)
        {
            value += 7;
        }

        return (char)value;
    }

    public static string ToHex(this byte[] value, int startIndex, int length)
    {
        return string.Create(length * 2, (value, startIndex, length), delegate (Span<char> outView, (byte[] value, int startIndex, int length) state)
        {
            var inView = new ReadOnlySpan<byte>(state.value, state.startIndex, state.length);

            var inIndex = 0;
            var outIndex = 0;

            while (inIndex < inView.Length)
            {
                var current = inView[inIndex++];
                outView[outIndex++] = ToCharUpper(current >> 4);
                outView[outIndex++] = ToCharUpper(current);
            }
        });
    }

    public static string ToHex(this byte[] value, int startIndex)
    {
        return ToHex(value, startIndex, value.Length - startIndex);
    }

    public static string ToHex(this byte[] value)
    {
        return ToHex(value, 0, value.Length);
    }
}
