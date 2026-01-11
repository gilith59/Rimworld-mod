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
                {
                    Log.Message("[LootScrap] Strip_Prefix: Pawn is null");
                    return;
                }

                Log.Message($"[LootScrap] Strip_Prefix called for {__instance.LabelShort}");

                var settings = LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();
                if (!settings.enableScrapSystem)
                {
                    Log.Message("[LootScrap] Strip_Prefix: Scrap system disabled");
                    return;
                }

                // Only process humanlike pawns
                if (!__instance.RaceProps.Humanlike)
                {
                    Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} is not humanlike");
                    return;
                }

                // Don't process dead pawns (handled by Kill_Postfix)
                if (__instance.Dead)
                {
                    Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} is dead - skipping (handled by Kill_Postfix)");
                    return;
                }

                bool isDowned = __instance.Downed;
                bool isPrisoner = __instance.IsPrisonerOfColony;
                Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} - Downed={isDowned}, Prisoner={isPrisoner}");

                // Check if we should process this strip
                if (!isDowned && !isPrisoner)
                {
                    Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} is neither downed nor prisoner - skipping");
                    return;
                }

                if (isDowned && !settings.scrapDownedWhenStripped)
                {
                    Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} is downed but scrapDownedWhenStripped is disabled");
                    return;
                }

                if (isPrisoner && !settings.scrapPrisonersWhenStripped)
                {
                    Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} is prisoner but scrapPrisonersWhenStripped is disabled");
                    return;
                }

                // Don't process player faction (unless prisoner)
                if (__instance.Faction == Faction.OfPlayer && !isPrisoner)
                {
                    Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} is player faction (not prisoner) - skipping");
                    return;
                }

                // Check hostile requirement (not applicable to prisoners, allow neutral faction)
                if (settings.onlyScrapHostiles && !isPrisoner && __instance.Faction != null)
                {
                    if (!__instance.Faction.HostileTo(Faction.OfPlayer))
                    {
                        Log.Message($"[LootScrap] Strip_Prefix: {__instance.LabelShort} faction is not hostile and onlyScrapHostiles is enabled");
                        return;
                    }
                }

                // Initialize batch processing for this pawn
                if (__instance.MapHeld != null)
                {
                    Log.Message($"[LootScrap] Strip_Prefix: Initializing batch for {__instance.LabelShort}");
                    pawnsBeingProcessed.Add(__instance);
                    ScrapUtility.InitializePawnBatch(__instance);
                }
                else
                {
                    Log.Warning($"[LootScrap] Strip_Prefix: {__instance.LabelShort} has no map!");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in Strip_Prefix: {ex}");
            }
        }

        public static void Postfix(Pawn __instance)
        {
            try
            {
                if (__instance == null)
                {
                    Log.Message("[LootScrap] Strip_Postfix: Pawn is null");
                    return;
                }

                Log.Message($"[LootScrap] Strip_Postfix called for {__instance.LabelShort}");

                // If this pawn was being processed, finalize the batch now
                if (pawnsBeingProcessed.Contains(__instance))
                {
                    Log.Message($"[LootScrap] Strip_Postfix: Finalizing batch for {__instance.LabelShort}");
                    pawnsBeingProcessed.Remove(__instance);
                    ScrapUtility.FinalizePawnBatch(__instance);
                }
                else
                {
                    Log.Message($"[LootScrap] Strip_Postfix: {__instance.LabelShort} was not being processed");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in Strip_Postfix: {ex}");
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
