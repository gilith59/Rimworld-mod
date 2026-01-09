using RimWorld;
using Verse;

namespace InsectLairIncident
{
    /// <summary>
    /// Static DefOf class for cached def references.
    /// Improves performance by avoiding repeated DefDatabase lookups.
    /// </summary>
    [DefOf]
    public static class InsectLairDefOf
    {
        // ═══════════════════════════════════════
        // ThingDefs - Buildings & Structures
        // ═══════════════════════════════════════

        /// <summary>Portal entrance to InsectLair pocket map</summary>
        public static ThingDef InsectLairEntrance;

        /// <summary>Invisible spawner placed before portal appears</summary>
        public static ThingDef InsectLairSpawner;

        // ═══════════════════════════════════════
        // VFE Insectoids - Hives (conditional)
        // ═══════════════════════════════════════

        [MayRequire("OskarPotocki.VFE.Insectoid2")]
        public static ThingDef VFEI2_NuchadusHive;

        [MayRequire("OskarPotocki.VFE.Insectoid2")]
        public static ThingDef VFEI2_ChelisHive;

        [MayRequire("OskarPotocki.VFE.Insectoid2")]
        public static ThingDef VFEI2_KemianHive;

        [MayRequire("OskarPotocki.VFE.Insectoid2")]
        public static ThingDef VFEI2_XanidesHive;

        // ═══════════════════════════════════════
        // Vanilla References
        // ═══════════════════════════════════════

        /// <summary>Vanilla insect hive (for reference)</summary>
        public static ThingDef Hive;

        // ═══════════════════════════════════════
        // Static Constructor (REQUIRED)
        // ═══════════════════════════════════════

        static InsectLairDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(InsectLairDefOf));
        }
    }
}
