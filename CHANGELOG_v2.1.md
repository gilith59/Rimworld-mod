# Changelog - Insect Lair Incident

## Version 2.1 (2026-01-05)

### 🐛 Bug Fixes

1. **Multiple Incidents Prevention**
   - Fixed: Players could spawn multiple insect lairs on the same map
   - Solution: Changed from `allBuildingsColonist` to `listerThings.AllThings` to properly detect existing lairs
   - File: `IncidentWorker_InsectLairSpawn.cs`

2. **Collapse Button Not Showing**
   - Fixed: The collapse button was not visible on the InsectLairEntrance
   - Solution: Created custom button generation instead of relying on base `CompSealable.CompGetGizmosExtra()`
   - File: `CompSealable_Conditional.cs`
   - Button is now always visible but grayed out with helpful tooltips until boss is defeated

3. **Boss Discovery Through Fog**
   - Fixed: Boss discovery message triggered immediately when boss spawned, even through fog of war
   - Solution: Added line-of-sight check in `MapComponentTick()` to verify colonist can actually see the boss
   - File: `MapComponent_HiveQueenTracker.cs`

4. **Death Message Delayed**
   - Fixed: Boss defeated message only appeared after leaving the cave
   - Solution: Created `MapComponent_InsectLairMonitor` on parent map (surface) that monitors pocket map every second
   - Files: `MapComponent_InsectLairMonitor.cs` (new), `IncidentWorker_InsectLairSpawn.cs`
   - Message now appears immediately via `Find.LetterStack.ReceiveLetter()` instead of `Messages.Message()`

5. **Auto-Collapse Not Working**
   - Fixed: Auto-collapse timer expired but entrance didn't seal, showing error "non-destroyable thing"
   - Solution: Use `CompSealable.Seal()` method instead of `Thing.Destroy()`
   - File: `MapComponent_HiveQueenTracker.cs`

### 🔧 Technical Details

**New Files:**
- `MapComponent_InsectLairMonitor.cs` - Monitors pocket map from parent map to detect boss death immediately

**Modified Files:**
- `CompSealable_Conditional.cs` - Custom button generation with proper state management
- `MapComponent_HiveQueenTracker.cs` - Line-of-sight check for discovery, proper seal for auto-collapse
- `IncidentWorker_InsectLairSpawn.cs` - Fixed prevention check, added monitor component

**Compiler:**
- Switched from `csc.exe` (C# 5) to `mcs` (Mono C# Compiler) for modern C# syntax support

### ⚖️ Balance (Unchanged)

- Geneline distribution: 60% Empress, 10% each for Titantick/Teramantis/Gigamite/Silverfish
- Auto-collapse delay: 72 hours (180000 ticks) after boss death
- Emergence time: 8-16 hours in-game (20000-40000 ticks)
- Wave spawn timing: Every 60000 ticks (~1 day)

### 🙏 Credits

- **MDebaque** - Bug reports and testing
- **Gilith** - Development

---

## Version 2.0 (2026-01-02)

Initial release with geneline rebalancing and new features.

### Features
- Boss discovery messages (6 unique messages)
- Always-visible collapse button (grayed until boss dead)
- 72-hour auto-collapse after boss death
- Boss death notification with sound
- Multiple incident prevention
- VFE Insectoids geneline support
