// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Anemone.Base;

public sealed class Config
{
    private readonly JsonDocument m_Document;

    private static readonly JsonDocumentOptions s_DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private Config(JsonDocument document)
    {
        this.m_Document = document;
    }

    public static Config Load(Stream stream)
    {
        return new(JsonDocument.Parse(stream, s_DocumentOptions));
    }

    public static Config Load(FileInfo file)
    {
        using var stream = file.OpenRead();
        return new(JsonDocument.Parse(stream, s_DocumentOptions));
    }

    public static Config Load(ReadOnlyMemory<byte> memory)
    {
        return new(JsonDocument.Parse(memory, s_DocumentOptions));
    }

    private static readonly JsonSerializerOptions s_SerializationOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private bool QueryImpl(string path, out JsonElement result)
    {
        var current = this.m_Document.RootElement;

        foreach (var part in path.Split('/'))
        {
            if (current.TryGetProperty(part, out var next))
            {
                current = next;
            }
            else
            {
                result = default;
                return false;
            }
        }

        result = current;
        return true;
    }

    public bool Query<T>(string path, out T? result)
    {
        if (this.QueryImpl(path, out var element))
        {
            result = element.Deserialize<T>(s_SerializationOptions);
            return true;
        }

        result = default;
        return false;
    }

    public void Map<T>(T instance)
    {
        var type = typeof(T);

        foreach (var property in type.GetProperties())
        {
            var mapping = property.GetCustomAttribute<ConfigSectionAttribute>();
            if (mapping != null)
            {
                var path = mapping.Path;

                if (this.QueryImpl(path, out var element))
                {
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        property.SetValue(instance, element.GetString());
                    }
                    else if (element.ValueKind == JsonValueKind.Number)
                    {
                        property.SetValue(instance, element.GetInt32());
                    }
                    else if (element.ValueKind == JsonValueKind.True)
                    {
                        property.SetValue(instance, true);
                    }
                    else if (element.ValueKind == JsonValueKind.False)
                    {
                        property.SetValue(instance, false);
                    }
                    else if (element.ValueKind == JsonValueKind.Array)
                    {
                        var array = element.EnumerateArray().ToArray();
                        property.SetValue(instance, array);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported value kind: {element.ValueKind}");
                    }
                }
            }
        }
    }
}
