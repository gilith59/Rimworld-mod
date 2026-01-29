using UnityEngine;
using Verse;

namespace InsectLairIncident
{
    public class InsectLairSettings : ModSettings
    {
        // Threat points - EASY difficulty by default
        public float threatPointsMultiplier = 0.5f; // 50% par défaut (easy mode)

        // Wave spawning
        public int waveIntervalTicks = 60000; // 1 jour par défaut (production)

        // Auto-collapse
        public int autoCollapseDelayTicks = 180000; // 72 heures par défaut (production)

        // VFE Integration
        public bool disableVFEInsects = false; // Si true, utilise vanilla même avec VFE installé

        public override void ExposeData()
        {
            Scribe_Values.Look(ref threatPointsMultiplier, "threatPointsMultiplier", 0.5f);
            Scribe_Values.Look(ref waveIntervalTicks, "waveIntervalTicks", 60000);
            Scribe_Values.Look(ref autoCollapseDelayTicks, "autoCollapseDelayTicks", 180000);
            Scribe_Values.Look(ref disableVFEInsects, "disableVFEInsects", false);
            base.ExposeData();
        }
    }
}
