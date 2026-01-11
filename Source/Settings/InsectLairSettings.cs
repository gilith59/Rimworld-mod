using UnityEngine;
using Verse;

namespace InsectLairIncident
{
    public class InsectLairSettings : ModSettings
    {
        // Threat points
        public float threatPointsMultiplier = 0.75f; // 75% par défaut (reduced from 100% based on beta feedback)

        // Wave spawning
        public int waveIntervalTicks = 60000; // 1 jour par défaut (production)

        // Auto-collapse
        public int autoCollapseDelayTicks = 180000; // 72 heures par défaut (production)

        // VFE Integration
        public bool useVFEForVanillaLairs = true; // Activer VFE pour lairs vanilla

        public override void ExposeData()
        {
            Scribe_Values.Look(ref threatPointsMultiplier, "threatPointsMultiplier", 0.75f);
            Scribe_Values.Look(ref waveIntervalTicks, "waveIntervalTicks", 60000);
            Scribe_Values.Look(ref autoCollapseDelayTicks, "autoCollapseDelayTicks", 180000);
            Scribe_Values.Look(ref useVFEForVanillaLairs, "useVFEForVanillaLairs", true);
            base.ExposeData();
        }
    }
}
