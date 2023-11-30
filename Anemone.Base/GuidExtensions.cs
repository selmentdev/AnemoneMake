// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Security.Cryptography;
using System.Text;

namespace Anemone.Base;

public static class GuidExtensions
{
    public static Guid FromString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes).AsSpan(0, 16);
        return new Guid(hash);
    }
}
