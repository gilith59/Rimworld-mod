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

            // Patch Pawn.Kill to strip equipment when pawns die
            var killMethod = AccessTools.Method(typeof(Pawn), "Kill", (Type[])null, (Type[])null);
            if (killMethod != null)
            {
                harmony.Patch(
                    killMethod,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Kill_Postfix)))
                );
            }
            else
            {
                Log.Error("[LootScrap] Failed to find Pawn.Kill!");
            }

            // Patch Pawn.Strip to handle downed/prisoner stripping
            var stripMethod = AccessTools.Method(typeof(Pawn), "Strip", new Type[] { typeof(bool) });
            if (stripMethod != null)
            {
                harmony.Patch(
                    stripMethod,
                    prefix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Strip_Prefix)))
                );
            }
            else
            {
                Log.Error("[LootScrap] Failed to find Pawn.Strip!");
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
            }
        }

        public static void Strip_Prefix(Pawn __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                var settings = LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();
                if (!settings.enableScrapSystem)
                    return;

                // Only process humanlike pawns
                if (!__instance.RaceProps.Humanlike)
                    return;

                // Don't process dead pawns (handled by Kill_Postfix)
                if (__instance.Dead)
                    return;

                bool isDowned = __instance.Downed;
                bool isPrisoner = __instance.IsPrisonerOfColony;

                // Check if we should process this strip
                if (!isDowned && !isPrisoner)
                    return;

                if (isDowned && !settings.scrapDownedWhenStripped)
                    return;

                if (isPrisoner && !settings.scrapPrisonersWhenStripped)
                    return;

                // Don't process player faction (unless prisoner)
                if (__instance.Faction == Faction.OfPlayer && !isPrisoner)
                    return;

                // Check hostile requirement (not applicable to prisoners)
                if (settings.onlyScrapHostiles && !isPrisoner)
                {
                    if (__instance.Faction == null || !__instance.Faction.HostileTo(Faction.OfPlayer))
                        return;
                }

                // Initialize batch processing for this pawn
                if (__instance.MapHeld != null)
                {
                    pawnsBeingProcessed.Add(__instance);
                    ScrapUtility.InitializePawnBatch(__instance);

                    // Schedule finalization
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
                Log.Error($"[LootScrap] Exception in Strip_Prefix: {ex}");
            }
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

                // Check hostile requirement (not applicable to prisoners being stripped)
                if (settings.onlyScrapHostiles && !isPrisoner)
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
                ScrapUtility.AddItemToBatch(pawn, thing);
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in TryDrop_Postfix: {ex}");
            }
        }

    }
}
