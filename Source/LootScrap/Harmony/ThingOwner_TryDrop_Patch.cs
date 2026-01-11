using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace LootScrap
{
    /// <summary>
    /// Patch for ThingOwner.TryDrop to intercept equipment drops
    /// Adds dropped items to batch for scrap conversion
    /// Multiple patches applied manually in HarmonyInit.cs due to method overloads
    /// </summary>
    public static class ThingOwner_TryDrop_Patch
    {
        // Prefix: Intercept weapon drops and add to batch instead of dropping
        public static bool Prefix(ThingOwner __instance, Thing thing, ref bool __result)
        {
            try
            {
                if (thing == null)
                    return true; // Continue normal execution

                var settings = LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();
                if (!settings.enableScrapSystem)
                    return true;

                // Only intercept weapon drops from EquipmentTracker
                if (!(__instance.Owner is Pawn_EquipmentTracker equipTracker))
                    return true; // Not equipment, let normal processing happen

                if (!thing.def.IsWeapon)
                    return true; // Not a weapon, let normal processing happen

                Pawn pawn = equipTracker.pawn;
                if (pawn == null)
                    return true;

                // Skip if dead or already processed
                if (pawn.Dead)
                    return true;

                // Check if this is an automatic drop (pawn becoming downed, not manual strip)
                // If pawn is not yet in tracking but is about to be downed, initialize batch now
                bool isBeingProcessed = Pawn_Strip_Patch.IsPawnBeingProcessed(pawn);

                // If not being processed yet, check if pawn health is critical (about to down)
                // We intercept here for automatic drops when pawn becomes downed
                if (!isBeingProcessed && pawn.Downed)
                {
                    // Pawn just became downed but PostApplyDamage hasn't run yet
                    // Initialize batch now so we can catch the weapon
                    Log.Message($"[LootScrap] Prefix: Pawn {pawn.LabelShort} is downed, initializing batch for automatic weapon drop");

                    // Check settings before initializing
                    if (!settings.scrapDownedWhenStripped)
                        return true;

                    if (pawn.Faction == Faction.OfPlayer && !pawn.IsPrisonerOfColony)
                        return true;

                    if (settings.onlyScrapHostiles && pawn.Faction != null && !pawn.Faction.HostileTo(Faction.OfPlayer))
                        return true;

                    // Initialize batch now
                    Pawn_Strip_Patch.AddProcessingPawn(pawn);
                    ScrapUtility.InitializePawnBatch(pawn);
                    isBeingProcessed = true;
                }

                // If still not being processed, let normal drop happen
                if (!isBeingProcessed)
                    return true;

                Log.Message($"[LootScrap] Prefix: Intercepting weapon drop from {pawn.LabelShort}");

                // Check settings
                if (!settings.scrapWeapons)
                    return true;

                if (settings.preserveUniqueItems && ScrapUtility.ShouldPreserveItem(thing))
                    return true;

                // Add to batch directly (thing is still in inventory at this point)
                Log.Message($"[LootScrap] Prefix: Adding {thing.LabelShort} to batch for {pawn.LabelShort}");
                ScrapUtility.AddItemToBatch(pawn, thing);

                // Remove from equipment without dropping
                __instance.Remove(thing);

                // Block the drop
                __result = false;
                return false; // Skip original method
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in TryDrop_Prefix: {ex}");
                return true; // Continue normal execution on error
            }
        }

        public static void Postfix(bool __result, ThingOwner __instance, Thing thing)
        {
            try
            {
                Log.Message($"[LootScrap] TryDrop_Postfix called - result={__result}, thing={thing?.LabelShort ?? "null"}, spawned={thing?.Spawned}");

                if (!__result || thing == null || !thing.Spawned)
                {
                    Log.Message($"[LootScrap] TryDrop_Postfix: Early return - result={__result}, thing null={thing == null}, spawned={thing?.Spawned}");
                    return;
                }

                var settings = LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();
                if (!settings.enableScrapSystem)
                {
                    Log.Message("[LootScrap] TryDrop_Postfix: Scrap system disabled");
                    return;
                }

                // Find the pawn that owns this equipment
                Pawn pawn = null;

                if (__instance.Owner is Pawn_EquipmentTracker equipTracker)
                {
                    pawn = equipTracker.pawn;
                    Log.Message($"[LootScrap] TryDrop_Postfix: Owner is EquipmentTracker, pawn={pawn?.LabelShort}");
                }
                else if (__instance.Owner is Pawn_ApparelTracker apparelTracker)
                {
                    pawn = apparelTracker.pawn;
                    Log.Message($"[LootScrap] TryDrop_Postfix: Owner is ApparelTracker, pawn={pawn?.LabelShort}");
                }
                else
                {
                    Log.Message($"[LootScrap] TryDrop_Postfix: Owner is neither EquipmentTracker nor ApparelTracker - type={__instance.Owner?.GetType().Name ?? "null"}");
                }

                if (pawn == null)
                {
                    Log.Message("[LootScrap] TryDrop_Postfix: Pawn is null");
                    return;
                }

                Log.Message($"[LootScrap] TryDrop_Postfix: Processing {thing.LabelShort} from {pawn.LabelShort}");

                // Check if we should process this pawn based on state
                bool isDead = pawn.Dead;
                bool isDowned = pawn.Downed;
                bool isPrisoner = pawn.IsPrisonerOfColony;

                Log.Message($"[LootScrap] TryDrop_Postfix: Pawn state - Dead={isDead}, Downed={isDowned}, Prisoner={isPrisoner}");

                // Only process if:
                // - Dead (always if not corpses only), OR
                // - Downed and scrapDownedWhenStripped enabled, OR
                // - Prisoner and scrapPrisonersWhenStripped enabled
                if (!isDead)
                {
                    if (isDowned && !settings.scrapDownedWhenStripped)
                    {
                        Log.Message($"[LootScrap] TryDrop_Postfix: Downed but scrapDownedWhenStripped disabled");
                        return;
                    }
                    if (isPrisoner && !settings.scrapPrisonersWhenStripped)
                    {
                        Log.Message($"[LootScrap] TryDrop_Postfix: Prisoner but scrapPrisonersWhenStripped disabled");
                        return;
                    }
                    if (!isDowned && !isPrisoner)
                    {
                        Log.Message($"[LootScrap] TryDrop_Postfix: Not dead, not downed, not prisoner - skipping");
                        return; // Not dead, not downed, not prisoner - skip
                    }
                }

                // Don't process player faction (unless prisoner strip is enabled and they're a prisoner)
                if (pawn.Faction == Faction.OfPlayer && !(isPrisoner && settings.scrapPrisonersWhenStripped))
                    return;

                // Check hostile requirement (not applicable to prisoners, allow neutral faction)
                if (settings.onlyScrapHostiles && !isPrisoner && pawn.Faction != null)
                {
                    if (!pawn.Faction.HostileTo(Faction.OfPlayer))
                        return;
                }

                // Check if it's equipment we should scrap
                bool isWeapon = thing.def.IsWeapon;
                bool isApparel = thing.def.IsApparel;

                Log.Message($"[LootScrap] TryDrop_Postfix: Item type - IsWeapon={isWeapon}, IsApparel={isApparel}");

                if (!isWeapon && !isApparel)
                {
                    Log.Message($"[LootScrap] TryDrop_Postfix: Not weapon or apparel - skipping");
                    return;
                }

                if (isWeapon && !settings.scrapWeapons)
                {
                    Log.Message($"[LootScrap] TryDrop_Postfix: Weapon but scrapWeapons disabled");
                    return;
                }

                if (isApparel && !settings.scrapApparel)
                {
                    Log.Message($"[LootScrap] TryDrop_Postfix: Apparel but scrapApparel disabled");
                    return;
                }

                // Check if we should preserve this item
                if (settings.preserveUniqueItems && ScrapUtility.ShouldPreserveItem(thing))
                {
                    Log.Message($"[LootScrap] TryDrop_Postfix: Item should be preserved (unique/legendary)");
                    return;
                }

                // Add to pawn's batch for later processing
                Log.Message($"[LootScrap] TryDrop_Postfix: Adding {thing.LabelShort} to batch for {pawn.LabelShort}");
                ScrapUtility.AddItemToBatch(pawn, thing);
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in TryDrop_Postfix: {ex}");
            }
        }
    }
}
