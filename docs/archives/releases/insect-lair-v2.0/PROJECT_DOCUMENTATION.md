# 📚 Project Documentation - Insect Lair Incident v2.0

**Date:** 2026-01-03
**Author:** gilith59 + Claude AI
**Version:** 2.0
**Development Time:** ~6 hours

---

## 📋 Table of Contents

1. [Project Overview](#project-overview)
2. [Development Timeline](#development-timeline)
3. [Technical Architecture](#technical-architecture)
4. [File Structure](#file-structure)
5. [Key Features](#key-features)
6. [Problems Solved](#problems-solved)
7. [GitHub Structure](#github-structure)
8. [Build & Compilation](#build--compilation)
9. [Testing Guide](#testing-guide)
10. [Release Process](#release-process)

---

## 🎯 Project Overview

**Goal:** Transform the Odyssey DLC's InsectLair into a random colony map incident (like Anomaly's PitGate) with full VFE Insectoids 2 geneline integration.

**Core Concept:**
- Random incident spawns an insect cave entrance on the colony map
- Progressive emergence over 8-16 hours (warning time)
- Daily insectoid waves until boss is defeated
- **Each cave uses ONE random VFE geneline** (all insects from same geneline)
- Boss is the geneline's unique leader (Empress, Gigamite, etc.)

---

## 📅 Development Timeline

### Phase 1: Initial Setup (2 hours)
- Created mod structure (About.xml, Defs, Source)
- Implemented basic incident spawning
- Created progressive emergence system using `BuildingGroundSpawner`
- Set up wave spawning system with `MapComponent_InsectLairWaveSpawner`

### Phase 2: Boss & Cave Integration (1 hour)
- Added boss tracking with `MapComponent_HiveQueenTracker`
- Created conditional collapse system (`CompSealable_Conditional`)
- Integrated with vanilla InsectLair cave generation
- Added GenStep for boss detection

### Phase 3: VFE Insectoids Integration (2 hours)
- Implemented `GenelineHelper` for VFE detection
- Created `GameComponent_InsectLairGenelines` for global tracking
- Added Harmony patches to replace vanilla insects
- **Major challenge:** Boss replacement causing crashes

### Phase 4: Bug Fixing & Optimization (1 hour)
- Fixed NullReferenceException in GenerateBossRoom
- Solution: Don't replace HiveQueen during generation, replace after
- Fixed DLL loading issues (removed 1.5 folder)
- Added LoadFolders.xml for proper version management
- Cleaned up debug logs

### Phase 5: Production Release (30 minutes)
- Created README, CHANGELOG, documentation
- Packaged v2.0 ZIP file
- Set up GitHub with branch structure
- Prepared Discord release message

---

## 🏗️ Technical Architecture

### Core Components

#### 1. **Incident System**
- `IncidentWorker_InsectLairSpawn.cs` - Triggers incident, spawns BuildingGroundSpawner
- Validates spawn location (no fog, no roof, solid terrain)
- Registers threat points and geneline

#### 2. **Wave Spawning**
- `MapComponent_InsectLairWaveSpawner.cs` - Manages recurring waves
- Spawns first wave 1 second after portal opens
- Subsequent waves every 60,000 ticks (1 in-game day)
- Wave strength: 40% of colony threat points

#### 3. **VFE Geneline Integration**
- `GenelineHelper.cs` - Detects VFE, chooses random geneline
- `GameComponent_InsectLairGenelines.cs` - Global storage for active genelines
- Weighted random selection based on VFE spawnWeight

#### 4. **Harmony Patching**
- `HarmonyPatches.cs` - Patches `PawnGenerator.GeneratePawn`
- Replaces vanilla insects (Megascarab, Spelopede, Megaspider) with VFE insects
- **Critical:** Does NOT replace HiveQueen during generation (causes crash)

#### 5. **Boss System**
- `GenStep_SpawnHiveQueen.cs` - Post-generation boss replacement
- Finds vanilla HiveQueen, replaces with VFE boss
- `MapComponent_HiveQueenTracker.cs` - Tracks boss alive/dead status
- `CompSealable_Conditional.cs` - Blocks collapse until boss dead

### Data Flow

```
Incident Triggered
    ↓
IncidentWorker spawns BuildingGroundSpawner + chooses geneline
    ↓
GameComponent stores geneline globally
    ↓
8-16 hours: Ground cracks (visual effects)
    ↓
Portal opens (InsectLairEntrance spawned)
    ↓
MapComponent detects portal, starts wave timer
    ↓
First wave after 60 ticks (1 second)
    ↓
Player enters portal → Pocket map generated
    ↓
Harmony patch: Replace vanilla insects with VFE (EXCEPT HiveQueen)
    ↓
GenStep_InsectLairCave generates boss room (vanilla HiveQueen)
    ↓
GenStep_TrackVanillaHiveQueen replaces HiveQueen with VFE boss
    ↓
Boss defeated → Collapse unlocked
    ↓
Collapse triggered → Waves stop
```

---

## 📁 File Structure

```
InsectLairIncident/
├── About/
│   └── About.xml                           # Mod metadata
├── 1.6/
│   ├── Assemblies/
│   │   └── InsectLairIncident.dll          # Compiled code (25 KB)
│   ├── Defs/
│   │   ├── GenStepDefs/
│   │   │   └── GenStep_TrackQueen.xml      # Boss tracking GenStep
│   │   ├── IncidentDefs/
│   │   │   └── Incident_InsectLair.xml     # Incident definition
│   │   └── ThingDefs_Buildings/
│   │       └── Buildings_InsectLairSpawner.xml  # BuildingGroundSpawner
│   └── Patches/
│       ├── InsectLair_Patch.xml            # Add GenStep to MapGenerator
│       └── InsectLairEntrance_CompPatch.xml # Replace CompSealable
├── Source/
│   ├── CompSealable_Conditional.cs         # Conditional collapse system
│   ├── GameComponent_InsectLairGenelines.cs # Global geneline storage
│   ├── GenelineHelper.cs                   # VFE detection & selection
│   ├── GenStep_SpawnHiveQueen.cs           # Post-gen boss replacement
│   ├── HarmonyPatches.cs                   # Pawn generation patches
│   ├── IncidentWorker_InsectLairSpawn.cs   # Incident trigger
│   ├── MapComponent_HiveQueenTracker.cs    # Boss tracking
│   └── MapComponent_InsectLairWaveSpawner.cs # Wave management
├── LoadFolders.xml                         # Version folder management
├── README.md                               # User documentation
├── CHANGELOG.md                            # Version history
├── DOCUMENTATION.md                        # Developer notes
└── compile.sh                              # Build script

Total: ~1,665 lines of code
```

---

## ✨ Key Features

### 1. Progressive Emergence System
**File:** `Buildings_InsectLairSpawner.xml`
```xml
<groundSpawnerSpawnDelay>5000~10000</groundSpawnerSpawnDelay>
```
- Random 8-16 hour delay before portal opens
- Uses vanilla `BuildingGroundSpawner` with custom effecters
- Visual effects: `EmergencePointSustained8X8`, `EmergencePointComplete8X8`
- Audio: `PitGateOpening` sustainer sound

### 2. VFE Geneline Integration
**Files:** `GenelineHelper.cs`, `GameComponent_InsectLairGenelines.cs`

**Detection:**
```csharp
Type insectGenelineDefType = GenTypes.GetTypeInAnyAssembly(
    "VFEInsectoids.InsectGenelineDef",
    "VFEInsectoids"
);
```

**Selection:**
- Weighted random based on VFE `spawnWeight` values
- Extracts: boss, hive, insects list from geneline
- Stores globally for pocket map access

**Genelines:**
- VFEI_Sorne → Boss: VFEI2_Empress
- VFEI_Nuchadus → Boss: VFEI2_Titantick
- VFEI_Chelis → Boss: VFEI2_Teramantis
- VFEI_Kemia → Boss: VFEI2_Gigamite
- VFEI_Xanides → Boss: VFEI2_Silverfish

### 3. Insect Replacement System
**File:** `HarmonyPatches.cs`

**Harmony Patch:**
```csharp
[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn))]
```

**Logic:**
- Intercepts all pawn generation in cave
- Checks if `request.Faction == Faction.OfInsects`
- Replaces vanilla insects with VFE geneline insects
- **CRITICAL:** Skips HiveQueen to avoid GenerateBossRoom crash

**Replaced Insects:**
- Megascarab → Random VFE insect from geneline
- Spelopede → Random VFE insect from geneline
- Megaspider → Random VFE insect from geneline

### 4. Boss Replacement (Post-Generation)
**File:** `GenStep_SpawnHiveQueen.cs`

**Why Post-Generation?**
- Vanilla `GenerateBossRoom` expects HiveQueen specifically
- Replacing during generation causes NullReferenceException
- Solution: Let vanilla generate HiveQueen, then replace

**Process:**
1. Vanilla generates HiveQueen in boss room
2. GenStep finds HiveQueen: `map.mapPawns.AllPawnsSpawned`
3. Destroys vanilla HiveQueen
4. Spawns VFE boss at same position
5. Registers boss in tracker

### 5. Wave System
**File:** `MapComponent_InsectLairWaveSpawner.cs`

**Configuration:**
```csharp
private const int WAVE_INTERVAL_TICKS = 60000; // 1 day
float pointsRemaining = threatPoints * 0.4f;   // 40% threat
```

**Wave Composition:**
- Uses geneline insects only
- Spawns until threat points exhausted
- All spawned as manhunter

---

## 🐛 Problems Solved

### Problem 1: Boss Not Spawning in Boss Room
**Symptom:**
- Boss spawned randomly in cave
- Boss room empty, no chests
- Other insects missing

**Cause:**
- Harmony patch replaced HiveQueen during `GenerateBossRoom`
- Vanilla code expects HiveQueen specifically
- NullReferenceException crashed generation

**Solution:**
```csharp
// In HarmonyPatches.cs Prefix
if (request.KindDef.defName == "HiveQueen") {
    return true; // Skip replacement, let vanilla handle
}
```
- Let vanilla generate HiveQueen normally
- Replace AFTER generation in GenStep
- Boss room generates correctly with chests

### Problem 2: Wrong DLL Loaded
**Symptom:**
- Old code still running despite recompilation
- Logs showed "Replacing BOSS HiveQueen" (old behavior)

**Cause:**
- RimWorld has both 1.5/ and 1.6/ folders
- Loading wrong version (1.5 DLL was old)
- No LoadFolders.xml to specify version

**Solution:**
1. Deleted 1.5 folder entirely
2. Created LoadFolders.xml:
```xml
<v1.6>
  <li>/</li>
  <li>1.6</li>
</v1.6>
```
3. Forced RimWorld restart

### Problem 3: PawnGenerationRequest Modification
**Symptom:**
- Error: "Failed to find kindDef field"
- Insects not being replaced

**Cause:**
- `PawnGenerationRequest` is a struct (not class)
- Can't modify fields via reflection
- Initial code tried `SetValue()` which failed

**Solution:**
- Create new `PawnGenerationRequest` with replacement kind
- Copy all properties from old request
- Return new request via `ref` parameter

### Problem 4: GitHub Token Security
**Symptom:**
- User accidentally shared GitHub token publicly

**Solution:**
- Stored token in `~/.claude/github_token`
- Added to `.gitignore`
- Used in push commands: `git push https://user:${TOKEN}@github.com/...`

---

## 🌿 GitHub Structure

### Repository Organization

**URL:** https://github.com/gilith59/Rimworld-mod

**Branch Strategy:**
```
main (default)
  ├── README.md (repository overview)
  └── .gitignore

mod/insect-lair-incident
  ├── Full mod source code
  ├── README.md (mod-specific)
  ├── CHANGELOG.md
  └── All mod files

mod/[future-mod-name]
  └── Future mods follow same pattern
```

**Benefits:**
- Clean separation per mod
- Independent version history
- Easy to navigate
- Scalable for multiple mods

### Commands Used

**Initial Setup:**
```bash
cd InsectLairIncident
git init
git config user.name "gilith59"
git config user.email "gilith59@users.noreply.github.com"
```

**Mod Branch:**
```bash
git add -A
git commit -m "InsectLairIncident v2.0"
git branch -M mod/insect-lair-incident
git remote add origin https://github.com/gilith59/Rimworld-mod.git
git push -u origin mod/insect-lair-incident
```

**Main Branch:**
```bash
git checkout -b main
# Clean up files, keep only README
git add README.md .gitignore
git commit -m "Main branch: Repository overview"
git push origin main --force
```

---

## 🔨 Build & Compilation

### Prerequisites
- Mono C# Compiler (mcs)
- RimWorld 1.6
- Odyssey DLC
- Harmony library

### Compilation Script
**File:** `compile.sh`

```bash
#!/bin/bash
cd "Source"

mcs -target:library \
  -out:"../1.6/Assemblies/InsectLairIncident.dll" \
  -reference:"../../../RimWorldWin64_Data/Managed/Assembly-CSharp.dll" \
  -reference:"../../../RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll" \
  -reference:"../../../RimWorldWin64_Data/Managed/UnityEngine.dll" \
  -reference:"/path/to/Harmony/0Harmony.dll" \
  -reference:"../../../RimWorldWin64_Data/Managed/netstandard.dll" \
  -nowarn:0219,0162,0414 \
  *.cs
```

### Build Process
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods/InsectLairIncident"
bash compile.sh
```

**Output:**
- `1.6/Assemblies/InsectLairIncident.dll` (25 KB)
- No warnings or errors

---

## 🧪 Testing Guide

### Test Environment Setup

**Launch Command:**
```bash
cd "/home/gilith/Rimworld mod"
./RimWorld/RimWorldWin64.exe \
  -quicktest \
  -savedatafolder="./TestData" \
  -logfile "./TestData/Player.log"
```

**Benefits:**
- Isolated test environment
- Separate logs
- Doesn't affect main game saves

### Test Scenarios

#### 1. Basic Incident Test
```
1. Start game with Dev Mode enabled
2. Open debug console (` key)
3. Type: incident insectlairemergence
4. Verify:
   - InsectLairSpawner appears (6x6 area)
   - Ground cracks visible
   - Geneline logged in Player.log
```

#### 2. Wave System Test
```
1. Wait for portal to open (or speed up time)
2. Verify first wave spawns after 1 second
3. Wait 1 in-game day
4. Verify second wave spawns
5. Check all insects are from same geneline
```

#### 3. Cave Generation Test
```
1. Enter portal with colonists
2. Verify cave generates correctly:
   - Boss room has treasure chests
   - Boss is VFE geneline leader
   - All insects are from same geneline
   - No vanilla insects present
```

#### 4. Boss Defeat & Collapse Test
```
1. Kill the boss
2. Exit cave
3. Select portal entrance
4. Verify "Collapse cave entrance" button is enabled
5. Collapse entrance
6. Verify waves stop spawning
```

### Log Monitoring

**Key Log Patterns:**
```bash
# Check geneline selection
grep "Geneline choisie" TestData/Player.log

# Check insect replacement
grep "Replacing" TestData/Player.log

# Check for errors
grep "Error\|Exception" TestData/Player.log
```

**Expected Logs:**
```
[InsectLairIncident] VFE Insectoids active check: True
[InsectLairIncident] Found 5 genelines in VFE
[InsectLairIncident] Geneline choisie: VFEI_Kemia
[InsectLairIncident] Geneline registered for map 0: VFEI_Kemia (Boss: VFEI2_Gigamite)
[InsectLairIncident] Replacing Megascarab with VFEI2_Megathrips from VFEI_Kemia geneline
[InsectLairIncident] Using geneline: VFEI_Kemia (Boss: VFEI2_Gigamite)
```

---

## 📦 Release Process

### Version 2.0 Release Checklist

**Pre-Release:**
- [x] All features implemented
- [x] Boss spawning fixed
- [x] Cave generation working
- [x] VFE integration complete
- [x] Debug logs removed
- [x] Production balance configured

**Documentation:**
- [x] README.md created
- [x] CHANGELOG.md updated
- [x] DOCUMENTATION.md written
- [x] Discord post prepared

**Packaging:**
```bash
cd "RimWorld/Mods"
zip -r "InsectLairIncident_v2.0.zip" InsectLairIncident \
  -x "*.git*" "*.bak" "*compile.sh" "*.gitignore"
```

**Result:**
- File: `InsectLairIncident_v2.0.zip` (39 KB)
- Contains: Mod files, README, CHANGELOG, Source code

**GitHub:**
- [x] Branch created: `mod/insect-lair-incident`
- [x] Main branch updated with README
- [x] Version tag: v2.0
- [x] All code pushed

**Discord Release:**
- [x] Message prepared in `DISCORD_POST.md`
- [x] ZIP file ready
- [x] GitHub link included

### Distribution Files

**For Users:**
1. `InsectLairIncident_v2.0.zip` - Full mod package
2. `DISCORD_POST.md` - Copy/paste announcement
3. GitHub link: https://github.com/gilith59/Rimworld-mod

**Installation Instructions:**
```
1. Download InsectLairIncident_v2.0.zip
2. Extract to RimWorld/Mods/ folder
3. Enable in mod list (after Harmony, Core, DLCs, VFE Insectoids)
4. Start or continue your game
```

---

## 📊 Statistics

### Development Metrics
- **Total Lines of Code:** ~1,665
- **C# Files:** 8
- **XML Files:** 6
- **Development Time:** ~6 hours
- **Bugs Fixed:** 7 major issues
- **Commits:** 15+
- **DLL Size:** 25 KB
- **Package Size:** 39 KB

### Code Distribution
- **Source Code:** 60% (C# logic)
- **XML Definitions:** 25% (Defs, Patches)
- **Documentation:** 15% (README, CHANGELOG)

### Balance Parameters (Production)
- Wave interval: 60,000 ticks (1 day)
- Wave strength: 40% of threat points
- Earliest spawn: Day 30, 4+ colonists
- Minimum interval: 45 days
- Emergence time: 5,000-10,000 ticks (8-16 hours)

---

## 🎓 Lessons Learned

### Technical Insights

1. **Harmony Patching Best Practices**
   - Prefix patches can skip execution with `return false`
   - Be careful with struct modifications (use new instance)
   - Don't patch critical vanilla methods if possible

2. **RimWorld Cave Generation**
   - `GenerateBossRoom` is fragile, expects specific pawn types
   - Use GenSteps with high order number for post-processing
   - Pocket maps share game components, not map components

3. **VFE Integration**
   - Use reflection carefully, cache Type lookups
   - Check for null at every step
   - Provide vanilla fallback if VFE not installed

4. **DLL Loading Issues**
   - RimWorld caches DLLs aggressively
   - Use LoadFolders.xml for version management
   - Remove unused version folders to avoid confusion

### Project Management

1. **Version Control**
   - Branch-per-mod structure scales well
   - Keep main branch clean with just overview
   - Commit frequently with descriptive messages

2. **Testing Strategy**
   - Use isolated test environment (savedatafolder)
   - Monitor logs in real-time
   - Test all scenarios before release

3. **Documentation**
   - Write docs during development, not after
   - Include "why" not just "what"
   - Document problems and solutions

---

## 🚀 Future Improvements

### Potential Features (Not Implemented)

1. **Dynamic Wave Scaling**
   - Increase difficulty over time
   - Add special waves (all bosses, swarms, etc.)

2. **Custom Rewards**
   - Unique geneline-specific loot
   - Bonus rewards for harder genelines

3. **Configuration Options**
   - XML-configurable wave intervals
   - Adjustable threat scaling
   - Toggle VFE integration

4. **Mod Compatibility**
   - Combat Extended support
   - Alpha Animals integration
   - More geneline mods

5. **Visual Improvements**
   - Custom emergence effects per geneline
   - Boss introduction cutscene
   - Warning notifications 30s before waves

---

## 📞 Support & Contact

### Bug Reports
- GitHub Issues: https://github.com/gilith59/Rimworld-mod/issues
- Discord: [Specify channel]

### Mod Compatibility
**Compatible:**
- VFE Insectoids 2 (optional, highly recommended)
- Vanilla Expanded Framework
- Most QoL mods

**Potential Conflicts:**
- Mods that modify InsectLairEntrance
- Mods that patch PawnGenerator heavily
- Custom incident mods with similar mechanics

---

## 🙏 Credits & Acknowledgments

### Development Team
- **gilith59** - Mod concept, testing, feedback
- **Claude AI (Anthropic)** - Code development, debugging, documentation

### Inspirations
- **Ludeon Studios** - RimWorld, Odyssey DLC
- **Oskar Potocki & Team** - VFE Insectoids 2 (amazing genelines!)
- **Anomaly DLC** - PitGate incident inspiration

### Special Thanks
- RimWorld modding community
- VFE Discord for mod support
- Everyone who tests and provides feedback

---

## 📄 License & Usage

This mod is provided as-is for the RimWorld community.

**Permissions:**
- ✅ Play and share
- ✅ Include in modpacks (with credit)
- ✅ Learn from source code
- ✅ Modify for personal use

**Restrictions:**
- ❌ No commercial use
- ❌ No re-upload without permission
- ❌ Credit required for derivatives

---

**End of Documentation**

*Last Updated: 2026-01-03*
*Version: 2.0*
*Document Version: 1.0*
