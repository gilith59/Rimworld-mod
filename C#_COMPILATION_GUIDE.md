# Beerophage Mod - C# Compilation Guide

## Overview
The Beerophage mod requires C# compilation to create a DLL that handles the beergen system functionality. The XML definitions alone are not sufficient for the complete functionality.

## Why C# is Needed

The mod requires C# code for:

1. **Building_BeerCasket.cs** - Handles beer casket functionality to restore beergen while sleeping
2. **IngestionOutcomeDoer_OffsetBeergen.cs** - Manages beergen restoration when drinking beer
3. **CompBeergenRestoration.cs** - Component for beergen restoration mechanics

## Compilation Options

### Option 1: Visual Studio (Windows)
1. Install Visual Studio with C# support
2. Open `Source/BeerophageMod.csproj`
3. Build the project (Ctrl+Shift+B)
4. DLL will be created in `1.5/Assemblies/BeerophageMod.dll`

### Option 2: Command Line (Windows)
1. Install .NET Framework 4.7.2 or later
2. Run `Source/build.bat`
3. DLL will be created automatically

### Option 3: Mono (Linux/Mac)
1. Install mono-complete: `sudo apt install mono-complete`
2. Run `chmod +x Source/build.sh && Source/build.sh`
3. DLL will be created automatically

### Option 4: Use Existing Vanilla Classes (Simplified)
If compilation is not possible, you can modify the XML to use vanilla classes:

1. Remove `<thingClass>BeerophageMod.Building_BeerCasket</thingClass>` from BeerCasket.xml
2. Use `<thingClass>Building_Bed</thingClass>` instead
3. Change the beer patch to use `IngestionOutcomeDoer_OffsetHemogen` instead of custom class
4. The mod will work but with reduced functionality (no automatic beergen restoration in caskets)

## Current Status Without Compilation

**What Works:**
- Beerophage xenotype with beergen meter
- Daily beergen drain
- Beer casket as a bed with beer fuel consumption
- Basic mood effects for low beergen

**What Doesn't Work:**
- Automatic beergen restoration while sleeping in beer casket
- Custom beergen restoration messages from drinking beer
- Enhanced beer casket inspection strings

## Files Created

### C# Source Files:
- `Source/BeerophageMod.csproj` - Project file
- `Source/Building_BeerCasket.cs` - Beer casket building class
- `Source/IngestionOutcomeDoer_OffsetBeergen.cs` - Beergen restoration from ingestion
- `Source/CompBeergenRestoration.cs` - Beergen restoration component
- `Source/build.bat` - Windows build script
- `Source/build.sh` - Linux/Mac build script

### Supporting Files:
- `1.5/Languages/English/Keyed/Beerophage.xml` - Localization keys
- Updated XML files to reference custom classes

## Testing the Mod

### Without C# Compilation:
1. Remove/comment out custom thingClass references
2. Use vanilla IngestionOutcomeDoer_OffsetHemogen in beer patch
3. Test basic functionality (xenotype, beergen meter, beer consumption)

### With C# Compilation:
1. Compile the DLL using one of the methods above
2. Ensure BeerophageMod.dll exists in both 1.4/Assemblies and 1.5/Assemblies
3. Test full functionality including beer casket restoration

## Key Classes Explained

### Building_BeerCasket
- Extends Building_Bed to add beergen restoration while sleeping
- Monitors fuel (beer) consumption
- Provides enhanced inspection strings showing beergen levels
- Automatically restores beergen for sleeping beergenic pawns

### IngestionOutcomeDoer_OffsetBeergen
- Handles beergen restoration when consuming alcoholic beverages
- Respects alcohol tolerance for restoration amounts
- Shows feedback messages to players
- Provides stat display for beergen restoration values

### CompBeergenRestoration
- Component that can be added to any building
- Provides passive beergen restoration
- Configurable restoration rates
- Alternative to the Building_BeerCasket approach

## Troubleshooting

**Build Errors:**
- Ensure RimWorld paths are correct in build scripts
- Check that Assembly-CSharp.dll exists in RimWorld installation
- Verify .NET Framework/Mono version compatibility

**Runtime Errors:**
- Check mod load order (should load after Biotech)
- Verify all XML references to C# classes are correct
- Ensure DLL is in correct Assemblies folder

**Missing Functionality:**
- If beergen doesn't restore from beer, check the patch operation
- If casket doesn't work, verify the thingClass reference
- Check for error logs in RimWorld console (Ctrl+Shift+O)