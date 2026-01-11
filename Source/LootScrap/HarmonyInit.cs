using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace LootScrap
{
    /// <summary>
    /// Initializes Harmony at game startup and applies all patches
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmony = new Harmony("gilith.lootscrap");

            // Manually patch ThingOwner.TryDrop (multiple overloads - can't use attributes)
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

            var prefixMethod = AccessTools.Method(typeof(ThingOwner_TryDrop_Patch), nameof(ThingOwner_TryDrop_Patch.Prefix));
            var postfixMethod = AccessTools.Method(typeof(ThingOwner_TryDrop_Patch), nameof(ThingOwner_TryDrop_Patch.Postfix));

            if (tryDropMethod1 != null)
            {
                harmony.Patch(
                    tryDropMethod1,
                    prefix: new HarmonyMethod(prefixMethod),
                    postfix: new HarmonyMethod(postfixMethod)
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
                    prefix: new HarmonyMethod(prefixMethod),
                    postfix: new HarmonyMethod(postfixMethod)
                );
            }

            // Patch all other classes automatically
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // Log.Message("[LootScrap] Harmony patches applied");
        }
    }
}
