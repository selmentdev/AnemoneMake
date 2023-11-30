// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;

namespace Anemone.Framework;

/// <summary>
///     Implements Graphviz Dot target exporter.
/// </summary>
public sealed class GraphvizDotTargetExporter : TargetExporter
{
    private static string GetRelationStyle(ModuleReferenceType kind)
    {
        return kind switch
        {
            ModuleReferenceType.Public => "solid",
            ModuleReferenceType.Private => "dotted",
            ModuleReferenceType.Interface => "dashed",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind.ToString())
        };
    }

    /// <inheritdoc />
    public override void Export(TextWriter writer, ResolvedTarget target)
    {
        writer.WriteLine("digraph G {");
        writer.WriteLine("    rankdir=LR;");
        writer.WriteLine("    node [shape=box];");

        var nodeIdMapping = new Dictionary<ResolvedModule, int>();

        foreach (var node in target.Nodes)
        {
            nodeIdMapping.Add(node, nodeIdMapping.Count);

            writer.Write($@"    {nodeIdMapping[node]} [label=""{node.Name}""");

            if (target.Transitive.Contains(node))
            {
                writer.Write(@", shape=doubleoctagon");
            }

            writer.WriteLine("];");
        }

        foreach (var edge in target.Edges)
        {
            var sourceId = nodeIdMapping[edge.Key.Source];
            var targetId = nodeIdMapping[edge.Key.Target];

            var style = GetRelationStyle(edge.Value);

            writer.WriteLine($"    {sourceId} -> {targetId} [style={style}];");
        }

        writer.WriteLine("}");
    }
}
