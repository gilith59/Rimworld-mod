using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

namespace LootScrap
{
    /// <summary>
    /// Patch for Corpse.SpawnSetup to handle pre-existing corpses found on map
    /// Converts equipment from corpses that spawn on the map (e.g., ancient danger)
    /// </summary>
    [HarmonyPatch(typeof(Corpse), "SpawnSetup")]
    public static class Corpse_SpawnSetup_Patch
    {
        public static void Postfix(Corpse __instance)
        {
            try
            {
                if (__instance == null || __instance.InnerPawn == null)
                    return;

                var settings = LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();
                if (!settings.enableScrapSystem || !settings.scrapFoundCorpses)
                    return;

                Pawn pawn = __instance.InnerPawn;

                // Only process humanlike pawns
                if (!pawn.RaceProps.Humanlike)
                    return;

                // Don't process player faction
                if (pawn.Faction == Faction.OfPlayer)
                    return;

                // Check hostile requirement (but always allow neutral/no-faction corpses in ruins)
                if (settings.onlyScrapHostiles && pawn.Faction != null)
                {
                    if (!pawn.Faction.HostileTo(Faction.OfPlayer))
                        return;
                }

                // Get equipment and apparel from corpse
                List<Thing> items = new List<Thing>();

                if (pawn.equipment != null && pawn.equipment.AllEquipmentListForReading != null)
                {
                    foreach (ThingWithComps eq in pawn.equipment.AllEquipmentListForReading)
                    {
                        if (settings.scrapWeapons && (!settings.preserveUniqueItems || !ScrapUtility.ShouldPreserveItem(eq)))
                            items.Add(eq);
                    }
                }

                if (pawn.apparel != null && pawn.apparel.WornApparel != null)
                {
                    foreach (Apparel ap in pawn.apparel.WornApparel)
                    {
                        if (settings.scrapApparel && (!settings.preserveUniqueItems || !ScrapUtility.ShouldPreserveItem(ap)))
                            items.Add(ap);
                    }
                }

                if (items.Count == 0)
                    return;

                // Initialize batch and convert
                ScrapUtility.InitializePawnBatch(pawn);
                foreach (Thing item in items)
                {
                    ScrapUtility.AddItemToBatch(pawn, item);
                }

                // Spawn scraps at corpse location
                IntVec3 pos = __instance.Position;
                Map map = __instance.Map;

                ScrapUtility.FinalizePawnBatch(pawn);

                // Remove the items from the corpse
                if (pawn.equipment != null)
                    pawn.equipment.DestroyAllEquipment();
                if (pawn.apparel != null)
                    pawn.apparel.DestroyAll();
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in CorpseSpawn_Postfix: {ex}");
            }
        }
    }
}
