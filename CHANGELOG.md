# Changelog - Insect Lair Incident

## [2.6.0] - 2026-01-29

### Balance Changes

- **Easier default difficulty (Easy Mode)**: Reduced threat multiplier from 75% to **50%** (-25%)
  - Effective wave strength: 20% of colony threat points (was 30%)
  - Rationale: First insect waves should be more manageable for new players
  - Updated presets: Easy (50%), Normal (75%), Hard (100%), Extreme (150%)

### Features

- **Wave warning system**: Receive an alert 2.5 hours before each wave arrives
  - Letter type: ThreatSmall (yellow)
  - Message: "The lair is stirring... Ominous chittering echoes from the depths. A new wave approaches!"
  - Gives players time to prepare defenses and position colonists

- **VFE Insectoids disable option**: New setting to force vanilla insects even if VFE installed
  - Setting name: "Disable VFE Insectoids (Force Vanilla)"
  - When enabled, all lairs use vanilla insects instead of VFE genelines
  - Useful for players who want consistent vanilla experience

### Bug Fixes

- **Fixed auto-collapse timer bug with multi-map games**
  - Problem: MapComponentTick ran on ALL maps, causing conflicts when queen was on a different map
  - Solution: Added early return if queen == null in MapComponent_HiveQueenTracker
  - Prevents timer bugs when multiple maps are loaded

---

## [2.5.5] - 2026-01-11

### Balance Changes

- **Reduced default wave strength**: Decreased threat points multiplier from 100% to **75%** (-25%)
  - Effective wave strength: 30% of colony threat points (was 40%)
  - Rationale: Beta feedback indicated waves were too strong, especially with VFE Insectoids acid-spitting bugs
  - Players can still adjust this in mod settings (25% to 300% range)

### Bug Fixes

- **Fixed RimSort crash**: Removed corrupted directory with Windows carriage returns in name (`Assemblies^M^M/`)
  - ZIP file structure is now clean and compatible with RimSort
  - Manual decompression workaround no longer needed

---

## [2.5.4] - 2026-01-09

### Code Organization

- **Professional directory structure**: Reorganized source code into logical directories for better maintainability
  - `Harmony/` - All Harmony patches split into separate files by patched type
    - `PawnGenerator_Patches.cs` - Pawn generation patches
    - `GenStep_Patches.cs` - GenStep cave generation patches
    - `MapPortal_Patches.cs` - Portal generation patches
    - `Thing_Patches.cs` - Thing spawn patches
  - `Components/` - MapComponents and GameComponents
  - `Incidents/` - IncidentWorkers
  - `WorldGen/` - GenSteps
  - `Utilities/` - Helper classes (GenelineHelper, MapPortalLinkHelper)
  - `UI/` - User interface (Alerts)
  - `Settings/` - Mod settings

### Technical Changes

- Split monolithic `HarmonyPatches.cs` (263 lines) into 4 focused patch files (~30-110 lines each)
- Extracted `MapPortalLinkHelper` to Utilities directory for code reuse
- Consolidated `GetVFEHiveForGeneline()` method into `GenelineHelper` utility class (removed duplication)
- Created `HarmonyInit.cs` for clean Harmony initialization
- Updated build script to compile from new directory structure

### Benefits

- Easier navigation: Find patches by file name instead of scrolling large files
- Better git workflow: Reduced merge conflicts when multiple patches modified
- Code reuse: Shared helper methods in Utilities directory
- Professional standard: Matches organization patterns used by VE mods

---

## [2.3.0] - 2026-01-06

### New Feature

- **VFE geneline-specific hives**: When VFE Insectoids 2 is installed, caves now spawn with geneline-specific dormant hives instead of vanilla ones
  - Nuchadus geneline → VFEI2_NuchadusHive
  - Chelis geneline → VFEI2_ChelisHive
  - Kemia geneline → VFEI2_KemianHive
  - Xanides geneline → VFEI2_XanidesHive
  - Sorne geneline → Vanilla Hive (no VFE equivalent)

### Technical Changes

- Modified `GenStep_SpawnHiveQueen` to automatically replace vanilla dormant hives with VFE geneline-specific hives after cave generation
- Added `ReplaceVanillaHivesWithVFE()` method to scan and replace all vanilla Hive ThingDefs on pocket map
- Added `GetVFEHiveForGeneline()` helper to map geneline names to appropriate VFE hive ThingDefs

---

## [2.2.0] - 2026-01-05

### Major Features

- **Collapse countdown timer**: After defeating the boss, a PitGate-style alert appears in the bottom right showing time remaining until cave collapse (72h countdown)
- **Waves stop after boss death**: Insectoid waves properly stop spawning when the boss is defeated
- **Multi-lair support**: Multiple lairs can now be active simultaneously, each with their own unique geneline
- **Alert color-coding**:
  - >24h remaining: Yellow alert (clear background)
  - <24h remaining: Red alert (critical background)

### Bug Fixes

- Fixed geneline not applying correctly to pocket map (boss and cave insects now match the geneline)
- Fixed waves continuing to spawn after boss death
- Fixed geneline being overwritten when multiple lairs are active

### Technical Changes

- Added `Alert_InsectLairCollapsing` class using vanilla Alert system
- Modified `MapComponent_HiveQueenTracker` to expose `IsBossDead()` and `GetTicksUntilCollapse()` methods
- Modified `MapComponent_InsectLairWaveSpawner` to check boss status before spawning waves
- Rewrote `GameComponent_InsectLairGenelines` to use portal ID instead of map ID (enables multi-lair support)
- Added Harmony patch for `MapPortal.GeneratePocketMapInt` to link pocket maps to portals during generation
- Added `MapPortalLinkHelper` to pass portal ID between patches

---

## [2.1.2] - 2026-01-05

### Bugfixes & UX Improvements

- **Added collapse countdown timer**: After defeating the boss, a PitGate-style alert now appears in the bottom right corner showing time remaining until cave collapse (72h countdown)
- **Fixed waves continuing after boss death**: Insectoid waves now properly stop spawning when the boss is defeated
- **Alert color-coding**:
  - >12h remaining: Yellow alert (clear background)
  - <12h remaining: Red alert (critical background)

#### Technical Changes
- Added `Alert_InsectLairCollapsing` class using vanilla Alert system
- Modified `MapComponent_HiveQueenTracker` to expose `IsBossDead()` and `GetTicksUntilCollapse()` methods
- Modified `MapComponent_InsectLairWaveSpawner` to check boss status before spawning waves
- Added English translation keys for alert labels

---

## [2.1.1] - 2026-01-05

### Bugfix
- Fixed XML error: Changed `<version>` tag to `<modVersion>` for RimWorld 1.6 compatibility

---

## [2.0.0] - 2026-01-03

### Major Update - VFE Geneline Integration

Previous version (1.0) had basic functionality. This version adds full VFE Insectoids support!

#### Features
- Transform Odyssey InsectLair into random colony map incident
- Progressive emergence system (8-16 hours)
- Recurring insectoid waves every 1 day (scales with colony wealth)
- VFE Insectoids 2 integration:
  - Each incident picks one random geneline
  - All cave insects use that geneline only
  - Boss is geneline's unique leader (Empress, Gigamite, etc.)
- Full vanilla cave exploration with rewards
- Boss must be defeated to unlock collapse
- Anti-fog spawning protection
- Vanilla insect support (without VFE)

#### Balance
- Earliest appearance: Day 30 with 4+ colonists
- Minimum interval: 45 days between incidents
- Wave strength: 40% of colony threat points
- Wave frequency: Every 1 day (60000 ticks)

#### Technical
- RimWorld 1.6 support (1.5 compatible)
- Requires Odyssey DLC
- Optional VFE Insectoids 2 support
- Harmony patches for pawn generation
- Custom GenStep for boss replacement
- MapComponent-based wave system

#### Known Issues
- None currently

---

**Credits:** gilith59
