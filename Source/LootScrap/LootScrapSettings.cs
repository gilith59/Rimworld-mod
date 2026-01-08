using Verse;

namespace LootScrap
{
    public class LootScrapSettings : ModSettings
    {
        // Enable/disable features
        public bool enableScrapSystem = true;
        public bool preserveInventory = true;
        public bool preserveUniqueItems = true;

        // Scrap costs (silver value per scrap)
        public int junkScrapCost = 100;           // 1 Junk Scrap = 100 silver
        public int goodScrapCost = 300;           // 1 Good Scrap = 300 silver
        public int highScrapCost = 500;           // 1 High Scrap = 500 silver
        public int glitterworldScrapCost = 1000;  // 1 Glitterworld Scrap = 1000 silver

        // Scrap limits
        public int maxScrapPerType = 3;           // Maximum 3 scraps of the same type
        public int maxTotalScrapPerPawn = 5;      // Maximum 5 scraps total per pawn

        // Scrap yields (per scrap) - SMELTER
        // Junk
        public int junkSteelYield = 10;

        // Good Quality
        public int goodSteelYield = 20;

        // High Quality
        public int highSteelYield = 20;
        public int highPlasteelYield = 5;

        // Glitterworld
        public int glitterworldPlasteelYield = 20;
        public int glitterworldGoldYield = 3;
        public int glitterworldUraniumYield = 3;

        // Scrap yields - MACHINING
        public int junkComponentYield = 0;           // 3 junk = 1 component (handled in recipe)
        public int goodComponentYield = 1;
        public int highComponentYield = 2;
        public int glitterworldComponentYield = 3;   // Alternative to Advanced Component
        public int glitterworldAdvancedComponentYield = 1;
        public bool glitterworldDropsAdvancedComponent = true;  // Toggle between AC or 3x components

        // Filters
        public bool scrapWeapons = true;
        public bool scrapApparel = true;
        public bool onlyScrapHostiles = true;
        public bool scrapFromCorpsesOnly = false;
        public bool scrapDownedWhenStripped = true;     // Downed hostiles drop scrap when stripped
        public bool scrapPrisonersWhenStripped = true;  // Prisoners drop scrap when stripped
        public bool scrapFoundCorpses = true;           // Convert pre-existing corpses found on map (ruins, etc)

        public override void ExposeData()
        {
            base.ExposeData();

            // Features
            Scribe_Values.Look(ref enableScrapSystem, "enableScrapSystem", true);
            Scribe_Values.Look(ref preserveInventory, "preserveInventory", true);
            Scribe_Values.Look(ref preserveUniqueItems, "preserveUniqueItems", true);

            // Scrap costs
            Scribe_Values.Look(ref junkScrapCost, "junkScrapCost", 100);
            Scribe_Values.Look(ref goodScrapCost, "goodScrapCost", 300);
            Scribe_Values.Look(ref highScrapCost, "highScrapCost", 500);
            Scribe_Values.Look(ref glitterworldScrapCost, "glitterworldScrapCost", 1000);

            // Scrap limits
            Scribe_Values.Look(ref maxScrapPerType, "maxScrapPerType", 3);
            Scribe_Values.Look(ref maxTotalScrapPerPawn, "maxTotalScrapPerPawn", 5);

            // Yields - Smelter
            Scribe_Values.Look(ref junkSteelYield, "junkSteelYield", 10);
            Scribe_Values.Look(ref goodSteelYield, "goodSteelYield", 20);
            Scribe_Values.Look(ref highSteelYield, "highSteelYield", 20);
            Scribe_Values.Look(ref highPlasteelYield, "highPlasteelYield", 5);
            Scribe_Values.Look(ref glitterworldPlasteelYield, "glitterworldPlasteelYield", 20);
            Scribe_Values.Look(ref glitterworldGoldYield, "glitterworldGoldYield", 3);
            Scribe_Values.Look(ref glitterworldUraniumYield, "glitterworldUraniumYield", 3);

            // Yields - Machining
            Scribe_Values.Look(ref junkComponentYield, "junkComponentYield", 0);
            Scribe_Values.Look(ref goodComponentYield, "goodComponentYield", 1);
            Scribe_Values.Look(ref highComponentYield, "highComponentYield", 2);
            Scribe_Values.Look(ref glitterworldComponentYield, "glitterworldComponentYield", 3);
            Scribe_Values.Look(ref glitterworldAdvancedComponentYield, "glitterworldAdvancedComponentYield", 1);
            Scribe_Values.Look(ref glitterworldDropsAdvancedComponent, "glitterworldDropsAdvancedComponent", true);

            // Filters
            Scribe_Values.Look(ref scrapWeapons, "scrapWeapons", true);
            Scribe_Values.Look(ref scrapApparel, "scrapApparel", true);
            Scribe_Values.Look(ref onlyScrapHostiles, "onlyScrapHostiles", true);
            Scribe_Values.Look(ref scrapFromCorpsesOnly, "scrapFromCorpsesOnly", false);
            Scribe_Values.Look(ref scrapDownedWhenStripped, "scrapDownedWhenStripped", true);
            Scribe_Values.Look(ref scrapPrisonersWhenStripped, "scrapPrisonersWhenStripped", true);
            Scribe_Values.Look(ref scrapFoundCorpses, "scrapFoundCorpses", true);
        }
    }
}
