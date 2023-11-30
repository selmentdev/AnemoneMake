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

2. Invoke tool

```powershell
./Build/Anemone.exe --nologo --generate --project ./Project.json --output ./Output
```

This will generate a structure of the project in the output directory:


```
\- Output
  \- SampleTarget-Windows-X64
  | \- Platform.bff
  | \- Target.bff
  | \- Solution.bff
  | \- Target-Debug.bff
  | \- Target-Development.bff
  | \- Target-EngineDebug.bff
  | \- Target-GameDebug.bff
  | \- Target-Shipping.bff
  | \- Target-Testing.bff
  \- SampleTarget-Windows-AArch64
    \- ...
```

3. Invoke external tool to build project

Use `FastBuild` project generator (https://www.fastbuild.org/docs/home.html) to built binaries or generate the Visual Studio solution files.

3.1. Build project directly:

```
fastbuild --config ./Output/SampleTarget-Windows-X64/Target.bff Target-Debug
```

3.2. Build and execute unit tests

```
fastbuild --config ./Output/SampleTarget-Windows-X64/Target.bff Tests-Debug
```

3.3. Generate Visual Studio solution files:

```
fastbuild --config ./Output/SampleTarget-Windows-X64/Solution.bff Solution
```
