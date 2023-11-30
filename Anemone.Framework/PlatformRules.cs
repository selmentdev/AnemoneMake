// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;

namespace Anemone.Framework;

/// <summary>
///     Represents platform specific set of SDKs and rules required to build target.
/// </summary>
public abstract class PlatformRules
{
    /// <summary>
    ///     Creates new instance of <see cref="PlatformRules" />.
    /// </summary>
    /// <param name="descriptor">
    ///     A platform descriptor.
    /// </param>
    protected PlatformRules(PlatformDescriptor descriptor)
    {
        this.Descriptor = descriptor;
    }

    private PlatformDescriptor Descriptor { get; }

    /// <summary>
    ///     Gets target platform.
    /// </summary>
    public TargetPlatform Platform => this.Descriptor.Platform;

    /// <summary>
    ///     Gets target architecture.
    /// </summary>
    public TargetArchitecture Architecture => this.Descriptor.Architecture;

    /// <summary>
    ///     Gets target toolchain.
    /// </summary>
    public TargetToolchain Toolchain => this.Descriptor.Toolchain;

    /// <summary>
    ///     Gets custom platform rules moniker.
    /// </summary>
    public string Moniker => this.Descriptor.Moniker;

    /// <summary>
    ///     Gets SDKs supported by current platform.
    /// </summary>
    public IReadOnlyCollection<SdkRules> Sdks => this.m_Sdks;

    protected readonly List<SdkRules> m_Sdks = new();

    /// <summary>
    ///     Creates compiler for given build context.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <returns>
    ///     The compiler rules.
    /// </returns>
    public abstract CompilerRules CreateCompiler(ResolvedModule module);

    /// <summary>
    ///     Creates assembler for given build context.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <returns>
    ///     The assembler rules.
    /// </returns>
    public virtual CompilerRules? CreateAssembler(ResolvedModule module)
    {
        // Default implementation is to return null. Not all platforms have support for assembler.
        return null;
    }

    /// <summary>
    ///     Creates linker for given build context.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <returns>
    ///     The linker rules.
    /// </returns>
    public abstract LinkerRules CreateLinker(ResolvedModule module);

    /// <summary>
    ///     Creates code generators for given build context.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <returns>
    ///     The code generators.
    /// </returns>
    public abstract IReadOnlyCollection<CodeGeneratorRules> CreateCodeGenerators(ResolvedModule module);

    /// <summary>
    ///     Creates resource compilers for given build context.
    /// </summary>
    /// <param name="module">
    ///     A resolved module.
    /// </param>
    /// <returns>
    ///     The resource compilers.
    /// </returns>
    public abstract IReadOnlyCollection<ResourceCompilerRules> CreateResourceCompilers(ResolvedModule module);

    /// <summary>
    ///     Gets descriptors of all supported compilers.
    /// </summary>
    public IEnumerable<CompilerDescriptor> Compilers { get; protected init; } = Enumerable.Empty<CompilerDescriptor>();

    /// <summary>
    ///     Gets descriptors of all supported assemblers.
    /// </summary>
    public IEnumerable<CompilerDescriptor> Assemblers { get; protected init; } = Enumerable.Empty<CompilerDescriptor>();

    /// <summary>
    ///     Gets descriptors of all supported linkers.
    /// </summary>
    public IEnumerable<LinkerDescriptor> Linkers { get; protected init; } = Enumerable.Empty<LinkerDescriptor>();

    /// <summary>
    ///     Gets descriptors of all supported code generators.
    /// </summary>
    public IEnumerable<CodeGeneratorDescriptor> CodeGenerators { get; protected init; } = Enumerable.Empty<CodeGeneratorDescriptor>();

    /// <summary>
    ///     Gets descriptors of all supported resource compilers.
    /// </summary>
    public IEnumerable<ResourceCompilerDescriptor> ResourceCompilers { get; protected init; } = Enumerable.Empty<ResourceCompilerDescriptor>();
}
