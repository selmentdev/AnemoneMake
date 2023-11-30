// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using Anemone.Base.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Anemone.Framework;

public sealed class RulesRegistry
{
    private readonly Dictionary<string, TargetDescriptor> m_Targets = new();
    private readonly Dictionary<string, ModuleDescriptor> m_Modules = new();

    public IReadOnlyDictionary<string, TargetDescriptor> Targets => this.m_Targets;
    public IReadOnlyDictionary<string, ModuleDescriptor> Modules => this.m_Modules;

    private RulesRegistry(IEnumerable<Type> types)
    {
        using var scope = Profiler.Function();

        foreach (var type in types)
        {
            if (type is { IsAbstract: false, IsClass: true } && type.IsSubclassOf(typeof(TargetRules)))
            {
                var targetRulesAttribute = type.GetCustomAttribute<TargetRulesAttribute>();

                if (targetRulesAttribute != null)
                {
                    this.m_Targets.Add(type.Name, new TargetDescriptor(type, targetRulesAttribute));
                }
            }
            else if (type is { IsAbstract: false, IsClass: true } && type.IsSubclassOf(typeof(ModuleRules)))
            {
                var moduleRulesAttribute = type.GetCustomAttribute<ModuleRulesAttribute>();

                if (moduleRulesAttribute != null)
                {
                    this.m_Modules.Add(type.Name, new ModuleDescriptor(type, moduleRulesAttribute));
                }
            }
        }
    }

    public static RulesRegistry FromAssembly(Assembly assembly)
    {
        return new RulesRegistry(assembly.DefinedTypes);
    }

    public static RulesRegistry FromTypes(IEnumerable<Type> types)
    {
        return new RulesRegistry(types);
    }

    public static RulesRegistry FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        return new RulesRegistry(assemblies.SelectMany(assembly => assembly.DefinedTypes));
    }

    public static RulesRegistry FromTypes(params Type[] types)
    {
        return RulesRegistry.FromTypes((IEnumerable<Type>)types);
    }

    public static RulesRegistry FromAssemblies(params Assembly[] assemblies)
    {
        return RulesRegistry.FromAssemblies((IEnumerable<Assembly>)assemblies);
    }
}
