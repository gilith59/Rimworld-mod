#!/bin/bash
echo "Building Beerophage Mod Assembly..."

# Set paths - adjust these to your RimWorld installation
RIMWORLD_PATH="/home/roman/Rimworld/RimWorldLinux_Data/Managed"
OUTPUT_PATH="../1.5/Assemblies"

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_PATH"

# Compile using mono's C# compiler if available
if command -v mcs &> /dev/null; then
    mcs -target:library \
        -out:"$OUTPUT_PATH/BeerophageMod.dll" \
        -reference:"$RIMWORLD_PATH/Assembly-CSharp.dll" \
        -reference:"$RIMWORLD_PATH/UnityEngine.CoreModule.dll" \
        *.cs
    
    if [ $? -eq 0 ]; then
        echo "Build successful! DLL created at $OUTPUT_PATH/BeerophageMod.dll"
        # Copy to 1.4 folder as well
        cp "$OUTPUT_PATH/BeerophageMod.dll" "../1.4/Assemblies/"
    else
        echo "Build failed"
        exit 1
    fi
else
    echo "Mono C# compiler (mcs) not found. Please install mono-complete package."
    echo "Or use Visual Studio/MSBuild on Windows to compile the .csproj file"
    exit 1
fi