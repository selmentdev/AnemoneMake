// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace Anemone.Base.Profiling;

// Implements https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview
// Usage https://www.chromium.org/developers/how-tos/trace-event-profiling-tool

public abstract class ProfileEvent
{
    public abstract void Serialize(ref Utf8JsonWriter writer);
}

public sealed class ProfileEventDuration : ProfileEvent
{
    public string Category { get; }
    public string Name { get; }
    public char Phase { get; }
    public long Timestamp { get; }
    public uint ThreadId { get; }


    public ProfileEventDuration(string name, long timestamp, char phase)
    {
        this.Category = "function";
        this.Name = name;
        this.Phase = phase;
        this.Timestamp = timestamp;
        this.ThreadId = (uint)Thread.CurrentThread.ManagedThreadId;
    }

    public override void Serialize(ref Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        {
            writer.WriteString("cat", this.Category);
            writer.WriteString("name", this.Name);
            writer.WriteString("ph", this.Phase.ToString());
            writer.WriteNumber("ts", this.Timestamp);
            writer.WriteNumber("pid", 1);
            writer.WriteNumber("tid", this.ThreadId);
        }
        writer.WriteEndObject();
    }
}

public sealed class ProfileEventComplete : ProfileEvent
{
    public string Category { get; }
    public string Name { get; }
    public long Timestamp { get; }
    public long Duration { get; }
    public uint ThreadId { get; }

    public ProfileEventComplete(string name, long timestamp, long duration)
    {
        this.Category = "function";
        this.Name = name;
        this.Duration = duration;
        this.Timestamp = timestamp;
        this.ThreadId = (uint)Thread.CurrentThread.ManagedThreadId;
    }

    public override void Serialize(ref Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        {
            writer.WriteString("cat", this.Category);
            writer.WriteString("name", this.Name);
            writer.WriteNumber("dur", this.Duration);
            writer.WriteNumber("ts", this.Timestamp);
            writer.WriteString("ph", "X");
            writer.WriteNumber("pid", 1);
            writer.WriteNumber("tid", this.ThreadId);
        }
        writer.WriteEndObject();
    }
}

public sealed class ProfileEventInstant : ProfileEvent
{
    public string Name { get; }
    public long Timestamp { get; }
    public uint ThreadId { get; }

    public ProfileEventInstant(string name, long timestamp)
    {
        this.Name = name;
        this.Timestamp = timestamp;
        this.ThreadId = (uint)Thread.CurrentThread.ManagedThreadId;
    }

    public override void Serialize(ref Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        {
            writer.WriteString("name", this.Name);
            writer.WriteString("s", "p");
            writer.WriteString("ph", "I");
            writer.WriteNumber("ts", this.Timestamp);
            writer.WriteNumber("pid", 1);
            writer.WriteNumber("tid", this.ThreadId);
        }
        writer.WriteEndObject();
    }
}


public sealed class ProfilerEventScope : IDisposable
{
    private readonly string m_Name;

    internal ProfilerEventScope(string name)
    {
        this.m_Name = name;
        Profiler.BeginTrace(this.m_Name);
    }

    public void Dispose()
    {
        Profiler.EndTrace(this.m_Name);
    }
}

public static class Profiler
{
    private static readonly Stopwatch s_Stopwatch = Stopwatch.StartNew();

    // TODO: Verify performance against preallocated List<>
    private static readonly LinkedList<ProfileEvent> s_Events = new();

    internal static void AddEvent(ProfileEvent e)
    {
        s_Events.AddLast(e);
    }

    public static long ElapsedMicroseconds => s_Stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency;

    public static void BeginTrace(string name)
    {
        Profiler.AddEvent(new ProfileEventDuration(
            name,
            Profiler.ElapsedMicroseconds,
            'B'));
    }

    public static void EndTrace(string name)
    {
        Profiler.AddEvent(new ProfileEventDuration(
            name,
            Profiler.ElapsedMicroseconds,
            'E'));
    }

    public static ProfilerEventScope Profile(string name)
    {
        return new ProfilerEventScope(name);
    }

    public static ProfilerEventScope Function([CallerMemberName] string name = "")
    {
        return new ProfilerEventScope(name);
    }

    private static readonly JsonWriterOptions s_JsonWriterOptions = new()
    {
        Indented = true,
    };

    public static void Serialize(Stream stream)
    {
        var writer = new Utf8JsonWriter(stream, s_JsonWriterOptions);

        writer.WriteStartObject();
        {
            writer.WritePropertyName("traceEvents");
            writer.WriteStartArray();
            {
                foreach (var e in Profiler.s_Events)
                {
                    e.Serialize(ref writer);
                }
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();

        writer.Dispose();
    }
}
