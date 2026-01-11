using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace LootScrap
{
    /// <summary>
    /// Patch for ThingOwner.TryDrop to intercept equipment drops
    /// Adds dropped items to batch for scrap conversion
    /// Multiple patches applied in HarmonyInit.cs due to C# attribute limitations
    /// </summary>
    [HarmonyPatch]
    public static class ThingOwner_TryDrop_Patch
    {
        public static void Postfix(bool __result, ThingOwner __instance, Thing thing)
        {
            try
            {
                if (!__result || thing == null || !thing.Spawned)
                    return;

                var settings = LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();
                if (!settings.enableScrapSystem)
                    return;

                // Find the pawn that owns this equipment
                Pawn pawn = null;

                if (__instance.Owner is Pawn_EquipmentTracker equipTracker)
                    pawn = equipTracker.pawn;
                else if (__instance.Owner is Pawn_ApparelTracker apparelTracker)
                    pawn = apparelTracker.pawn;

                if (pawn == null)
                    return;

                // Check if we should process this pawn based on state
                bool isDead = pawn.Dead;
                bool isDowned = pawn.Downed;
                bool isPrisoner = pawn.IsPrisonerOfColony;

                // Only process if:
                // - Dead (always if not corpses only), OR
                // - Downed and scrapDownedWhenStripped enabled, OR
                // - Prisoner and scrapPrisonersWhenStripped enabled
                if (!isDead)
                {
                    if (isDowned && !settings.scrapDownedWhenStripped)
                        return;
                    if (isPrisoner && !settings.scrapPrisonersWhenStripped)
                        return;
                    if (!isDowned && !isPrisoner)
                        return; // Not dead, not downed, not prisoner - skip
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

                if (!isWeapon && !isApparel)
                    return;

                if (isWeapon && !settings.scrapWeapons)
                    return;

                if (isApparel && !settings.scrapApparel)
                    return;

                // Check if we should preserve this item
                if (settings.preserveUniqueItems && ScrapUtility.ShouldPreserveItem(thing))
                    return;

                // Add to pawn's batch for later processing
                ScrapUtility.AddItemToBatch(pawn, thing);
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in TryDrop_Postfix: {ex}");
            }
        }
    }
}
