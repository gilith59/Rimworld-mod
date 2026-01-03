using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace InsectLairIncident
{
    public class CompSealable_Conditional : CompSealable
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                Command cmd = gizmo as Command;
                if (cmd != null)
                {
                    // Chercher le tracker sur le pocket map (la cave), pas la surface
                    MapPortal portal = parent as MapPortal;
                    if (portal != null && portal.PocketMap != null)
                    {
                        MapComponent_HiveQueenTracker tracker = portal.PocketMap.GetComponent<MapComponent_HiveQueenTracker>();
                        if (tracker != null && !tracker.IsQueenDead())
                        {
                            cmd.Disable("Hive Queen must be defeated first");
                        }
                    }
                }
                yield return gizmo;
            }
        }
    }

    public class CompProperties_Sealable_Conditional : CompProperties_Sealable
    {
        public CompProperties_Sealable_Conditional()
        {
            compClass = typeof(CompSealable_Conditional);
        }
    }
}
