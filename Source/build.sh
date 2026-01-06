#!/bin/bash
echo "🔨 Building Decadents Mod..."

# Set paths
RIMWORLD_LIBS="/home/gilith/Rimworld mod/RimWorld/RimWorldWin64_Data/Managed"
HARMONY_DLL="/home/gilith/Rimworld mod/references/Harmony/Current/Assemblies/0Harmony.dll"
OUTPUT_PATH="../1.5/Assemblies"

# Create output directory
mkdir -p "$OUTPUT_PATH"
mkdir -p "../1.4/Assemblies"

# Compile
mcs -target:library \
    -out:"$OUTPUT_PATH/Decadents.dll" \
    -reference:"$RIMWORLD_LIBS/Assembly-CSharp.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.CoreModule.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.dll" \
    -reference:"$HARMONY_DLL" \
    -reference:"$RIMWORLD_LIBS/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    *.cs

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    cp "$OUTPUT_PATH/Decadents.dll" "../1.4/Assemblies/"
    ls -lh "$OUTPUT_PATH/Decadents.dll"
else
    echo "❌ Build failed"
    exit 1
fi
