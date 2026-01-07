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

        // Scrap yields (per scrap)
        // Junk
        public int junkSteelYield = 15;

        // Good Quality
        public int goodSteelYield = 35;
        public int goodComponentYield = 1;

        // High Quality
        public int highPlasteelYield = 15;
        public int highComponentYield = 2;

        // Glitterworld
        public int glitterworldPlasteelYield = 25;
        public int glitterworldUraniumYield = 5;
        public int glitterworldAdvancedComponentYield = 1;

        // Filters
        public bool scrapWeapons = true;
        public bool scrapApparel = true;
        public bool onlyScrapHostiles = true;
        public bool scrapFromCorpsesOnly = false;

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

            // Yields - Junk
            Scribe_Values.Look(ref junkSteelYield, "junkSteelYield", 15);

            // Yields - Good
            Scribe_Values.Look(ref goodSteelYield, "goodSteelYield", 35);
            Scribe_Values.Look(ref goodComponentYield, "goodComponentYield", 1);

            // Yields - High
            Scribe_Values.Look(ref highPlasteelYield, "highPlasteelYield", 15);
            Scribe_Values.Look(ref highComponentYield, "highComponentYield", 2);

            // Yields - Glitterworld
            Scribe_Values.Look(ref glitterworldPlasteelYield, "glitterworldPlasteelYield", 25);
            Scribe_Values.Look(ref glitterworldUraniumYield, "glitterworldUraniumYield", 5);
            Scribe_Values.Look(ref glitterworldAdvancedComponentYield, "glitterworldAdvancedComponentYield", 1);

            // Filters
            Scribe_Values.Look(ref scrapWeapons, "scrapWeapons", true);
            Scribe_Values.Look(ref scrapApparel, "scrapApparel", true);
            Scribe_Values.Look(ref onlyScrapHostiles, "onlyScrapHostiles", true);
            Scribe_Values.Look(ref scrapFromCorpsesOnly, "scrapFromCorpsesOnly", false);
        }
    }
}
