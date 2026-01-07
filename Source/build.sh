#!/bin/bash

# LootScrap Build Script
echo "🔨 Building LootScrap..."

# Paths
RIMWORLD_LIBS="/home/gilith/Rimworld mod/RimWorld/RimWorldWin64_Data/Managed"
HARMONY_DLL="/home/gilith/Rimworld mod/mod rimworld/Harmony/Current/Assemblies/0Harmony.dll"
OUTPUT_DIR="../1.5/Assemblies"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Compile
mcs -target:library \
    -out:"$OUTPUT_DIR/LootScrap.dll" \
    -reference:"$RIMWORLD_LIBS/Assembly-CSharp.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.CoreModule.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.dll" \
    -reference:"$HARMONY_DLL" \
    -reference:"$RIMWORLD_LIBS/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    LootScrap/*.cs

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo "DLL size: $(du -h "$OUTPUT_DIR/LootScrap.dll" | cut -f1)"
else
    echo "❌ Build failed!"
    exit 1
fi
