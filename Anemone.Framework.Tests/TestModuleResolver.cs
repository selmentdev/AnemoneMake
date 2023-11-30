// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

namespace Anemone.Framework.Tests;

#if false
// Sadly, project support disabled us from getting these tests running. But hey, they were!
// TODO: Check MSTest support for custom data files.

[TestClass]
public class TestDependencyResolving
{
    [ModuleRules(ModuleKind.RuntimeLibrary)]
    private class RootModule : ModuleRules
    {
        public RootModule(TargetRules target)
            : base(target)
        {
            var mockTarget = target as MetaMockTarget;
            Assert.IsNotNull(mockTarget);

            switch (mockTarget.TrunkKind)
            {
                case ModuleReferenceType.Public:
                    {
                        this.PublicDependencies.Add(nameof(TrunkModule));
                        break;
                    }

                case ModuleReferenceType.Private:
                    {
                        this.PrivateDependencies.Add(nameof(TrunkModule));
                        break;
                    }

                case ModuleReferenceType.Interface:
                    {
                        this.InterfaceDependencies.Add(nameof(TrunkModule));
                        break;
                    }
            }

            this.PublicDefines.Add("ROOT_PUBLIC_DEFINE=1");
            this.PrivateDefines.Add("ROOT_PRIVATE_DEFINE=1");
            this.InterfaceDefines.Add("ROOT_INTERFACE_DEFINE=1");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    private class TrunkModule : ModuleRules
    {
        public TrunkModule(TargetRules target)
            : base(target)
        {
            var mockTarget = target as MetaMockTarget;
            Assert.IsNotNull(mockTarget);

            switch (mockTarget.LeafKind)
            {
                case ModuleReferenceType.Public:
                    {
                        this.PublicDependencies.Add(nameof(LeafModule));
                        break;
                    }

                case ModuleReferenceType.Private:
                    {
                        this.PrivateDependencies.Add(nameof(LeafModule));
                        break;
                    }

                case ModuleReferenceType.Interface:
                    {
                        this.InterfaceDependencies.Add(nameof(LeafModule));
                        break;
                    }
            }

            this.PublicDefines.Add("TRUNK_PUBLIC_DEFINE=1");
            this.PrivateDefines.Add("TRUNK_PRIVATE_DEFINE=1");
            this.InterfaceDefines.Add("TRUNK_INTERFACE_DEFINE=1");
        }
    }

    [ModuleRules(ModuleKind.RuntimeLibrary)]
    private class LeafModule : ModuleRules
    {
        public LeafModule(TargetRules target)
            : base(target)
        {
            this.PublicDefines.Add("LEAF_PUBLIC_DEFINE=1");
            this.PrivateDefines.Add("LEAF_PRIVATE_DEFINE=1");
            this.InterfaceDefines.Add("LEAF_INTERFACE_DEFINE=1");
        }
    }

    private abstract class MetaMockTarget : TargetRules
    {
        public ModuleReferenceType TrunkKind { get; }
        public ModuleReferenceType LeafKind { get; }

        protected MetaMockTarget(ResolveContext context, ModuleReferenceType trunkKind, ModuleReferenceType leafKind)
            : base(context)
        {
            this.TrunkKind = trunkKind;
            this.LeafKind = leafKind;
            this.StartupModule = "RootModule";
        }
    }

    [TargetRules]
    private class MockTarget_PrivatePrivate : MetaMockTarget
    {
        public MockTarget_PrivatePrivate(ResolveContext context)
            : base(context, ModuleReferenceType.Private, ModuleReferenceType.Private)
        {
        }
    }

    [TestMethod]
    public void TestPrivatePrivate()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_PrivatePrivate), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_PrivatePrivate)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_PrivatePublic : MetaMockTarget
    {
        public MockTarget_PrivatePublic(ResolveContext context)
            : base(context, ModuleReferenceType.Private, ModuleReferenceType.Public)
        {
        }
    }

    [TestMethod]
    public void TestPrivatePublic()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_PrivatePublic), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_PrivatePublic)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_PrivateInterface : MetaMockTarget
    {
        public MockTarget_PrivateInterface(ResolveContext context)
            : base(context, ModuleReferenceType.Private, ModuleReferenceType.Interface)
        {
        }
    }

    [TestMethod]
    public void TestPrivateInterface()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_PrivateInterface), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));

        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_PrivateInterface)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_PublicPrivate : MetaMockTarget
    {
        public MockTarget_PublicPrivate(ResolveContext context)
            : base(context, ModuleReferenceType.Public, ModuleReferenceType.Private)
        {
        }
    }

    [TestMethod]
    public void TestPublicPrivate()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_PublicPrivate), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_PublicPrivate)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_PublicPublic : MetaMockTarget
    {
        public MockTarget_PublicPublic(ResolveContext context)
            : base(context, ModuleReferenceType.Public, ModuleReferenceType.Public)
        {
        }
    }

    [TestMethod]
    public void TestPublicPublic()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_PublicPublic), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_PublicPublic)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_PublicInterface : MetaMockTarget
    {
        public MockTarget_PublicInterface(ResolveContext context)
            : base(context, ModuleReferenceType.Public, ModuleReferenceType.Interface)
        {
        }
    }

    [TestMethod]
    public void TestPublicInterface()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_PublicInterface), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_PublicInterface)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_InterfacePrivate : MetaMockTarget
    {
        public MockTarget_InterfacePrivate(ResolveContext context)
            : base(context, ModuleReferenceType.Interface, ModuleReferenceType.Private)
        {
        }
    }

    [TestMethod]
    public void TestInterfacePrivate()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_InterfacePrivate), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_InterfacePrivate)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_InterfacePublic : MetaMockTarget
    {
        public MockTarget_InterfacePublic(ResolveContext context)
            : base(context, ModuleReferenceType.Interface, ModuleReferenceType.Public)
        {
        }
    }

    [TestMethod]
    public void TestInterfacePublic()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_InterfacePublic), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_InterfacePublic)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    [TargetRules]
    private class MockTarget_InterfaceInterface : MetaMockTarget
    {
        public MockTarget_InterfaceInterface(ResolveContext context)
            : base(context, ModuleReferenceType.Interface, ModuleReferenceType.Interface)
        {
        }
    }

    [TestMethod]
    public void TestInterfaceInterface()
    {
        var rules = RulesRegistry.FromTypes(typeof(MockTarget_InterfaceInterface), typeof(RootModule), typeof(TrunkModule), typeof(LeafModule));
        var projectContext = new ProjectContext("", "", "");
        var targetContext = new TargetContext(projectContext, new MockPlatformRules(), rules.Targets.Values.Single(), rules.Modules.Values.ToArray());
        var context = new ResolveContext(targetContext, TargetConfiguration.Debug);
        var target = rules.Targets[nameof(MockTarget_InterfaceInterface)].Create(context);
        var modules = rules.Modules.Select(x => x.Value.Create(target)).ToArray();

        var graph = new ResolvedTarget(context.PlatformRules, target, modules);
        var root = graph.Nodes.First(x => x.Name == nameof(RootModule));
        var trunk = graph.Nodes.First(x => x.Name == nameof(TrunkModule));
        var leaf = graph.Nodes.First(x => x.Name == nameof(LeafModule));

        PrintDependencies(root);
        PrintDependencies(trunk);
        PrintDependencies(leaf);

#region Defines
        Assert.IsTrue(root.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsTrue(root.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(root.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsTrue(trunk.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.Defines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsTrue(leaf.Defines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.Defines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion

#region InterfaceDefines
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(root.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(root.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(trunk.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(trunk.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));

        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("ROOT_INTERFACE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_PRIVATE_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("TRUNK_INTERFACE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_PUBLIC_DEFINE=1"));
        Assert.IsFalse(leaf.InterfaceDefines.Contains("LEAF_PRIVATE_DEFINE=1"));
        Assert.IsTrue(leaf.InterfaceDefines.Contains("LEAF_INTERFACE_DEFINE=1"));
#endregion
    }

    private static void PrintDependencies(ResolvedModule node)
    {
#if false
        Trace.WriteLine($@"{node.Name}");
        Trace.WriteLine($@"ResolvedDefines:");

        foreach (var define in node.Defines)
        {
            Trace.WriteLine($@"    `{define}`");
        }

        Trace.WriteLine($@"ResolvedInterfaceDefines:");

        foreach (var define in node.InterfaceDefines)
        {
            Trace.WriteLine($@"    `{define}`");
        }
#endif
    }
}
#endif
