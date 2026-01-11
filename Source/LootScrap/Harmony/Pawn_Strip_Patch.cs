using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

namespace LootScrap
{
    /// <summary>
    /// Patch for Pawn.Strip to handle downed/prisoner stripping
    /// Initializes batch processing when pawns are stripped
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Strip")]
    public static class Pawn_Strip_Patch
    {
        // Track pawns being processed to know when to finalize batch conversion
        private static HashSet<Pawn> pawnsBeingProcessed = new HashSet<Pawn>();

        public static void Prefix(Pawn __instance)
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

                // Check hostile requirement (not applicable to prisoners, allow neutral faction)
                if (settings.onlyScrapHostiles && !isPrisoner && __instance.Faction != null)
                {
                    if (!__instance.Faction.HostileTo(Faction.OfPlayer))
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

        /// <summary>
        /// Check if a pawn is currently being processed for batch conversion
        /// </summary>
        public static bool IsPawnBeingProcessed(Pawn pawn)
        {
            return pawnsBeingProcessed.Contains(pawn);
        }

        /// <summary>
        /// Add a pawn to the processing tracker
        /// </summary>
        public static void AddProcessingPawn(Pawn pawn)
        {
            pawnsBeingProcessed.Add(pawn);
        }

        /// <summary>
        /// Remove a pawn from the processing tracker
        /// </summary>
        public static void RemoveProcessingPawn(Pawn pawn)
        {
            pawnsBeingProcessed.Remove(pawn);
        }
    }
}
