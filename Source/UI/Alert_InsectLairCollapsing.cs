using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using System.Linq;

namespace InsectLairIncident
{
    public class Alert_InsectLairCollapsing : Alert_Critical
    {
        private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

        private MapPortal Portal => targets.Count > 0 ? targets[0].Thing as MapPortal : null;

        protected override Color BGColor
        {
            get
            {
                MapComponent_HiveQueenTracker tracker = GetTracker();
                if (tracker != null)
                {
                    int ticksRemaining = tracker.GetTicksUntilCollapse();
                    // Stage 1: > 2h remaining = yellow (clear background) pour un timer de 6h
                    if (ticksRemaining > 5000) // 2h = 5000 ticks
                    {
                        return Color.clear;
                    }
                }
                // Stage 2: < 2h remaining = red background
                return base.BGColor;
            }
        }

        public Alert_InsectLairCollapsing()
        {
            defaultLabel = "Insect Lair Collapsing";
            defaultExplanation = "The insect lair is unstable and will collapse soon! All pawns inside the lair will be crushed when it collapses. Evacuate immediately!";
        }

        private MapComponent_HiveQueenTracker GetTracker()
        {
            MapPortal portal = Portal;
            if (portal?.PocketMap != null)
            {
                return portal.PocketMap.GetComponent<MapComponent_HiveQueenTracker>();
            }
            return null;
        }

        private void CalculateTargets()
        {
            targets.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Thing thing in maps[i].listerThings.ThingsOfDef(InsectLairDefOf.InsectLairEntrance))
                {
                    MapPortal portal = thing as MapPortal;
                    if (portal != null && portal.PocketMap != null)
                    {
                        MapComponent_HiveQueenTracker tracker = portal.PocketMap.GetComponent<MapComponent_HiveQueenTracker>();
                        if (tracker != null && tracker.IsBossDead() && tracker.GetTicksUntilCollapse() > 0)
                        {
                            targets.Add(portal);
                        }
                    }
                }
            }
        }

        public override string GetLabel()
        {
            MapComponent_HiveQueenTracker tracker = GetTracker();
            if (tracker != null)
            {
                int ticksRemaining = tracker.GetTicksUntilCollapse();
                return defaultLabel + ": " + ticksRemaining.ToStringTicksToPeriodVerbose();
            }
            return defaultLabel;
        }

        public override AlertReport GetReport()
        {
            CalculateTargets();
            return AlertReport.CulpritsAre(targets);
        }
    }
}
