# Changelog - Insect Lair Incident

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
