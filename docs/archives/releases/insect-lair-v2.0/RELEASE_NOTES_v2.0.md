# 🎉 Release Notes - Insect Lair Incident v2.0

**Release Date:** 2026-01-03
**Download:** [InsectLairIncident_v2.0.zip](InsectLairIncident_v2.0.zip) (39 KB)

---

## 🌟 What's New in v2.0

### Major Features

**🐛 VFE Insectoids 2 Integration**
- Each incident now picks ONE random geneline
- The entire cave uses only that geneline (Sorne, Nuchadus, Chelis, Kemia, or Xanides)
- Boss is the geneline's unique leader:
  - Sorne → **VFEI2_Empress**
  - Nuchadus → **VFEI2_Titantick**
  - Chelis → **VFEI2_Teramantis**
  - Kemia → **VFEI2_Gigamite**
  - Xanides → **VFEI2_Silverfish**

**✅ Fixed Boss Spawning**
- Boss now correctly spawns in boss room
- Treasure chests generate properly
- All cave insects are from the chosen geneline
- No more crashes during cave generation

**🎮 Vanilla Support**
- Works perfectly without VFE Insectoids installed
- Falls back to vanilla HiveQueen and insects

---

## 🔧 Technical Improvements

### Fixed Issues
1. **NullReferenceException in GenerateBossRoom** - Fixed by allowing vanilla HiveQueen generation, then replacing post-generation
2. **Wrong DLL Loading** - Removed 1.5 folder, added LoadFolders.xml
3. **PawnGenerationRequest Modification** - Properly handle struct by creating new instance
4. **Boss Not in Boss Room** - Harmony patch now skips HiveQueen during generation

### Code Quality
- Removed all debug logs
- Production-ready balance settings
- Clean compilation with no warnings
- Well-documented source code

---

## 📦 Installation

1. Download `InsectLairIncident_v2.0.zip`
2. Extract to `RimWorld/Mods/` folder
3. Enable in mod list (load after Harmony, Core, all DLCs, and VFE Insectoids if using)
4. Start or continue your game

---

## ⚙️ Requirements

- **RimWorld 1.6** (1.5 compatible)
- **Odyssey DLC** (required)
- **VFE Insectoids 2** (optional but highly recommended!)

---

## 🎮 Gameplay

### Balance Settings
- Appears after day 30 with 4+ colonists
- 45 days minimum between incidents
- Waves spawn every 1 in-game day
- Wave strength: 40% of colony threat points
- Never spawns in fogged areas

### How to Play
1. Receive warning letter when ground starts cracking
2. Prepare during 8-16 hour emergence phase
3. Fight off daily insectoid waves
4. Enter cave when ready
5. Defeat the geneline boss
6. Claim rewards and collapse entrance

---

## 📊 Statistics

- **Development Time:** ~6 hours
- **Lines of Code:** 1,665
- **DLL Size:** 25 KB
- **Package Size:** 39 KB
- **Bugs Fixed:** 7 major issues

---

## 🐛 Known Issues

None currently! Please report any bugs on the GitHub Issues page.

---

## 🙏 Credits

- **Author:** gilith59
- **Development Assistant:** Claude AI (Anthropic)
- **Special Thanks:** Oskar Potocki & VFE team for amazing genelines

---

## 📝 Changelog Summary

### v2.0 (2026-01-03)
- ✨ Added full VFE Insectoids geneline integration
- 🐛 Fixed boss spawning in boss room
- 🐛 Fixed cave generation crashes
- 🐛 Fixed insect replacement system
- 📦 Improved mod structure and loading
- 📚 Complete documentation

### v1.0 (Previous)
- Initial release with basic functionality

---

## 🔗 Links

- **GitHub:** https://github.com/gilith59/Rimworld-mod
- **Branch:** `mod/insect-lair-incident`
- **Full Documentation:** See PROJECT_DOCUMENTATION.md

---

**Ready to test your colony against geneline-specific insect hordes!** 🐛🔥
