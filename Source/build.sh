#!/bin/bash

# LootScrap Build Script
echo "🔨 Building LootScrap..."

# Paths
RIMWORLD_LIBS="/home/gilith/Rimworld mod/RimWorld/RimWorldWin64_Data/Managed"
HARMONY_DLL="/home/gilith/Rimworld mod/RimWorld/Mods/Harmony/1.5/Assemblies/0Harmony.dll"
OUTPUT_DIR="../1.5/Assemblies"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Compile (includes all subdirectories)
mcs -target:library \
    -out:"$OUTPUT_DIR/LootScrap.dll" \
    -r:"$RIMWORLD_LIBS/Assembly-CSharp.dll" \
    -r:"$RIMWORLD_LIBS/UnityEngine.CoreModule.dll" \
    -r:"$RIMWORLD_LIBS/UnityEngine.dll" \
    -r:"$HARMONY_DLL" \
    -r:"$RIMWORLD_LIBS/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    LootScrap/*.cs LootScrap/Harmony/*.cs LootScrap/Utilities/*.cs LootScrap/Settings/*.cs

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo "DLL size: $(du -h "$OUTPUT_DIR/LootScrap.dll" | cut -f1)"
else
    echo "❌ Build failed!"
    exit 1
fi
