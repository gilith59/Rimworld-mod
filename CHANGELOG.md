# Changelog - Insect Lair Incident

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
