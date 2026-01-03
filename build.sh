#!/usr/bin/env bash

echo "Building Beerophage Mod..."

# Set .NET environment if not already set
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet

# Navigate to source folder
cd "$(dirname "$0")/Source"

# Build the project
$HOME/.dotnet/dotnet build BeerophageMod-Modern.csproj --configuration Release

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Build successful!"
    echo "📁 DLL created at: 1.5/Assemblies/BeerophageMod.dll"
    echo "📁 DLL copied to: 1.4/Assemblies/BeerophageMod.dll"
    echo ""
    echo "🍺 Your Beerophage mod is ready to test in RimWorld!"
    echo ""
    echo "Next steps:"
    echo "1. Start RimWorld with Biotech enabled"
    echo "2. Enable the Beerophage mod in the mod manager"
    echo "3. Create a new game or use dev mode to test"
else
    echo ""
    echo "❌ Build failed! Check the errors above."
fi