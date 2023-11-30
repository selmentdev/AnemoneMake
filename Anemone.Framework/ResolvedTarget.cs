// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Anemone.Framework;

/// <summary>
///     Represents a resolved target.
/// </summary>
[DebuggerDisplay("{this.Rules.Name}")]
public sealed class ResolvedTarget
{
    #region Types
    /// <summary>
    ///     Represents an edge in the dependency graph.
    /// </summary>
    /// <param name="Source">
    ///     A starting point of the edge.
    /// </param>
    /// <param name="Target">
    ///     An ending point of the edge.
    /// </param>
    public record Edge(ResolvedModule Source, ResolvedModule Target);
    #endregion

    #region Properties
    public PlatformRules Platform { get; }
    /// <summary>
    ///     Gets target for this graph.
    /// </summary>
    public TargetRules Rules { get; }

    /// <summary>
    ///     Gets list of all nodes.
    /// </summary>
    public IReadOnlyList<ResolvedModule> Nodes { get; }

    /// <summary>
    ///     Gets list of all edges.
    /// </summary>
    public IReadOnlyDictionary<Edge, ModuleReferenceType> Edges { get; }

    /// <summary>
    ///     Gets list of all graph roots.
    /// </summary>
    public IReadOnlyList<ResolvedModule> Roots { get; }

    /// <summary>
    ///     Gets list of all graph nodes, sorted topologically.
    /// </summary>
    public IReadOnlyList<ResolvedModule> Sorted { get; }

    /// <summary>
    ///     Gets list of all target transitive (required) nodes, sorted topologically.
    /// </summary>
    /// <value></value>
    public IReadOnlySet<ResolvedModule> Transitive { get; }
    #endregion


    #region Constructors
    /// <summary>
    ///     Initializes a new instance of the <see cref="ResolvedTarget" /> class.
    /// </summary>
    /// <param name="platform">
    ///     A platform rules.
    /// </param>
    /// <param name="target">
    ///     A target.
    /// </param>
    /// <param name="modules">
    ///     A list of modules.
    /// </param>
    public ResolvedTarget(PlatformRules platform, TargetRules target, IReadOnlyList<ModuleRules> modules)
    {
        using var scope = Profiler.Function();

        this.Platform = platform;
        this.Rules = target;

        var lookup = this.CreateGraphNodes(modules);

        this.Nodes = lookup.Values.ToArray();


        //
        // Import properties from module rules.
        //

        foreach (var node in this.Nodes)
        {
            node.ImportProperties(lookup);
        }


        //
        // Import dependencies for each module.
        // This creates edges in the graph.
        //

        this.Edges = CreateGraph(modules, lookup);


        //
        // Detect cycles in the graph. This ensures that graph is acyclic.
        //

        DetectCycles(this.Nodes);


        //
        // Discover set of graph roots.
        //

        this.Roots = GetGraphRoots(this.Nodes);


        //
        // This graph is acyclic, so we can sort it.
        //

        this.Sorted = TopologicalSort(this.Roots, this.Nodes);


        //
        // Resolve dependencies.
        //

        this.ResolveDependencies();


        //
        // Create set of transitive nodes for current target.
        //

        this.Transitive = ComputeTransitiveNodesForTarget(target, lookup);
    }

    /// <summary>
    ///     Creates graph nodes by name.
    /// </summary>
    /// <param name="modules">
    ///     A list of modules.
    /// </param>
    /// <returns>
    ///     The created graph nodes.
    /// </returns>
    private Dictionary<string, ResolvedModule> CreateGraphNodes(IReadOnlyList<ModuleRules> modules)
    {
        using var scope = Profiler.Function();

        //
        // Create graph nodes.
        //

        var lookup = new Dictionary<string, ResolvedModule>(modules.Count);

        foreach (var module in modules)
        {
            var node = new ResolvedModule(this, module);
            lookup.Add(module.Name, node);
        }

        return lookup;
    }
    #endregion

    #region Graph
    /// <summary>
    ///     Creates a graph from a list of modules.
    /// </summary>
    /// <param name="modules">
    ///     A list of modules.
    /// </param>
    /// <param name="nodes">
    ///     A lookup of all nodes.
    /// </param>
    /// <returns>
    ///     The collection of edges in dependency graph.
    /// </returns>
    private static IReadOnlyDictionary<Edge, ModuleReferenceType> CreateGraph(
         IReadOnlyList<ModuleRules> modules,
         Dictionary<string, ResolvedModule> nodes)
    {
        using var scope = Profiler.Function();

        var edges = new Dictionary<Edge, ModuleReferenceType>();

        foreach (var module in modules)
        {
            var node = nodes[module.Name];

            //
            // Import public dependencies.
            //

            foreach (var dependency in module.PublicDependencies)
            {
                if (nodes.TryGetValue(dependency, out var dependencyNode))
                {
                    node.AddReference(dependencyNode, ModuleReferenceType.Public, edges);
                }
                else
                {
                    throw new Exception($"Module '{module.Name}' has public dependency on '{dependency}', but it is not found.");
                }
            }


            //
            // Import private dependencies.
            //

            foreach (var dependency in module.PrivateDependencies)
            {
                if (nodes.TryGetValue(dependency, out var dependencyNode))
                {
                    node.AddReference(dependencyNode, ModuleReferenceType.Private, edges);
                }
                else
                {
                    throw new Exception($"Module '{module.Name}' has private dependency on '{dependency}', but it is not found.");
                }
            }


            //
            // Import interface dependencies.
            //

            foreach (var dependency in module.InterfaceDependencies)
            {
                if (nodes.TryGetValue(dependency, out var dependencyNode))
                {
                    node.AddReference(dependencyNode, ModuleReferenceType.Interface, edges);
                }
                else
                {
                    throw new Exception($"Module '{module.Name}' has interface dependency on '{dependency}', but it is not found.");
                }
            }
        }

        return edges;
    }

    /// <summary>
    ///     Computes set of transitive nodes for current target.
    /// </summary>
    /// <param name="target">
    ///     A target rules.
    /// </param>
    /// <param name="nodes">
    ///     A lookup of all nodes.
    /// </param>
    /// <returns>
    ///     A set of transitive nodes.
    /// </returns>
    private static IReadOnlySet<ResolvedModule> ComputeTransitiveNodesForTarget(
        TargetRules target,
        IReadOnlyDictionary<string, ResolvedModule> nodes)
    {
        using var scope = Profiler.Function();

        var transitive = new HashSet<ResolvedModule>();

        //
        // Import startup module.
        //

        if (target.StartupModule != null)
        {
            if (nodes.TryGetValue(target.StartupModule, out var module))
            {
                GetTransitiveNodesImpl(transitive, module);
            }
            else
            {
                throw new Exception($@"Module '{target.StartupModule}' not defined");
            }
        }


        //
        // Import required modules.
        //

        foreach (var name in target.RequiredModules)
        {
            if (nodes.TryGetValue(name, out var module))
            {
                GetTransitiveNodesImpl(transitive, module);
            }
            else
            {
                throw new Exception($@"Module '{name}' not defined");
            }
        }


        //
        // Import applications and runtime dependencies.
        //

        foreach (var module in nodes.Values)
        {
            if (ModuleLinkKindExtensions.IsApplication(module.LinkKind))
            {
                //
                // Import this module as-is.
                //

                GetTransitiveNodesImpl(transitive, module);

                //
                // Import runtime dependencies to transitive set as well.
                //

                foreach (var dependency in module.RuntimeDependencies)
                {
                    GetTransitiveNodesImpl(transitive, dependency);
                }
            }
        }

        return transitive;
    }

    /// <summary>
    ///     Computes set of graph roots.
    /// </summary>
    /// <param name="nodes">
    ///     A list of nodes.
    /// </param>
    /// <returns>
    ///     A list of graph roots.
    /// </returns>
    private static IReadOnlyList<ResolvedModule> GetGraphRoots(IReadOnlyCollection<ResolvedModule> nodes)
    {
        using var scope = Profiler.Function();

        var result = new List<ResolvedModule>();

        foreach (var node in nodes)
        {
            if (node.IncomingReferences.Count == 0)
            {
                result.Add(node);
            }
        }

        return result;
    }

    /// <summary>
    ///     Formats trace of nodes in cycle.
    /// </summary>
    /// <param name="trace"></param>
    /// <returns></returns>
    private static string FormatCircularDependency(IList<ResolvedModule> trace)
    {
        var builder = new StringBuilder();
        builder.AppendLine();

        for (var i = trace.Count - 1; i >= 0; i--)
        {
            if (i != 0)
            {
                builder.Append(trace[i].Name).Append(" -> ");
            }
            else
            {
                builder.Append(trace[i].Name);
            }
        }

        return builder.ToString();
    }

    private enum DetectCycleState
    {
        InProgress,
        Processed,
    }

    private static bool DetectCyclesVisit(
        ResolvedModule node,
        Dictionary<ResolvedModule, DetectCycleState> states,
        List<ResolvedModule> trace)
    {
        trace.Add(node);

        //
        // Mark node as being processed.
        //

        states[node] = DetectCycleState.InProgress;

        foreach (var (reference, _) in node.OutgoingReferences)
        {
            if (states.TryGetValue(reference, out var state))
            {
                if (state == DetectCycleState.InProgress)
                {
                    if (node.Equals(reference))
                    {
                        //
                        // Found a self-reference cycle.
                        //

                        throw new CircularDependencyException(FormatCircularDependency(new[] { node, node }));
                    }

                    return false;
                }
            }
            else
            {
                //
                // Visit dependency recursively.
                //

                var result = DetectCyclesVisit(reference, states, trace);

                if (!result)
                {
                    if (trace[0].Equals(node))
                    {
                        //
                        // Found a cycle.
                        //

                        trace.Add(node);

                        throw new CircularDependencyException(FormatCircularDependency(trace));
                    }

                    return false;
                }
            }
        }

        //
        // Mark node as processed.
        //

        states[node] = DetectCycleState.Processed;
        trace.RemoveAt(trace.Count - 1);
        return true;
    }

    private static void DetectCycles(
        IReadOnlyCollection<ResolvedModule> nodes)
    {
        using var scope = Profiler.Function();

        var states = new Dictionary<ResolvedModule, DetectCycleState>();

        foreach (var node in nodes)
        {
            if (!states.TryGetValue(node, out var state))
            {
                DetectCyclesVisit(node, states, new List<ResolvedModule>());
            }
            else
            {
                if (state != DetectCycleState.Processed)
                {
                    // This node should be visited but it was not.
                    throw new InvalidOperationException();
                }
            }
        }
    }

    /// <summary>
    ///     Sorts nodes in topological order.
    /// </summary>
    /// <param name="graphRoots">
    ///     A list of graph roots.
    /// </param>
    /// <param name="graphNodes">
    ///     A list of all graph nodes.
    /// </param>
    /// <returns>
    ///     The list of nodes sorted in topological order.
    /// </returns>
    private static IReadOnlyList<ResolvedModule> TopologicalSort(
        IReadOnlyCollection<ResolvedModule> graphRoots,
        IReadOnlyCollection<ResolvedModule> graphNodes)
    {
        using var scope = Profiler.Function();

        var result = new List<ResolvedModule>(graphNodes.Count);
        var partialRoots = new Queue<ResolvedModule>(graphNodes.Count);
        var inDegree = graphNodes.ToDictionary(n => n, n => n.IncomingReferences.Count);

        foreach (var root in graphRoots)
        {
            partialRoots.Enqueue(root);
        }

        while (partialRoots.Count != 0)
        {
            var partialRoot = partialRoots.Dequeue();

            result.Add(partialRoot);

            foreach (var (reference, _) in partialRoot.OutgoingReferences)
            {
                if (--inDegree[reference] == 0)
                {
                    partialRoots.Enqueue(reference);
                }
            }
        }

        if (result.Count != graphNodes.Count)
        {
            throw new CircularDependencyException("Graph is cyclic");
        }

        result.Reverse();

        return result;
    }

    private static void GetTransitiveNodesImpl(HashSet<ResolvedModule> result, ResolvedModule node)
    {
        if (result.Add(node))
        {
            foreach (var (reference, _) in node.OutgoingReferences)
            {
                GetTransitiveNodesImpl(result, reference);
            }
        }
    }

    /// <summary>
    ///     Gets strongly connected components of the graph.
    /// </summary>
    /// <remarks>
    ///     This method uses Tarjan's Algorithm implementation of Strongly Connected Components.
    ///     https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
    /// </remarks>
    public List<List<ResolvedModule>> StronglyConnectedComponents()
    {
        using var scope = Profiler.Function();

        const int unassigned = -1;

        var index = 0;
        var stack = new Stack<int>();
        var indices = new int[this.Nodes.Count];
        var lowlinks = new int[this.Nodes.Count];
        var onStack = new bool[this.Nodes.Count];
        var nodes = this.Nodes.ToArray();
        var result = new List<List<ResolvedModule>>();

        Array.Fill(indices, unassigned);

        for (var i = 0; i < this.Nodes.Count; i++)
        {
            if (indices[i] == unassigned)
            {
                StrongConnect(i);
            }
        }

        void StrongConnect(int current)
        {
            stack.Push(current);
            onStack[current] = true;
            indices[current] = index;
            lowlinks[current] = index;

            ++index;

            var node = nodes[current];

            foreach (var dependency in node.OutgoingReferences)
            {
                var dependencyId = Array.IndexOf(nodes, dependency.Key);

                if (indices[dependencyId] == unassigned)
                {
                    StrongConnect(dependencyId);

                    lowlinks[current] = Math.Min(lowlinks[current], lowlinks[dependencyId]);
                }
                else if (onStack[dependencyId])
                {
                    lowlinks[current] = Math.Min(lowlinks[current], indices[dependencyId]);
                }
            }

            if (lowlinks[current] == indices[current])
            {
                var scc = new List<ResolvedModule>();

                while (stack.Count > 0)
                {
                    var nodeId = stack.Pop();
                    onStack[nodeId] = false;
                    scc.Add(nodes[nodeId]);

                    if (nodeId == current)
                    {
                        break;
                    }
                }

                result.Add(scc);
            }
        }

        return result;
    }
    #endregion

    #region Dependency Resolving
    /// <summary>
    ///     Resolves module dependencies for given graph.
    /// </summary>
    private void ResolveDependencies()
    {
        using var scope = Profiler.Function();

        //
        // Resolve dependencies.
        //
        // Graph is traversed topologically, so we can resolve dependencies in any order.
        //

        foreach (var current in this.Sorted)
        {
            foreach (var (dependency, kind) in current.OutgoingReferences)
            {
                ResolveDependencies(current, dependency, kind);
            }
        }
    }

    /// <summary>
    ///     Resolve properties between two modules.
    /// </summary>
    /// <param name="current">
    ///     A module that is being resolved.
    /// </param>
    /// <param name="dependency">
    ///     A module that is being resolved against.
    /// </param>
    /// <param name="kind">
    ///     A kind of directed dependency between the two modules.
    /// </param>
    private static void ResolveDependencies(
        ResolvedModule current,
        ResolvedModule dependency,
        ModuleReferenceType kind)
    {
        //
        // Import runtime dependencies.
        //

        foreach (var item in dependency.RuntimeDependencies)
        {
            current.RuntimeDependencies.Add(item);
        }

        if (kind is ModuleReferenceType.Public or ModuleReferenceType.Interface)
        {
            //
            // Forward interface properties.
            //

            foreach (var item in dependency.InterfaceDefines)
            {
                current.InterfaceDefines.Add(item);
            }

            foreach (var item in dependency.InterfaceLibraries)
            {
                current.InterfaceLibraries.Add(item);
            }

            foreach (var item in dependency.InterfaceIncludePaths)
            {
                current.InterfaceIncludePaths.Add(item);
            }

            foreach (var item in dependency.InterfaceLibraryPaths)
            {
                current.InterfaceLibraryPaths.Add(item);
            }

            foreach (var item in dependency.InterfaceDependencies)
            {
                current.InterfaceDependencies.Add(item);
            }
        }

        if (kind is ModuleReferenceType.Public or ModuleReferenceType.Private)
        {
            //
            // Import interface properties.
            //

            foreach (var item in dependency.InterfaceDefines)
            {
                current.Defines.Add(item);
            }

            foreach (var item in dependency.InterfaceLibraries)
            {
                current.Libraries.Add(item);
            }

            foreach (var item in dependency.InterfaceIncludePaths)
            {
                current.IncludePaths.Add(item);
            }

            foreach (var item in dependency.InterfaceLibraryPaths)
            {
                current.LibraryPaths.Add(item);
            }

            foreach (var item in dependency.InterfaceDependencies)
            {
                current.Dependencies.Add(item);
            }
        }

        if (dependency.LinkKind is ModuleLinkKind.StaticLibrary or ModuleLinkKind.ImportedLibrary)
        {
            //
            // Forward properties required to link modules.
            //
            // Compile dependencies were resolved in previous step so they can be skipped here.
            //

            foreach (var item in dependency.Libraries)
            {
                current.Libraries.Add(item);
            }

            foreach (var item in dependency.LibraryPaths)
            {
                current.LibraryPaths.Add(item);
            }

            foreach (var item in dependency.Dependencies)
            {
                current.Dependencies.Add(item);
            }
        }
    }
    #endregion
}
