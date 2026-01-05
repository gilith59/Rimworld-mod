using RimWorld;
using Verse;
using System.Linq;

namespace InsectLairIncident
{
    // Détecte et enregistre la HiveQueen vanilla après génération
    public class GenStep_TrackVanillaHiveQueen : GenStep
    {
        public override int SeedPart => 123456790;

        public override void Generate(Map map, GenStepParams parms)
        {
            // Récupérer la geneline depuis le GameComponent global
            GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
            GenelineData geneline = globalComp?.GetGeneline(map);

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
    }
}
