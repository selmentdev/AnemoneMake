// Copyright (c) 2023, Karol Grzybowski
// SPDX-License-Identifier: Apache-2.0

using System.IO;

namespace Anemone.Framework.Sdks.Qt;

public sealed class QtSdkRules : SdkRules
{
    internal QtSdkRules(QtSdkInstance instance, TargetToolchain toolchain)
    {
        this.m_MOC = new CodeGeneratorDescriptor(instance.Platform, instance.Architecture, toolchain, "QT_MOC")
        {
            Executable = Path.Combine(instance.Root, "bin", "moc.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.h", "*.hpp", "*.c", "*.cpp", "*.hxx", "*.cxx", "*.cc", "*.hh", },
            OutputExtension = ".cxx",
            SourceDirectory = "QtMoc",
            OutputDirectory = "QtMoc",
            RequiresCompilation = true,
            RequiresInclude = true,
        };

        this.m_UIC = new CodeGeneratorDescriptor(instance.Platform, instance.Architecture, toolchain, "QT_UIC")
        {
            Executable = Path.Combine(instance.Root, "bin", "uic.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.ui", },
            OutputExtension = ".h",
            SourceDirectory = "QtUic",
            OutputDirectory = "QtUic",
            RequiresCompilation = false,
            RequiresInclude = true,
        };

        this.m_RCC = new CodeGeneratorDescriptor(instance.Platform, instance.Architecture, toolchain, "QT_RCC")
        {
            Executable = Path.Combine(instance.Root, "bin", "rcc.exe"),
            AllowDistribution = false,
            InputPatterns = new[] { "*.qrc", },
            OutputExtension = ".cpp",
            SourceDirectory = "QtRcc",
            OutputDirectory = "QtRcc",
            RequiresCompilation = true,
            RequiresInclude = false,
        };

        this.CodeGenerators = new CodeGeneratorDescriptor[]
        {
            this.m_MOC,
            this.m_UIC,
            this.m_RCC,
        };

        this.IncludePaths = new[]
        {
            Path.Combine(instance.Root, "include"),
        };

        this.LibraryPaths = new[]
        {
            Path.Combine(instance.Root, "lib"),
        };
    }

    public CodeGeneratorRules[] CreateGenerators(ResolvedModule module)
    {
        return new CodeGeneratorRules[]
        {
            new QtMocGeneratorRules(module, this.m_MOC),
            new QtUicGeneratorRules(module, this.m_UIC),
            new QtRccGeneratorRules(module, this.m_RCC),
        };
    }

    private readonly CodeGeneratorDescriptor m_MOC;
    private readonly CodeGeneratorDescriptor m_UIC;
    private readonly CodeGeneratorDescriptor m_RCC;

    public CodeGeneratorDescriptor[] CodeGenerators { get; }
}
