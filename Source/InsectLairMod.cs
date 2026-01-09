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
            listingStandard.Label("<color=#87CEEB>ℹ️ Settings that apply immediately (no restart required):</color>");
            listingStandard.Label("  • Threat Points Multiplier (for new lairs)");
            listingStandard.Label("  • Wave Interval (for new lairs)");
            listingStandard.Label("  • Auto-Collapse Delay (for new lairs)");
            listingStandard.Label("  • VFE Integration (for new lairs)");
            listingStandard.Label("  • Incident Frequency");
            listingStandard.Gap(6f);
            listingStandard.Label("<color=#ffff00>⚠️ Existing lairs use settings from when they spawned.</color>");
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
                "Use VFE genelines for vanilla InsectLairs",
                ref settings.useVFEForVanillaLairs,
                "When enabled, naturally spawning vanilla InsectLairs will use random VFE Insectoids genelines instead of vanilla queens."
            );
            listingStandard.Gap(12f);

            // INCIDENT FREQUENCY SECTION
            listingStandard.Gap(6f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("Incident Frequency");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);

            listingStandard.Label($"Earliest Day: {settings.earliestDay} days");
            listingStandard.Label("Minimum number of days before the first InsectLair incident can occur.");
            settings.earliestDay = (int)listingStandard.Slider(settings.earliestDay, 10, 120);
            listingStandard.Gap(6f);

            listingStandard.Label($"Minimum Refire Delay: {settings.minRefireDays} days");
            listingStandard.Label("Minimum days between InsectLair incident occurrences.");
            settings.minRefireDays = (int)listingStandard.Slider(settings.minRefireDays, 15, 90);
            listingStandard.Gap(12f);

            // PRESETS SECTION
            listingStandard.Gap(6f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("Presets");
            Text.Font = GameFont.Small;
            listingStandard.Gap(6f);

            if (listingStandard.ButtonText("Easy Mode (Longer delays, less threat)"))
            {
                settings.threatPointsMultiplier = 0.75f;
                settings.waveIntervalTicks = 90000; // 1.5 jours
                settings.autoCollapseDelayTicks = 240000; // 96 heures (4 jours)
                settings.earliestDay = 90;
                settings.minRefireDays = 60;
            }

            if (listingStandard.ButtonText("Normal Mode (Balanced)"))
            {
                settings.threatPointsMultiplier = 1.0f;
                settings.waveIntervalTicks = 60000; // 1 jour
                settings.autoCollapseDelayTicks = 180000; // 72 heures (3 jours)
                settings.earliestDay = 60;
                settings.minRefireDays = 45;
            }

            if (listingStandard.ButtonText("Hard Mode (Shorter delays, more threat)"))
            {
                settings.threatPointsMultiplier = 1.5f;
                settings.waveIntervalTicks = 30000; // 12 heures
                settings.autoCollapseDelayTicks = 120000; // 48 heures (2 jours)
                settings.earliestDay = 30;
                settings.minRefireDays = 30;
            }

            if (listingStandard.ButtonText("Extreme Mode (Chaos)"))
            {
                settings.threatPointsMultiplier = 2.0f;
                settings.waveIntervalTicks = 15000; // 6 heures
                settings.autoCollapseDelayTicks = 60000; // 24 heures (1 jour)
                settings.earliestDay = 20;
                settings.minRefireDays = 20;
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
