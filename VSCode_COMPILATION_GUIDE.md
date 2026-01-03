# Visual Studio Code Compilation Guide

## Prerequisites

### 1. Install .NET SDK
Download and install the .NET SDK from https://dotnet.microsoft.com/download
- Choose .NET 6.0 or later (compatible with .NET Framework projects)
- Or install .NET Framework 4.7.2+ on Windows

### 2. Install VS Code Extensions
Open VS Code and install these extensions:
- **C# Dev Kit** (Microsoft) - Official C# support
- **C#** (Microsoft) - IntelliSense and debugging
- **.NET Install Tool** (Microsoft) - Helps manage .NET versions

## Method 1: Using .NET CLI (Recommended)

### Step 1: Open Terminal in VS Code
1. Open the `Beerophage` folder in VS Code
2. Open Terminal (Ctrl+` or View → Terminal)
3. Navigate to the Source folder:
```bash
cd Source
```

### Step 2: Build with .NET CLI
```bash
# Build the project
dotnet build BeerophageMod.csproj

# Or build in Release mode for better performance
dotnet build BeerophageMod.csproj --configuration Release
```

The DLL will be created in `../1.5/Assemblies/BeerophageMod.dll`

## Method 2: Using VS Code Tasks

### Step 1: Create Build Task
1. Press `Ctrl+Shift+P` and type "Tasks: Configure Task"
2. Select "Create tasks.json from template"
3. Choose ".NET Core"

### Step 2: Update tasks.json
Replace the content with:
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build-beerophage",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/Source/BeerophageMod.csproj",
                "--configuration",
                "Release"
            ],
            "group": {
                "kind": "build",
                "isDefault": true
            },
            "presentation": {
                "echo": true,
                "reveal": "silent",
                "focus": false,
                "panel": "shared"
            },
            "problemMatcher": "$msCompile"
        }
    ]
}
```

### Step 3: Build Using Task
- Press `Ctrl+Shift+P`
- Type "Tasks: Run Build Task"
- Select "build-beerophage"

## Method 3: Alternative .csproj for .NET SDK

If you have issues with the current .csproj, create this alternative version:

### Create: Source/BeerophageMod-DotNet.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <OutputPath>../1.5/Assemblies/</OutputPath>
    <AssemblyName>BeerophageMod</AssemblyName>
    <RootNamespace>BeerophageMod</RootNamespace>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
  </PropertyGroup>
  
  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>../../rimworld base/RimWorldWin64_Data/Managed/Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>../../rimworld base/RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

Then build with:
```bash
dotnet build BeerophageMod-DotNet.csproj
```

## Method 4: Manual Compilation (If .NET SDK Issues)

### Windows with .NET Framework:
```cmd
cd Source
csc /target:library /out:../1.5/Assemblies/BeerophageMod.dll /reference:"../../rimworld base/RimWorldWin64_Data/Managed/Assembly-CSharp.dll" /reference:"../../rimworld base/RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll" *.cs
```

### Linux/Mac with Mono:
```bash
cd Source
mcs -target:library -out:../1.5/Assemblies/BeerophageMod.dll -reference:"../../rimworld base/RimWorldWin64_Data/Managed/Assembly-CSharp.dll" -reference:"../../rimworld base/RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll" *.cs
```

## Troubleshooting

### Error: "Could not find RimWorld references"
1. Update the paths in the .csproj file to match your RimWorld installation
2. Make sure RimWorld is installed and Assembly-CSharp.dll exists

### Error: "Target framework not found"
1. Install .NET Framework 4.7.2+ (Windows) or use the alternative .csproj
2. Or change TargetFramework to `net6.0` in the alternative .csproj

### Error: "MSBuild not found"
1. Install Visual Studio Build Tools or full Visual Studio
2. Or use the .NET CLI method instead

### Success Indicators
- ✅ No compilation errors in terminal
- ✅ `BeerophageMod.dll` appears in `1.5/Assemblies/`
- ✅ File size is reasonable (not 0 bytes)

## VS Code Features You Can Use

### IntelliSense
- Hover over RimWorld classes to see documentation
- Auto-completion for methods and properties
- Error highlighting in real-time

### Debugging (Advanced)
- Set breakpoints in your code
- Debug RimWorld mod execution (requires additional setup)

### Git Integration
- Track changes to your mod files
- Commit and push to repositories

## After Successful Compilation

1. Copy the DLL to 1.4 folder: `cp 1.5/Assemblies/BeerophageMod.dll 1.4/Assemblies/`
2. Test the mod in RimWorld
3. Check for errors in RimWorld's console (Ctrl+Shift+O)

## Quick Commands Summary

```bash
# Navigate to source
cd Source

# Build (most common)
dotnet build BeerophageMod.csproj

# Build release version
dotnet build BeerophageMod.csproj --configuration Release

# Clean and rebuild
dotnet clean && dotnet build
```