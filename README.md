# AnemoneMake

The project description system for Anemone Engine


# Usage

0. Build and publish tool to output location

```powershell
dotnet publish . --output ./Build --configuration Release
```

1. Create project manifest file

Create a file named `Project.json` in the root directory of the project.

```json
{
  "Name": "Sample Project",
  "Company": "Sample Company",
  "Copyright": "Sample Copyright",
  "Version":  "1.0.0.0",
  "Targets": [
    "SampleTarget"
  ],
  "Platforms": [
    "Windows-X64",
    "Windows-AArch64",
  ],
  "Branch": "main",
  "Generator": "FASTBuild-v1.0"
}
```

2. Create target and module manifest files

```
\- Source
  |- Sample.Target.cs
  |- SampleApplication
  | |- Module.cs
  | |- Public
  | | \- SampleApplication.h
  | \- Source
  |   \- SampleApplication.cpp
  \- SampleModule
    |- Module.cs
    |- Public
    | \- SampleModule.h
    \- Source
      \- SampleModule.cpp
```

2.1. `Source/Sample.Target.cs`

```csharp
using Anemone.Framework;

[TargetRules]
public sealed class SmapleTarget : TargetRules
{
    public SmapleTarget(ResolveContext context)
        : base(context)
    {
        this.Kind = TargetKind.Editor;
        this.StartupModule = "SampleApplication";

        this.LinkKind = TargetLinkKind.Monolithic;

        if (this.Configuration == TargetConfiguration.Shipping)
        {
            this.EnableAddressSanitizer = false;
        }
        else
        {
            this.EnableAddressSanitizer = true;
        }
    }
}
```

2.2. `Source/SampleApplication/Module.cs`

```csharp
using Anemone.Framework;

[ModuleRules(ModuleKind.ConsoleApplication)]
public sealed class SampleApplication : ModuleRules
{
    public SampleApplication(TargetRules target)
        : base(target)
    {
        this.PublicDependencies.Add("SampleModule");
    }
}
```

2.3. `Source/SampleModule/Module.cs`

```csharp
using Anemone.Framework;

[ModuleRules(ModuleKind.RuntimeLibrary)]
public sealed class SampleModule : ModuleRules
{
    public SampleModule(TargetRules target)
        : base(target)
    {
    }
}
```


2.2

3. Invoke tool

```powershell
./Build/Anemone.exe --nologo --generate --project ./Project.json --output ./Output
```

This will generate a structure of the project in the output directory:


```
\- Output
  \- SampleTarget-Windows-X64
  | |- Platform.bff
  | |- Target.bff
  | |- Solution.bff
  | |- Target-Debug.bff
  | |- Target-Development.bff
  | |- Target-EngineDebug.bff
  | |- Target-GameDebug.bff
  | |- Target-Shipping.bff
  | \- Target-Testing.bff
  \- SampleTarget-Windows-AArch64
    \- ...
```

4. Invoke external tool to build project

Use `FastBuild` project generator (https://www.fastbuild.org/docs/home.html) to built binaries or generate the Visual Studio solution files.

4.1. Build project directly:

```
fastbuild --config ./Output/SampleTarget-Windows-X64/Target.bff Target-Debug
```

4.2. Build and execute unit tests

```
fastbuild --config ./Output/SampleTarget-Windows-X64/Target.bff Tests-Debug
```

4.3. Generate Visual Studio solution files:

```
fastbuild --config ./Output/SampleTarget-Windows-X64/Solution.bff Solution
```
