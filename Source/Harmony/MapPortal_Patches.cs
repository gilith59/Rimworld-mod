using HarmonyLib;
using RimWorld;
using Verse;

namespace InsectLairIncident
{
    /// <summary>
    /// Patch pour lier la pocket map au portal AVANT sa génération
    /// Permet à GenStep de trouver la geneline correcte
    /// </summary>
    [HarmonyPatch(typeof(MapPortal), "GeneratePocketMapInt")]
    public static class MapPortal_GeneratePocketMapInt_Patch
    {
        public static void Prefix(MapPortal __instance)
        {
            // Vérifier si c'est un InsectLairEntrance
            if (__instance.def.defName == "InsectLairEntrance")
            {
                // Le pocket map va être créé, lier dès maintenant au portal
                GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
                if (globalComp != null)
                {
                    // Note: On ne peut pas obtenir l'ID de la pocket map ici car elle n'existe pas encore
                    // On va stocker temporairement le portal ID pour que GenStep puisse le récupérer
                    MapPortalLinkHelper.currentGeneratingPortalID = __instance.thingIDNumber;
                    // Log.Message($"[InsectLairIncident] Portal {__instance.thingIDNumber} is generating pocket map");
                }
            }
        }

        public static void Postfix(MapPortal __instance, Map __result)
        {
            if (__instance.def.defName == "InsectLairEntrance" && __result != null)
            {
                // Maintenant on peut lier la pocket map créée au portal
                GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
                if (globalComp != null)
                {
                    globalComp.LinkPocketMapToPortal(__result.uniqueID, __instance.thingIDNumber);
                    // Log.Message($"[InsectLairIncident] Linked pocket map {__result.uniqueID} to portal {__instance.thingIDNumber}");
                }
                MapPortalLinkHelper.currentGeneratingPortalID = -1;
            }
        }
    }
}
