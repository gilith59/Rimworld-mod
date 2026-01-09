using HarmonyLib;
using RimWorld;
using Verse;

namespace InsectLairIncident
{
    /// <summary>
    /// Patch pour remplacer les Hives vanilla par des hives VFE spécifiques à la geneline
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
    public static class Thing_SpawnSetup_Patch
    {
        public static void Prefix(Thing __instance, Map map)
        {
            // Vérifier si c'est une Hive vanilla qui spawn dans une InsectLair pocket map
            if (__instance.def.defName == "Hive" && map.Parent != null && map.Parent.def != null && map.Parent.def.defName == "InsectLair")
            {
                // Récupérer la geneline de cette pocket map
                GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
                GenelineData geneline = globalComp?.GetGenelineFromPocketMap(map);

                if (geneline != null && !geneline.isVanilla)
                {
                    // Remplacer le def par le hive VFE correspondant
                    ThingDef vfeHive = GenelineHelper.GetVFEHiveForGeneline(geneline.defName);
                    if (vfeHive != null)
                    {
                        __instance.def = vfeHive;
                        Log.Message($"[InsectLairIncident] Replaced vanilla Hive with {vfeHive.defName} for geneline {geneline.defName}");
                    }
                }
            }
        }
    }
}
