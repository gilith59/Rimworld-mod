using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace InsectLairIncident
{
    // Détecte et enregistre la HiveQueen vanilla après génération
    public class GenStep_TrackVanillaHiveQueen : GenStep
    {
        public override int SeedPart => 123456790;

        public override void Generate(Map map, GenStepParams parms)
        {
            // Récupérer la geneline depuis le GameComponent global
            // Utiliser le portal ID en cours de génération
            GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
            int portalID = MapPortalLinkHelper.currentGeneratingPortalID;
            GenelineData geneline = (portalID >= 0) ? globalComp?.GetGeneline(portalID) : null;

            if (geneline == null)
            {
                Log.Warning("[InsectLairIncident] No geneline found - using vanilla fallback");
                geneline = new GenelineData
                {
                    boss = PawnKindDef.Named("HiveQueen"),
                    defName = "Vanilla",
                    isVanilla = true
                };
            }
            else
            {
                Log.Warning($"[InsectLairIncident] Using geneline: {geneline.defName} (Boss: {geneline.boss.defName})");
            }

            // Chercher la HiveQueen qui a été spawné par vanilla GenStep_InsectLairCave
            Pawn queen = map.mapPawns.AllPawnsSpawned
                .FirstOrDefault(p => p.kindDef?.defName == "HiveQueen" || p.kindDef?.defName == "Insect_MegaspiderQueen");

            if (queen != null)
            {
                // Remplacer par le boss de la geneline si ce n'est pas vanilla
                if (!geneline.isVanilla && geneline.boss != null)
                {
                    IntVec3 queenPos = queen.Position;
                    // Log.Message($"[InsectLairIncident] Replacing vanilla HiveQueen with {geneline.boss.defName}");

                    // Détruire la vanilla queen
                    queen.Destroy();

                    // Spawner le boss de la geneline
                    Pawn boss = PawnGenerator.GeneratePawn(geneline.boss, Faction.OfInsects);
                    GenSpawn.Spawn(boss, queenPos, map);
                    queen = boss;

                    // Log.Message($"[InsectLairIncident] Spawned {geneline.boss.defName} at {queenPos}");
                }
            }
            else
            {
                Log.Warning("[InsectLairIncident] No HiveQueen found in InsectLair - trying to spawn manually");

                // Essayer de spawner manuellement le boss au centre de la map
                IntVec3 center = map.Center;
                if (CellFinder.TryFindRandomCellNear(center, map, 20, c => c.Standable(map) && !c.Fogged(map), out IntVec3 spawnCell))
                {
                    queen = PawnGenerator.GeneratePawn(geneline.boss, Faction.OfInsects);
                    GenSpawn.Spawn(queen, spawnCell, map);
                    // Log.Message($"[InsectLairIncident] Manually spawned {geneline.boss.defName} at {spawnCell}");
                }
            }

            if (queen == null)
            {
                Log.Error("[InsectLairIncident] Failed to spawn or find boss!");
                return;
            }

            // Remplacer les hives vanilla par les hives VFE si nécessaire
            if (!geneline.isVanilla)
            {
                ReplaceVanillaHivesWithVFE(map, geneline);
            }

            // Enregistrer dans MapComponent
            MapComponent_HiveQueenTracker tracker = map.GetComponent<MapComponent_HiveQueenTracker>();
            if (tracker == null)
            {
                tracker = new MapComponent_HiveQueenTracker(map);
                map.components.Add(tracker);
            }

            // Trouver la map parent (colonie) pour l'auto-collapse
            Map parentMap = Find.Maps.FirstOrDefault(m => m.IsPlayerHome);
            tracker.RegisterQueen(queen, parentMap);

            // Log.Message($"[InsectLairIncident] Registered {queen.kindDef.defName} ({geneline.defName} geneline) at {queen.Position}");
        }

        private void ReplaceVanillaHivesWithVFE(Map map, GenelineData geneline)
        {
            // Trouver toutes les hives vanilla sur la map
            List<Thing> vanillaHives = map.listerThings.ThingsOfDef(ThingDefOf.Hive).ToList();

            if (vanillaHives.Count == 0)
            {
                Log.Warning("[InsectLairIncident] No vanilla hives found to replace");
                return;
            }

            // Déterminer le ThingDef VFE correspondant
            ThingDef vfeHiveDef = GetVFEHiveForGeneline(geneline.defName);
            if (vfeHiveDef == null)
            {
                Log.Warning($"[InsectLairIncident] No VFE hive found for geneline {geneline.defName}");
                return;
            }

            Log.Message($"[InsectLairIncident] Replacing {vanillaHives.Count} vanilla hives with {vfeHiveDef.defName}");

            // Remplacer chaque hive
            foreach (Thing vanillaHive in vanillaHives)
            {
                IntVec3 position = vanillaHive.Position;
                Rot4 rotation = vanillaHive.Rotation;
                Faction faction = vanillaHive.Faction;

                // Détruire la vanilla hive
                vanillaHive.Destroy(DestroyMode.Vanish);

                // Spawner la hive VFE
                Thing vfeHive = ThingMaker.MakeThing(vfeHiveDef, null);
                vfeHive.SetFaction(faction);
                GenSpawn.Spawn(vfeHive, position, map, rotation);
            }

            Log.Message($"[InsectLairIncident] Successfully replaced all hives with {vfeHiveDef.defName}");
        }

        private ThingDef GetVFEHiveForGeneline(string genelineDefName)
        {
            string hiveDef = null;

            if (genelineDefName == "VFEI_Nuchadus")
                hiveDef = "VFEI2_NuchadusHive";
            else if (genelineDefName == "VFEI_Chelis")
                hiveDef = "VFEI2_ChelisHive";
            else if (genelineDefName == "VFEI_Kemia")
                hiveDef = "VFEI2_KemianHive";
            else if (genelineDefName == "VFEI_Xanides")
                hiveDef = "VFEI2_XanidesHive";
            // VFEI_Sorne utilise Hive vanilla

            if (hiveDef != null)
            {
                return DefDatabase<ThingDef>.GetNamedSilentFail(hiveDef);
            }
            return null;
        }
    }
}
