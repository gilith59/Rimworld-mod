using UnityEngine;
using Verse;

namespace LootScrap
{
    public class LootScrapMod : Mod
    {
        private LootScrapSettings settings;
        private Vector2 scrollPosition;

        public LootScrapMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<LootScrapSettings>();
        }

        public override string SettingsCategory() => "Loot Scrap";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 30f, 1800f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);

            // === RESTART NOTE ===
            listingStandard.Label("<color=#ffff00>⚠️ Note: Recipe yield changes require game restart to take effect</color>");
            listingStandard.Gap(12f);

            // === GENERAL SETTINGS ===
            listingStandard.Label("<b>General Settings</b>");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Enable Scrap System", ref settings.enableScrapSystem,
                "Enable or disable the entire scrap system");
            listingStandard.CheckboxLabeled("Preserve Inventory Items", ref settings.preserveInventory,
                "Inventory items (not equipped) are never converted to scrap");
            listingStandard.CheckboxLabeled("Preserve Unique Items", ref settings.preserveUniqueItems,
                "Legendary, Artifact, and Named items are never converted to scrap");

            listingStandard.Gap(20f);

            // === EQUIPMENT FILTERS ===
            listingStandard.Label("<b>Equipment Filters</b>");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Scrap Weapons", ref settings.scrapWeapons);
            listingStandard.CheckboxLabeled("Scrap Apparel", ref settings.scrapApparel);
            listingStandard.CheckboxLabeled("Only Scrap Hostiles", ref settings.onlyScrapHostiles,
                "Only scrap equipment from hostile faction pawns");
            listingStandard.CheckboxLabeled("Scrap From Corpses Only", ref settings.scrapFromCorpsesOnly,
                "Only scrap equipment when pawn is dead (not from live pawns)");
            listingStandard.CheckboxLabeled("Scrap Downed When Stripped", ref settings.scrapDownedWhenStripped,
                "Downed hostile pawns drop scrap when manually stripped (not just when dead)");
            listingStandard.CheckboxLabeled("Scrap Prisoners When Stripped", ref settings.scrapPrisonersWhenStripped,
                "Prisoners drop scrap when stripped instead of keeping their equipment");
            listingStandard.CheckboxLabeled("Scrap Found Corpses", ref settings.scrapFoundCorpses,
                "Pre-existing corpses found on map (ruins, ancient dangers, etc) get scrapped when discovered");

            listingStandard.Gap(20f);

            // === SCRAP COSTS ===
            listingStandard.Label("<b>Scrap Costs (Silver per scrap)</b>");
            listingStandard.Gap();

            listingStandard.Label($"Junk Scrap: {settings.junkScrapCost} silver");
            settings.junkScrapCost = (int)listingStandard.Slider(settings.junkScrapCost, 50f, 500f);

            listingStandard.Label($"Good Scrap: {settings.goodScrapCost} silver");
            settings.goodScrapCost = (int)listingStandard.Slider(settings.goodScrapCost, 100f, 1000f);

            listingStandard.Label($"High Scrap: {settings.highScrapCost} silver");
            settings.highScrapCost = (int)listingStandard.Slider(settings.highScrapCost, 200f, 2000f);

            listingStandard.Label($"Glitterworld Scrap: {settings.glitterworldScrapCost} silver");
            settings.glitterworldScrapCost = (int)listingStandard.Slider(settings.glitterworldScrapCost, 500f, 5000f);

            listingStandard.Gap();

            listingStandard.Label($"Max scraps per type: {settings.maxScrapPerType}");
            settings.maxScrapPerType = (int)listingStandard.Slider(settings.maxScrapPerType, 1f, 10f);

            listingStandard.Label($"Max total scraps per pawn: {settings.maxTotalScrapPerPawn}");
            settings.maxTotalScrapPerPawn = (int)listingStandard.Slider(settings.maxTotalScrapPerPawn, 1f, 20f);

            listingStandard.Gap(20f);

            // === SMELTER YIELDS ===
            listingStandard.Label("<b>Smelter Yields (Electric Smelter)</b>");
            listingStandard.Gap();

            listingStandard.Label("Junk Scrap:");
            listingStandard.Label($"  Steel: {settings.junkSteelYield}");
            settings.junkSteelYield = (int)listingStandard.Slider(settings.junkSteelYield, 5f, 50f);

            listingStandard.Gap();

            listingStandard.Label("Good Quality Scrap:");
            listingStandard.Label($"  Steel: {settings.goodSteelYield}");
            settings.goodSteelYield = (int)listingStandard.Slider(settings.goodSteelYield, 10f, 100f);

            listingStandard.Gap();

            listingStandard.Label("High Quality Scrap:");
            listingStandard.Label($"  Steel: {settings.highSteelYield}");
            settings.highSteelYield = (int)listingStandard.Slider(settings.highSteelYield, 10f, 100f);
            listingStandard.Label($"  Plasteel: {settings.highPlasteelYield}");
            settings.highPlasteelYield = (int)listingStandard.Slider(settings.highPlasteelYield, 0f, 50f);

            listingStandard.Gap();

            listingStandard.Label("Glitterworld Scrap:");
            listingStandard.Label($"  Plasteel: {settings.glitterworldPlasteelYield}");
            settings.glitterworldPlasteelYield = (int)listingStandard.Slider(settings.glitterworldPlasteelYield, 10f, 100f);
            listingStandard.Label($"  Gold: {settings.glitterworldGoldYield}");
            settings.glitterworldGoldYield = (int)listingStandard.Slider(settings.glitterworldGoldYield, 0f, 20f);
            listingStandard.Label($"  Uranium: {settings.glitterworldUraniumYield}");
            settings.glitterworldUraniumYield = (int)listingStandard.Slider(settings.glitterworldUraniumYield, 0f, 20f);

            listingStandard.Gap(20f);

            // === MACHINING YIELDS ===
            listingStandard.Label("<b>Machining Yields (Machining Table)</b>");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Require Fabrication Research", ref settings.requireFabricationForMachining,
                "If enabled, machining recipes require Fabrication research to unlock");

            listingStandard.Gap();

            listingStandard.Label("Junk Scrap: 3x → 1 component (fixed ratio)");
            listingStandard.Gap();

            listingStandard.Label("Good Quality Scrap:");
            listingStandard.Label($"  Components: {settings.goodComponentYield}");
            settings.goodComponentYield = (int)listingStandard.Slider(settings.goodComponentYield, 0f, 5f);

            listingStandard.Gap();

            listingStandard.Label("High Quality Scrap:");
            listingStandard.Label($"  Components: {settings.highComponentYield}");
            settings.highComponentYield = (int)listingStandard.Slider(settings.highComponentYield, 0f, 10f);

            listingStandard.Gap();

            listingStandard.Label("Glitterworld Scrap:");
            listingStandard.CheckboxLabeled("Give Advanced Component", ref settings.glitterworldDropsAdvancedComponent,
                "If enabled: 1 Advanced Component. If disabled: 3 Components");
            listingStandard.Label($"  Advanced Components (if enabled): {settings.glitterworldAdvancedComponentYield}");
            settings.glitterworldAdvancedComponentYield = (int)listingStandard.Slider(settings.glitterworldAdvancedComponentYield, 0f, 10f);
            listingStandard.Label($"  Components (if disabled): {settings.glitterworldComponentYield}");
            settings.glitterworldComponentYield = (int)listingStandard.Slider(settings.glitterworldComponentYield, 0f, 10f);

            listingStandard.Gap(20f);

            // Reset button
            if (listingStandard.ButtonText("Reset to Defaults"))
            {
                settings = new LootScrapSettings();
            }

            listingStandard.Gap(20f);
            listingStandard.Label("<i>End of settings - All sliders are visible above</i>");

            listingStandard.End();
            Widgets.EndScrollView();

            base.DoSettingsWindowContents(inRect);
        }
    }
}
