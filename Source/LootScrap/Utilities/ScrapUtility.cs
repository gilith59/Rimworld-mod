using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace LootScrap
{
    public static class ScrapUtility
    {
        private static LootScrapSettings Settings => LoadedModManager.GetMod<LootScrapMod>().GetSettings<LootScrapSettings>();

        // Track items per pawn for batch conversion
        private static Dictionary<Pawn, List<Thing>> pawnBatches = new Dictionary<Pawn, List<Thing>>();

        /// <summary>
        /// Initialize batch tracking for a pawn
        /// </summary>
        public static void InitializePawnBatch(Pawn pawn)
        {
            if (!pawnBatches.ContainsKey(pawn))
            {
                pawnBatches[pawn] = new List<Thing>();
            }
        }

        /// <summary>
        /// Add an item to a pawn's batch
        /// </summary>
        public static void AddItemToBatch(Pawn pawn, Thing item)
        {
            if (!pawnBatches.ContainsKey(pawn))
            {
                pawnBatches[pawn] = new List<Thing>();
            }
            pawnBatches[pawn].Add(item);
        }

        /// <summary>
        /// Finalize and convert all items in a pawn's batch
        /// </summary>
        public static void FinalizePawnBatch(Pawn pawn)
        {
            if (!pawnBatches.ContainsKey(pawn))
            {
                return;
            }

            List<Thing> items = pawnBatches[pawn];
            pawnBatches.Remove(pawn);

            if (items.Count == 0)
            {
                return;
            }

            // Convert all items as a batch
            ConvertBatchToScrap(items, pawn.PositionHeld, pawn.MapHeld);
        }


        /// <summary>
        /// Checks if an item should be preserved (unique weapons, persona weapons, legendary items)
        /// Based on RimWorld Discord advice: check CompUniqueWeapon and CompBladelinkWeapon
        /// </summary>
        public static bool ShouldPreserveItem(Thing thing)
        {
            if (!Settings.preserveUniqueItems)
                return false;

            // Check for Odyssey unique weapons and Royalty persona weapons
            // These have special traits and word-generated names
            if (thing is ThingWithComps thingWithComps)
            {
                foreach (var comp in thingWithComps.AllComps)
                {
                    string compTypeName = comp.GetType().Name;
                    if (compTypeName == "CompUniqueWeapon" || compTypeName == "CompBladelinkWeapon")
                        return true;
                }
            }

            // Check quality - only preserve Legendary+
            if (thing.TryGetComp<CompQuality>() is CompQuality qualityComp)
            {
                if (qualityComp.Quality >= QualityCategory.Legendary)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if equipment should be scrapped based on settings
        /// </summary>
        public static bool ShouldScrapEquipment(Thing equipment, Pawn pawn)
        {
            if (!Settings.enableScrapSystem)
                return false;

            // Check if it's a weapon or apparel
            bool isWeapon = equipment.def.IsWeapon;
            bool isApparel = equipment.def.IsApparel;

            if (isWeapon && !Settings.scrapWeapons)
                return false;

            if (isApparel && !Settings.scrapApparel)
                return false;

            if (!isWeapon && !isApparel)
                return false;

            // Check if pawn is hostile
            if (Settings.onlyScrapHostiles)
            {
                if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer))
                    return false;
            }

            // Check if pawn is dead
            if (Settings.scrapFromCorpsesOnly && !pawn.Dead)
                return false;

            // Check if item should be preserved
            if (ShouldPreserveItem(equipment))
                return false;

            return true;
        }


        /// <summary>
        /// Convert a batch of items to scrap using greedy algorithm on TOTAL value
        /// </summary>
        private static void ConvertBatchToScrap(List<Thing> items, IntVec3 position, Map map)
        {
            if (items == null || items.Count == 0 || map == null)
                return;

            // Calculate TOTAL value across ALL items
            float totalValue = 0f;
            foreach (Thing item in items)
            {
                // Calculate value based on base price and quality only (ignore HP damage)
                float baseValue = item.def.BaseMarketValue;
                float itemValue = baseValue;

                // Apply quality multiplier
                if (item is ThingWithComps twc)
                {
                    var qc = twc.TryGetComp<CompQuality>();
                    if (qc != null)
                    {
                        // Quality multipliers from RimWorld
                        switch (qc.Quality)
                        {
                            case QualityCategory.Awful:     itemValue *= 0.5f; break;
                            case QualityCategory.Poor:      itemValue *= 0.75f; break;
                            case QualityCategory.Normal:    itemValue *= 1.0f; break;
                            case QualityCategory.Good:      itemValue *= 1.15f; break;
                            case QualityCategory.Excellent: itemValue *= 1.5f; break;
                            case QualityCategory.Masterwork: itemValue *= 2.5f; break;
                            case QualityCategory.Legendary: itemValue *= 5.0f; break;
                        }
                    }
                }

                itemValue *= item.stackCount;
                totalValue += itemValue;
            }

            // Greedy algorithm on TOTAL value
            Dictionary<ThingDef, int> scrapCounts = new Dictionary<ThingDef, int>();
            int totalScraps = 0;

            // Define scrap types in order from most to least expensive (using cached DefOf)
            ThingDef[] scrapDefs = new ThingDef[]
            {
                LootScrapDefOf.LootScrap_Glitterworld,
                LootScrapDefOf.LootScrap_High,
                LootScrapDefOf.LootScrap_Good,
                LootScrapDefOf.LootScrap_Junk
            };
            int[] scrapCosts = new int[]
            {
                Settings.glitterworldScrapCost,
                Settings.highScrapCost,
                Settings.goodScrapCost,
                Settings.junkScrapCost
            };

            // Greedy algorithm: reduce value by most expensive scrap possible
            float remainingValue = totalValue;
            for (int i = 0; i < scrapDefs.Length; i++)
            {
                ThingDef scrapDef = scrapDefs[i];
                int scrapCost = scrapCosts[i];

                if (totalScraps >= Settings.maxTotalScrapPerPawn)
                    break;

                int maxOfThisType = Mathf.Min(Settings.maxScrapPerType, Settings.maxTotalScrapPerPawn - totalScraps);
                int count = 0;

                while (remainingValue >= scrapCost && count < maxOfThisType && totalScraps < Settings.maxTotalScrapPerPawn)
                {
                    remainingValue -= scrapCost;
                    count++;
                    totalScraps++;
                }

                if (count > 0)
                {
                    scrapCounts[scrapDef] = count;
                }
            }

            // Destroy ALL items in the batch
            foreach (Thing item in items)
            {
                if (!item.Destroyed)
                {
                    item.Destroy(DestroyMode.Vanish);
                }
            }

            // Spawn scrap stacks
            foreach (var kvp in scrapCounts)
            {
                Thing scrap = ThingMaker.MakeThing(kvp.Key);
                scrap.stackCount = kvp.Value;

                // Use TryPlaceThing to automatically merge with existing stacks
                GenPlace.TryPlaceThing(scrap, position, map, ThingPlaceMode.Near);
            }
        }

    }
}
