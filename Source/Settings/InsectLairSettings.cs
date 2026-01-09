using UnityEngine;
using Verse;

namespace InsectLairIncident
{
    public class InsectLairSettings : ModSettings
    {
        // Threat points
        public float threatPointsMultiplier = 1.0f; // 100% par défaut

        // Wave spawning
        public int waveIntervalTicks = 60000; // 1 jour par défaut (production)

        // Auto-collapse
        public int autoCollapseDelayTicks = 180000; // 72 heures par défaut (production)

        // VFE Integration
        public bool useVFEForVanillaLairs = true; // Activer VFE pour lairs vanilla

        public override void ExposeData()
        {
            Scribe_Values.Look(ref threatPointsMultiplier, "threatPointsMultiplier", 1.0f);
            Scribe_Values.Look(ref waveIntervalTicks, "waveIntervalTicks", 60000);
            Scribe_Values.Look(ref autoCollapseDelayTicks, "autoCollapseDelayTicks", 180000);
            Scribe_Values.Look(ref useVFEForVanillaLairs, "useVFEForVanillaLairs", true);
            base.ExposeData();
        }
    }
}
