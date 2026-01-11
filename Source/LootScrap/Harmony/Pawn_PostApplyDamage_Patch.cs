using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace LootScrap
{
    /// <summary>
    /// Patch for Pawn.PostApplyDamage to detect when pawns become downed
    /// Initializes batch processing so weapons can be added before they drop
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "PostApplyDamage")]
    public static class Pawn_PostApplyDamage_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                // Check if pawn just became downed (not dead)
                if (!__instance.Downed || __instance.Dead)
                    return;

                Log.Message($"[LootScrap] PostApplyDamage: {__instance.LabelShort} is now downed");

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
                if (settings.onlyScrapHostiles && __instance.Faction != null)
                {
                    if (!__instance.Faction.HostileTo(Faction.OfPlayer))
                        return;
                }

                // Check if scrapDownedWhenStripped is enabled
                if (!settings.scrapDownedWhenStripped)
                    return;

                // Initialize batch for this pawn so weapons can be caught
                if (__instance.MapHeld != null && !Pawn_Strip_Patch.IsPawnBeingProcessed(__instance))
                {
                    Log.Message($"[LootScrap] PostApplyDamage: Initializing batch for downed {__instance.LabelShort}");
                    Pawn_Strip_Patch.AddProcessingPawn(__instance);
                    ScrapUtility.InitializePawnBatch(__instance);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[LootScrap] Exception in PostApplyDamage_Postfix: {ex}");
            }
        }
    }
}
