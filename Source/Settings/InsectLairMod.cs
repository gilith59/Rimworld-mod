using UnityEngine;
using Verse;
using System;

namespace InsectLairIncident
{
    public class InsectLairMod : Mod
    {
        private InsectLairSettings settings;
        private Vector2 scrollPosition = Vector2.zero;

        public InsectLairMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<InsectLairSettings>();
        }

        public override string SettingsCategory() => "Insect Lair Incident";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 30f, 1400f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);

            // Header
            Text.Font = GameFont.Medium;
            listingStandard.Label("Insect Lair Incident Settings");
            Text.Font = GameFont.Small;
            listingStandard.Gap(12f);

            // Important note at the top
            listingStandard.Label("<color=#87CEEB>ℹ️ Settings apply to new lairs (no restart required):</color>");
            listingStandard.Label("  • Threat Points Multiplier");
            listingStandard.Label("  • Wave Interval");
            listingStandard.Label("  • Auto-Collapse Delay");
            listingStandard.Label("  • VFE Insectoids Integration");
            listingStandard.Gap(6f);
            listingStandard.Label("<color=#ffff00>⚠️ Existing lairs keep their original settings.</color>");
            listingStandard.Gap(12f);

            // THREAT POINTS SECTION
            listingStandard.Gap(6f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("Threat Points");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);

            listingStandard.Label($"Threat Points Multiplier: {settings.threatPointsMultiplier:P0} ({(settings.threatPointsMultiplier * 100):F0}%)");
            listingStandard.Label("Controls the size of spawned insect waves based on colony threat points.");
            settings.threatPointsMultiplier = listingStandard.Slider(settings.threatPointsMultiplier, 0.25f, 3.0f);
            listingStandard.Gap(12f);

            // WAVE SPAWNING SECTION
            listingStandard.Gap(6f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("Wave Spawning");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);

            float waveIntervalHours = settings.waveIntervalTicks / 2500f;
            float waveIntervalDays = waveIntervalHours / 24f;
            listingStandard.Label($"Wave Interval: {waveIntervalDays:F1} days ({waveIntervalHours:F1} hours, {settings.waveIntervalTicks} ticks)");
            listingStandard.Label("Time between each insect wave spawn from the lair.");

            // Slider en jours (0.5 à 5 jours)
            waveIntervalDays = listingStandard.Slider(waveIntervalDays, 0.5f, 5f);
            settings.waveIntervalTicks = (int)(waveIntervalDays * 24 * 2500);
            listingStandard.Gap(12f);

            // AUTO-COLLAPSE SECTION
            listingStandard.Gap(6f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("Auto-Collapse");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);

            float collapseHours = settings.autoCollapseDelayTicks / 2500f;
            float collapseDays = collapseHours / 24f;
            listingStandard.Label($"Auto-Collapse Delay: {collapseDays:F1} days ({collapseHours:F1} hours, {settings.autoCollapseDelayTicks} ticks)");
            listingStandard.Label("Time after boss death before the lair automatically collapses.");

            // Slider en jours (1 à 10 jours)
            collapseDays = listingStandard.Slider(collapseDays, 1f, 10f);
            settings.autoCollapseDelayTicks = (int)(collapseDays * 24 * 2500);
            listingStandard.Gap(12f);

            // VFE INTEGRATION SECTION
            listingStandard.Gap(6f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("VFE Insectoids Integration");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);

            listingStandard.CheckboxLabeled(
                "Disable VFE Insectoids (Force Vanilla)",
                ref settings.disableVFEInsects,
                "When enabled, lairs will always use vanilla insects even if VFE Insectoids 2 is installed."
            );
            listingStandard.Gap(12f);

            // PRESETS SECTION
            listingStandard.Gap(6f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("Presets");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);

            if (listingStandard.ButtonText("Easy Mode (Default - Longer delays, less threat)"))
            {
                settings.threatPointsMultiplier = 0.5f;
                settings.waveIntervalTicks = 90000; // 1.5 jours
                settings.autoCollapseDelayTicks = 240000; // 96 heures (4 jours)
            }

            if (listingStandard.ButtonText("Normal Mode (Balanced)"))
            {
                settings.threatPointsMultiplier = 0.75f;
                settings.waveIntervalTicks = 60000; // 1 jour
                settings.autoCollapseDelayTicks = 180000; // 72 heures (3 jours)
            }

            if (listingStandard.ButtonText("Hard Mode (Shorter delays, more threat)"))
            {
                settings.threatPointsMultiplier = 1.0f;
                settings.waveIntervalTicks = 45000; // 18 heures
                settings.autoCollapseDelayTicks = 120000; // 48 heures (2 jours)
            }

            if (listingStandard.ButtonText("Extreme Mode (Chaos)"))
            {
                settings.threatPointsMultiplier = 1.5f;
                settings.waveIntervalTicks = 30000; // 12 heures
                settings.autoCollapseDelayTicks = 60000; // 24 heures (1 jour)
            }

            listingStandard.Gap(12f);

            // End of settings marker
            listingStandard.Gap(12f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("<color=#00ff00>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            listingStandard.Label("<color=#00ff00>✓ End of Settings</color>");
            listingStandard.Label("<color=#00ff00>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);
            listingStandard.Label("All settings have been displayed. You can scroll up to review them.");

            listingStandard.End();
            Widgets.EndScrollView();
            base.DoSettingsWindowContents(inRect);
        }

        public static InsectLairSettings GetSettings()
        {
            return LoadedModManager.GetMod<InsectLairMod>().GetSettings<InsectLairSettings>();
        }
    }
}
