using HarmonyLib;
using System.Reflection;
using Verse;

namespace InsectLairIncident
{
    /// <summary>
    /// Initialise Harmony au démarrage du jeu et applique tous les patches
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmony = new Harmony("com.insectlairincident.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // Log.Message("[InsectLairIncident] Harmony patches applied");
        }
    }
}
