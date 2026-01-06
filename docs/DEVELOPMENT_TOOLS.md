# RimWorld Modding - Development Tools Setup

This guide covers the essential tools and setup for efficient RimWorld mod development.

## Tools Overview

### 1. TDBug - Debug Enhancement Mod ✅ INSTALLED

**Location:** `/home/gilith/Rimworld mod/RimWorld/Mods/TDBug`

**What it does:**
- Provides debug/dev enhancements for RimWorld mod developers
- Essential development tools and utilities
- Compatible with RimWorld 1.6

**Installation:**
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods"
git clone https://github.com/alextd/RimWorld-TDBug.git TDBug
```

**Usage:**
- Always keep this mod enabled during development
- Provides various debugging features accessible in-game

---

### 2. Doorstop - Hot Reload & Debugging ✅ INSTALLED

**Location:** `/home/gilith/Rimworld mod/RimWorld/`

**What it does:**
- Enables C# debugger attachment to running game
- **Hot reload support** - recompile mods without restarting RimWorld
- Breakpoint debugging with IDE support

**Installed Files:**
- `winhttp.dll` - Unity Doorstop v4.4.0 (Windows version for Wine/Proton)
- `Doorstop.dll` - pardeike's RimWorld-Doorstop (2.9MB)
- `doorstop_config.ini` - Configuration file

**Configuration:**
```ini
[UnityMono]
debug_enabled=true
debug_address=127.0.0.1:50000
debug_suspend=false
```

**Hot Reload Usage:**

1. Mark methods as reloadable in your code:
```csharp
using HarmonyLib;

[Reloadable]
public void MyMethod()
{
    // This method can be hot-reloaded
}
```

2. Recompile your mod while RimWorld is running:
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods/YourMod/Source"
mcs -target:library -out:"../1.6/Assemblies/YourMod.dll" \
    -reference:"../../../RimWorldWin64_Data/Managed/Assembly-CSharp.dll" \
    -reference:"../../../RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll" \
    -reference:"../../../RimWorldWin64_Data/Managed/UnityEngine.dll" \
    -reference:"../../../../references/Harmony/Current/Assemblies/0Harmony.dll" \
    -reference:"../../../RimWorldWin64_Data/Managed/netstandard.dll" \
    -nowarn:0219,0162,0414 *.cs
```

3. The code will reload automatically without restarting the game!

**Benefits:**
- **Massive time savings** - no need to restart RimWorld after every code change
- Test fixes immediately
- Faster iteration during bug fixing

**IDE Debugging Support:**
- JetBrains Rider: Use "Attach to Unity Process" with Host: 127.0.0.1, Port: 50000
- Visual Studio: Use "Debug → Attach to Process" and connect to Mono port
- dnSpyEx: Attach to process for debugging

---

### 3. Better Log - Enhanced Logging (Optional)

**Status:** Not installed (requires Steam Workshop subscription)

**Workshop ID:** 3531364227 (1.6 Temp version)
**URL:** https://steamcommunity.com/sharedfiles/filedetails/?id=3531364227

**What it does:**
- Color-coded log messages
- Advanced filtering and muting capabilities
- Better log button placement
- Helps identify errors quickly

**Installation:**
1. Open Steam
2. Subscribe to the mod via Workshop
3. Enable in RimWorld mod manager

**Note:** The original Better Log has compatibility issues with 1.6. Use the [1.6 Temp] version instead.

---

## Installation Summary

### Already Installed ✅
1. **TDBug** - Debug tools mod
2. **Doorstop** - Hot reload & debugging framework

### Manual Installation Required ⏳
- **Better Log [1.6 Temp]** - Via Steam Workshop subscription

---

## Quick Reference

**RimWorld Directory:** `/home/gilith/Rimworld mod/RimWorld/`
**Mods Directory:** `/home/gilith/Rimworld mod/RimWorld/Mods/`
**References Directory:** `/home/gilith/Rimworld mod/references/`

**Compiler:** `mcs` (Mono C# Compiler)
**Target Framework:** .NET 4.7.2

**Debug Port:** 127.0.0.1:50000

---

## Troubleshooting

### Doorstop not loading
- Verify `winhttp.dll` is in the same directory as `RimWorldWin64.exe`
- Check `doorstop_config.ini` has `enabled=true`
- Ensure `Doorstop.dll` is present

### Hot reload not working
- Verify method is marked with `[Reloadable]` attribute
- Check that compilation succeeded without errors
- Ensure Doorstop is properly loaded (check logs)

### Debug port not accessible
- Verify `debug_enabled=true` in `doorstop_config.ini`
- Check firewall settings
- Ensure port 50000 is not in use by another application

---

## References

- TDBug GitHub: https://github.com/alextd/RimWorld-TDBug
- Doorstop GitHub: https://github.com/pardeike/Rimworld-Doorstop
- Unity Doorstop: https://github.com/NeighTools/UnityDoorstop
- Better Log GitHub: https://github.com/bbradson/BetterLog

---

**Last Updated:** January 5, 2026
