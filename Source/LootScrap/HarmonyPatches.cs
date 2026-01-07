using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace LootScrap
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        // Track pawns being processed to know when to finalize batch conversion
        private static HashSet<Pawn> pawnsBeingProcessed = new HashSet<Pawn>();

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("gilith.lootscrap");

            Log.Message("[LootScrap] Initializing Harmony patches...");

            // Patch Pawn.Kill to strip equipment when pawns die (using LootingManager's exact syntax)
            var killMethod = AccessTools.Method(typeof(Pawn), "Kill", (Type[])null, (Type[])null);
            if (killMethod != null)
            {
                harmony.Patch(
                    killMethod,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Kill_Postfix)))
                );
                Log.Message("[LootScrap] Successfully patched Pawn.Kill");
            }
            else
            {
                Log.Error("[LootScrap] Failed to find Pawn.Kill!");
            }

            // Patch ThingOwner.TryDrop to intercept equipment drops
            var tryDropMethod1 = AccessTools.Method(typeof(ThingOwner), "TryDrop", new Type[]
            {
                typeof(Thing),
                typeof(IntVec3),
                typeof(Map),
                typeof(ThingPlaceMode),
                typeof(int),
                typeof(Thing).MakeByRefType(),
                typeof(Action<Thing, int>),
                typeof(Predicate<IntVec3>)
            });

            if (tryDropMethod1 != null)
            {
                harmony.Patch(
                    tryDropMethod1,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(TryDrop_Postfix)))
                );
                Log.Message("[LootScrap] Successfully patched ThingOwner.TryDrop (8 params v1)");
            }

            var tryDropMethod2 = AccessTools.Method(typeof(ThingOwner), "TryDrop", new Type[]
            {
                typeof(Thing),
                typeof(IntVec3),
                typeof(Map),
                typeof(ThingPlaceMode),
                typeof(Thing).MakeByRefType(),
                typeof(Action<Thing, int>),
                typeof(Predicate<IntVec3>),
                typeof(bool)
            });

            if (tryDropMethod2 != null)
            {
                harmony.Patch(
                    tryDropMethod2,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(TryDrop_Postfix)))
                );
                Log.Message("[LootScrap] Successfully patched ThingOwner.TryDrop (8 params v2)");
            }

            Log.Message("[LootScrap] Harmony initialization complete!");
        }

        public static void Kill_Postfix(Pawn __instance)
        {
            try
            {
                if (__instance == null || !__instance.Dead)
                    return;

                var settings = LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();
                if (!settings.enableScrapSystem)
                    return;

                // Only process humanlike pawns
                if (!__instance.RaceProps.Humanlike)
                    return;

                // Don't process player faction
                if (__instance.Faction == Faction.OfPlayer)
                    return;

                // Check hostile requirement
                if (settings.onlyScrapHostiles)
                {
                    if (__instance.Faction == null || !__instance.Faction.HostileTo(Faction.OfPlayer))
                        return;
                }

                // Mark pawn as being processed and force equipment/apparel to drop
                if (__instance.MapHeld != null)
                {
                    Log.Message($"[LootScrap] Stripping pawn {__instance.LabelShort}");

                    // Register pawn for batch processing
                    pawnsBeingProcessed.Add(__instance);
                    ScrapUtility.InitializePawnBatch(__instance);

                    // Force drop all equipment and apparel
                    __instance.Strip(true);

                    // Schedule finalization to run after all TryDrop calls complete
                    Pawn pawnCopy = __instance;
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        if (pawnsBeingProcessed.Contains(pawnCopy))
                        {
                            pawnsBeingProcessed.Remove(pawnCopy);
                            ScrapUtility.FinalizePawnBatch(pawnCopy);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in Kill_Postfix: {ex}");
            }
        }

        public static void TryDrop_Postfix(bool __result, ThingOwner __instance, Thing thing)
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

                // Only process dead pawns
                if (!pawn.Dead)
                    return;

                // Don't process player faction
                if (pawn.Faction == Faction.OfPlayer)
                    return;

                // Check hostile requirement
                if (settings.onlyScrapHostiles)
                {
                    if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer))
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
                Log.Message($"[LootScrap] Queueing {thing.def.defName} from {pawn.LabelShort} (value: {thing.GetStatValue(StatDefOf.MarketValue)} silver)");
                ScrapUtility.AddItemToBatch(pawn, thing);
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in TryDrop_Postfix: {ex}");
            }
        }

    }
}
