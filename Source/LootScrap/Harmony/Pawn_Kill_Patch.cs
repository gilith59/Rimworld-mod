using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace LootScrap
{
    /// <summary>
    /// Patch for Pawn.Kill to strip equipment when pawns die
    /// Initiates scrap conversion process for dead enemies
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        public static void Postfix(Pawn __instance)
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

                // Check hostile requirement (allow neutral/no-faction pawns)
                if (settings.onlyScrapHostiles && __instance.Faction != null)
                {
                    if (!__instance.Faction.HostileTo(Faction.OfPlayer))
                        return;
                }

                // Mark pawn as being processed and force equipment/apparel to drop
                if (__instance.MapHeld != null)
                {
                    // Register pawn for batch processing
                    Pawn_Strip_Patch.AddProcessingPawn(__instance);
                    ScrapUtility.InitializePawnBatch(__instance);

                    // Force drop all equipment and apparel
                    __instance.Strip(true);

                    // Schedule finalization to run after all TryDrop calls complete
                    Pawn pawnCopy = __instance;
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        if (Pawn_Strip_Patch.IsPawnBeingProcessed(pawnCopy))
                        {
                            Pawn_Strip_Patch.RemoveProcessingPawn(pawnCopy);
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
    }
}
