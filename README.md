# Beerophage Mod - Testing Version

## Overview
A RimWorld mod that adds alcohol-dependent xenotypes called Beerophages. They use a "beergen" resource system similar to hemogen for sanguophages.

## Features
- **Beerophage Xenotype**: Alcohol-dependent humans with unique abilities
- **Beergen System**: Custom resource that depletes daily and must be replenished with beer
- **Beer Casket**: Specialized rest chamber that consumes beer to restore beergen
- **Enhanced Alcohol Tolerance**: Reduced negative effects from alcohol consumption

## How to Test
1. Start a new game with Biotech enabled
2. Create pawns with the Beerophage xenotype in scenario editor OR
3. Use dev mode to add Beergenic gene to existing pawns
4. Monitor the beergen meter (like hemogen for sanguophages)
5. Provide beer for beerophages to drink
6. Research "Beer Casket Technology" to build beer caskets
7. Build beer caskets in the Beerophage designation category

## Key Components
- **Beergenic Gene**: Creates the beergen resource and daily drain
- **Enhanced Alcohol Tolerance**: Reduces alcohol tolerance buildup and addiction
- **Beer Casket**: Uses beer as fuel to provide enhanced rest
- **Beer Integration**: Drinking beer restores beergen levels

## Dependencies
- RimWorld Biotech DLC (required)
- Compatible with RimWorld 1.4 and 1.5

## Technical Notes
- Uses vanilla hemogen system (Gene_Hemogen) with custom resource labels
- Patches vanilla beer to provide beergen restoration
- Includes mood effects for low/empty beergen levels
- Custom designation category for beerophage buildings

## Compilation Required
**IMPORTANT**: This mod requires C# compilation to work fully. See `C#_COMPILATION_GUIDE.md` for detailed instructions.

### Quick Compilation (if you have Visual Studio):
1. Open `Source/BeerophageMod.csproj`
2. Build the project
3. DLL will be created in `1.5/Assemblies/`

### Alternative (Simplified Version):
If you can't compile C#, edit the XML files to use vanilla classes instead of custom ones.

## Known Issues
- This is a testing version with borrowed textures from Sanguophage mod
- Requires C# compilation for full functionality
- May need balancing adjustments for beergen drain rates
- Beer casket currently uses drain casket graphics

## File Structure
```
Beerophage/
├── About/About.xml
├── LoadFolders.xml
├── Source/ (C# source code)
├── 1.4/ (RimWorld 1.4 compatibility)
├── 1.5/ (RimWorld 1.5 compatibility)
└── Documentation files
```