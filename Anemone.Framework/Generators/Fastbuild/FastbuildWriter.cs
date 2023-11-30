// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Anemone.Framework.Generators.Fastbuild;

internal sealed class FastbuildWriter : IDisposable
{
    private readonly StreamWriter m_Writer;
    private int m_Level;
    private bool m_TabsPending;
    private readonly string m_TabString;

    public const string DefaultTabString = "\t";

    public FastbuildWriter(string path)
    {
        this.m_Writer = new StreamWriter(path);
        this.m_TabString = DefaultTabString;
        this.m_TabsPending = false;
        this.m_Level = 0;

        this.m_Writer.WriteLine(@"// This file was generated by machine.");
        this.m_Writer.WriteLine(@"// Do not edit this file - all changes will be lost.");
        this.m_Writer.WriteLine();
        this.m_Writer.WriteLine(@"#once");
        this.m_Writer.WriteLine();
    }

    public void Dispose()
    {
        Debug.Assert(this.m_Level == 0);
        this.m_Writer.WriteLine(@"// end of file");
        this.m_Writer.Dispose();
    }

    public int Level
    {
        get => this.m_Level;
        set
        {
            Debug.Assert(value >= 0);
            this.m_Level = Math.Max(value, 0);
        }
    }

    public void Flush()
    {
        this.m_Writer.Flush();
    }

    private void FlushTabs()
    {
        if (this.m_TabsPending)
        {
            for (var i = 0; i < this.m_Level; ++i)
            {
                this.m_Writer.Write(this.m_TabString);
            }

            this.m_TabsPending = false;
        }
    }

    public void Indent()
    {
        ++this.m_Level;
    }

    public void Unindent()
    {
        Debug.Assert(this.m_Level > 0);
        --this.m_Level;
    }

    public void Write(string s)
    {
        this.FlushTabs();
        this.m_Writer.Write(s);
    }

    public void WriteLineBase(string s)
    {
        this.m_Writer.WriteLine(s);
    }

    public void WriteLine(string s)
    {
        this.FlushTabs();
        this.m_Writer.WriteLine(s);
        this.m_TabsPending = true;
    }

    public void WriteLine()
    {
        this.FlushTabs();
        this.m_Writer.WriteLine();
        this.m_TabsPending = true;
    }

    public void WriteLineBase()
    {
        this.m_Writer.WriteLine();
    }

    public void WriteArrayInline(IEnumerable<string> values)
    {
        this.Write("{");

        using (var e = values.GetEnumerator())
        {
            if (e.MoveNext())
            {
                this.Write($@"'{e.Current}'");

                while (e.MoveNext())
                {
                    this.Write($@", '{e.Current}'");
                }
            }
        }

        this.WriteLine("}");
    }

    public void WriteCommandList(IEnumerable<string> values)
    {
        this.Indent();
        {
            foreach (var value in values)
            {
                this.FlushTabs();
                this.m_Writer.Write("+ ' ");
                this.m_Writer.Write(value);
                this.m_Writer.WriteLine('\'');
                this.m_TabsPending = true;
            }
        }
        this.Unindent();
    }

    public void WriteStartScope()
    {
        this.WriteLine("{");
        this.Indent();
    }

    public void WriteEndScope()
    {
        this.Unindent();
        this.WriteLine("}");
    }

    public void WriteStartArray()
    {
        this.WriteLine("{");
        this.Indent();
    }

    public void WriteEndArray()
    {
        this.Unindent();
        this.WriteLine("}");
    }

    public void WriteStartStruct()
    {
        this.WriteLine("[");
        this.Indent();
    }

    public void WriteEndStruct()
    {
        this.Unindent();
        this.WriteLine("]");
    }
}