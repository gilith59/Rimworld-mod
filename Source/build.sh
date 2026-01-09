#!/bin/bash

# InsectLairIncident Build Script
echo "🔨 Building InsectLairIncident..."

# Paths
RIMWORLD_LIBS="/home/gilith/Rimworld mod/RimWorld/RimWorldWin64_Data/Managed"
HARMONY_DLL="/home/gilith/Rimworld mod/RimWorld/Mods/Harmony/1.5/Assemblies/0Harmony.dll"
OUTPUT_DIR="../1.6/Assemblies"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Compile (includes all subdirectories)
mcs -target:library \
    -out:"$OUTPUT_DIR/InsectLairIncident.dll" \
    -r:"$RIMWORLD_LIBS/Assembly-CSharp.dll" \
    -r:"$RIMWORLD_LIBS/UnityEngine.CoreModule.dll" \
    -r:"$RIMWORLD_LIBS/UnityEngine.dll" \
    -r:"$HARMONY_DLL" \
    -r:"$RIMWORLD_LIBS/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    *.cs Harmony/*.cs Components/*.cs Incidents/*.cs WorldGen/*.cs Utilities/*.cs UI/*.cs Settings/*.cs

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo "DLL size: $(du -h "$OUTPUT_DIR/InsectLairIncident.dll" | cut -f1)"
else
    echo "❌ Build failed!"
    exit 1
fi
