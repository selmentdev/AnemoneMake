// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.Serialization;

namespace Anemone.Framework;

/// <summary>
///     Represents an exception that is thrown when a circular dependency is detected.
/// </summary>
public class CircularDependencyException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CircularDependencyException" /> class.
    /// </summary>
    public CircularDependencyException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CircularDependencyException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CircularDependencyException(string message)
        : base(message)
    {
    }
}
