// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework.Tests;

#if false
// Sadly, project support disabled us from getting these tests running. But hey, they were!
// TODO: Check MSTest support for custom data files.

[TestClass]
public class TestDependencyCycles
{
    [ModuleRules(ModuleKind.GameApplication)]
    private class AppWithCycle : ModuleRules
    {
        public AppWithCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibAWithCycle");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    private class LibAWithCycle : ModuleRules
    {
        public LibAWithCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibCWithCycle");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    private class LibBWithCycle : ModuleRules
    {
        public LibBWithCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibAWithCycle");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    private class LibCWithCycle : ModuleRules
    {
        public LibCWithCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibBWithCycle");
        }
    }

    [TargetRules]
    private class TargetWithCycle : TargetRules
    {
        public TargetWithCycle(ResolveContext context)
            : base(context)
        {
            this.StartupModule = "AppWithCycle";
        }
    }

    [TestMethod]
    public void ExpectCircularDependency()
    {
        var rules = RulesRegistry.FromTypes(new[]
        {
            typeof(AppWithCycle),
            typeof(LibAWithCycle),
            typeof(LibBWithCycle),
            typeof(LibCWithCycle),
            typeof(TargetWithCycle)
        });


        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets["TargetWithCycle"].Create(context);

        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        Assert.ThrowsException<CircularDependencyException>(() =>
        {
            var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        });
    }

    [ModuleRules(ModuleKind.GameApplication)]
    public class AppNoCycle : ModuleRules
    {
        public AppNoCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibDNoCycle");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    public class LibANoCycle : ModuleRules
    {
        public LibANoCycle(TargetRules target)
            : base(target)
        {
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    public class LibBNoCycle : ModuleRules
    {
        public LibBNoCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibANoCycle");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    public class LibCNoCycle : ModuleRules
    {
        public LibCNoCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibANoCycle");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    public class LibDNoCycle : ModuleRules
    {
        public LibDNoCycle(TargetRules target)
            : base(target)
        {
            this.PrivateDependencies.Add("LibBNoCycle");
            this.PrivateDependencies.Add("LibCNoCycle");
        }
    }

    [TargetRules]
    public class TargetNoCycle : TargetRules
    {
        public TargetNoCycle(ResolveContext context)
            : base(context)
        {
            this.StartupModule = "AppNoCycle";
        }
    }

    [TestMethod]
    public void ExpectNoCircularDependency()
    {
        var rules = RulesRegistry.FromTypes(new[]
        {
            typeof(AppNoCycle),
            typeof(LibANoCycle),
            typeof(LibBNoCycle),
            typeof(LibCNoCycle),
            typeof(LibDNoCycle),
            typeof(TargetNoCycle)
        });


        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets["TargetNoCycle"].Create(context);

        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        _ = new ResolvedTarget(context.PlatformRules, target, modules);
    }
}
#endif
