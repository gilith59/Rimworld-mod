using RimWorld;
using Verse;

namespace LootScrap
{
    /// <summary>
    /// Cached ThingDef references for LootScrap mod
    /// Eliminates repeated string-based def lookups for better performance
    /// </summary>
    [DefOf]
    public static class LootScrapDefOf
    {
        // Scrap ThingDefs - sorted by quality tier
        public static ThingDef LootScrap_Glitterworld;
        public static ThingDef LootScrap_High;
        public static ThingDef LootScrap_Good;
        public static ThingDef LootScrap_Junk;

        // Static Constructor (REQUIRED for DefOf pattern)
        static LootScrapDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LootScrapDefOf));
        }
    }
}
