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
            Log.Message($"[LootScrap] InitializePawnBatch for {pawn.LabelShort}");
            if (!pawnBatches.ContainsKey(pawn))
            {
                pawnBatches[pawn] = new List<Thing>();
                Log.Message($"[LootScrap] Created new batch for {pawn.LabelShort}");
            }
            else
            {
                Log.Message($"[LootScrap] Batch already exists for {pawn.LabelShort} with {pawnBatches[pawn].Count} items");
            }
        }

        /// <summary>
        /// Add an item to a pawn's batch
        /// </summary>
        public static void AddItemToBatch(Pawn pawn, Thing item)
        {
            Log.Message($"[LootScrap] AddItemToBatch: Adding {item.LabelShort} to {pawn.LabelShort}'s batch");
            if (!pawnBatches.ContainsKey(pawn))
            {
                pawnBatches[pawn] = new List<Thing>();
                Log.Message($"[LootScrap] Created new batch for {pawn.LabelShort} (was missing)");
            }
            pawnBatches[pawn].Add(item);
            Log.Message($"[LootScrap] Batch for {pawn.LabelShort} now has {pawnBatches[pawn].Count} items");
        }

        /// <summary>
        /// Finalize and convert all items in a pawn's batch
        /// </summary>
        public static void FinalizePawnBatch(Pawn pawn)
        {
            Log.Message($"[LootScrap] FinalizePawnBatch for {pawn.LabelShort}");

            if (!pawnBatches.ContainsKey(pawn))
            {
                Log.Message($"[LootScrap] No batch found for {pawn.LabelShort} - nothing to finalize");
                return;
            }

            List<Thing> items = pawnBatches[pawn];
            pawnBatches.Remove(pawn);

            Log.Message($"[LootScrap] Batch for {pawn.LabelShort} contains {items.Count} items");

            if (items.Count == 0)
            {
                Log.Message($"[LootScrap] Batch is empty - nothing to convert");
                return;
            }

            // Log all items in batch
            foreach (var item in items)
            {
                Log.Message($"[LootScrap] Batch item: {item.LabelShort}");
            }

            // Convert all items as a batch
            Log.Message($"[LootScrap] Converting batch to scrap at {pawn.PositionHeld}");
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
            Log.Message($"[LootScrap] ConvertBatchToScrap called with {items?.Count ?? 0} items at {position}");

            if (items == null || items.Count == 0 || map == null)
            {
                Log.Message($"[LootScrap] ConvertBatchToScrap: Early return - items null={items == null}, count={items?.Count ?? 0}, map null={map == null}");
                return;
            }

            // Calculate TOTAL value across ALL items
            float totalValue = 0f;
            foreach (Thing item in items)
            {
                Log.Message($"[LootScrap] Processing item for value calculation: {item.LabelShort}");
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
                Log.Message($"[LootScrap] Item {item.LabelShort}: base={baseValue}, final={itemValue}, total so far={totalValue}");
            }

            Log.Message($"[LootScrap] Total value calculated: {totalValue} silvers");

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

            Log.Message($"[LootScrap] Greedy algorithm results: {scrapCounts.Count} scrap types, {totalScraps} total scraps");
            foreach (var kvp in scrapCounts)
            {
                Log.Message($"[LootScrap] Scrap: {kvp.Key.defName} x{kvp.Value}");
            }

            // Destroy ALL items in the batch
            Log.Message($"[LootScrap] Destroying {items.Count} items");
            foreach (Thing item in items)
            {
                if (!item.Destroyed)
                {
                    Log.Message($"[LootScrap] Destroying {item.LabelShort}");
                    item.Destroy(DestroyMode.Vanish);
                }
            }

            // Spawn scrap stacks
            Log.Message($"[LootScrap] Spawning {scrapCounts.Count} scrap stacks");
            foreach (var kvp in scrapCounts)
            {
                Thing scrap = ThingMaker.MakeThing(kvp.Key);
                scrap.stackCount = kvp.Value;
                Log.Message($"[LootScrap] Spawning {kvp.Value}x {kvp.Key.defName} at {position}");

                // Use TryPlaceThing to automatically merge with existing stacks
                GenPlace.TryPlaceThing(scrap, position, map, ThingPlaceMode.Near);
            }
        }

    }
}
