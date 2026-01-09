#!/bin/bash

# InsectLairIncident Build Script
echo "🔨 Building InsectLairIncident..."

# Paths
RIMWORLD_LIBS="/home/gilith/Rimworld mod/RimWorld/RimWorldWin64_Data/Managed"
VEF_HARMONY="/home/gilith/Rimworld mod/RimWorld/Mods/VanillaExpandedFramework/1.0/Assemblies/0Harmony.dll"
OUTPUT_DIR="../1.6/Assemblies"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Compile
mcs -target:library \
    -out:"$OUTPUT_DIR/InsectLairIncident.dll" \
    -reference:"$RIMWORLD_LIBS/Assembly-CSharp.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.CoreModule.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.dll" \
    -reference:"$VEF_HARMONY" \
    -reference:"$RIMWORLD_LIBS/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    *.cs

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo "DLL size: $(du -h "$OUTPUT_DIR/InsectLairIncident.dll" | cut -f1)"
else
    echo "❌ Build failed!"
    exit 1
fi
