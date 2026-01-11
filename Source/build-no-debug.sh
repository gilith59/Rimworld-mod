#!/bin/bash
# Build LootScrap without debug logs for production release

echo "🔨 Building LootScrap v1.5.0 (production - no debug logs)..."

# Create temporary directory for source without logs
TEMP_DIR=$(mktemp -d)
echo "📁 Temporary directory: $TEMP_DIR"

# Copy all source files
cp -r LootScrap "$TEMP_DIR/"

# Remove all Log.Message lines from all .cs files
echo "🗑️  Removing debug logs..."
find "$TEMP_DIR" -name "*.cs" -type f -exec sed -i '/Log\.Message/d' {} \;

# Count removed lines
REMOVED=$(find LootScrap -name "*.cs" -exec grep -c "Log.Message" {} \; | awk '{s+=$1} END {print s}')
echo "✅ Removed $REMOVED debug log lines"

# Compile
RIMWORLD_LIBS="/home/gilith/Rimworld mod/RimWorld/RimWorldWin64_Data/Managed"
HARMONY_DLL="/home/gilith/Rimworld mod/RimWorld/Mods/Harmony/Current/Assemblies/0Harmony.dll"
OUTPUT_DIR="../1.6/Assemblies"

echo "⚙️  Compiling..."
mcs -target:library \
    -out:"$OUTPUT_DIR/LootScrap.dll" \
    -reference:"$RIMWORLD_LIBS/Assembly-CSharp.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.CoreModule.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.dll" \
    -reference:"$HARMONY_DLL" \
    -reference:"$RIMWORLD_LIBS/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    $(find "$TEMP_DIR/LootScrap" -name "*.cs")

if [ $? -eq 0 ]; then
    echo "✅ LootScrap compiled successfully (production build)!"
    ls -lh "$OUTPUT_DIR/LootScrap.dll"

    # Cleanup
    rm -rf "$TEMP_DIR"
    echo "🧹 Cleaned up temporary files"
else
    echo "❌ Compilation failed"
    rm -rf "$TEMP_DIR"
    exit 1
fi
