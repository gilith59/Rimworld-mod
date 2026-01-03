#!/usr/bin/env bash
echo "Building Beerophage Mod..."
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet
cd Source
$HOME/.dotnet/dotnet build BeerophageMod-Modern.csproj --configuration Release
echo "Build complete!"