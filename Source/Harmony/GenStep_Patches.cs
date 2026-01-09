using HarmonyLib;
using RimWorld;
using Verse;

namespace InsectLairIncident
{
    /// <summary>
    /// Patch pour activer le remplacement des insectes pendant la génération de cave
    /// </summary>
    [HarmonyPatch(typeof(GenStep_InsectLairCave), "Generate")]
    public static class GenStep_InsectLairCave_Generate_Patch
    {
        public static void Prefix(Map map)
        {
            // Récupérer la geneline depuis le GameComponent global
            // Utiliser le portal ID en cours de génération
            GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
            int portalID = MapPortalLinkHelper.currentGeneratingPortalID;
            GenelineData geneline = (portalID >= 0) ? globalComp?.GetGeneline(portalID) : null;

            if (geneline != null)
            {
                // Log.Message($"[InsectLairIncident] Activating geneline spawning for cave generation: {geneline.defName}");
                PawnGenerator_GeneratePawn_Patch.SetInsectLairGeneration(true, geneline);
            }
            else
            {
                Log.Error("[InsectLairIncident] No geneline found for cave generation!");
            }
        }

        public static void Postfix()
        {
            // Désactiver après génération
            PawnGenerator_GeneratePawn_Patch.SetInsectLairGeneration(false, null);
            // Log.Message("[InsectLairIncident] Deactivated geneline spawning");
        }
    }
}
