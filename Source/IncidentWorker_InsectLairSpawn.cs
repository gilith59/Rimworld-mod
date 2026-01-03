using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace InsectLairIncident
{
    // Spawne le vanilla InsectLairEntrance comme incident
    public class IncidentWorker_InsectLairSpawn : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            // Chercher emplacement 6x6
            if (!TryFindSpawnCell(map, out IntVec3 cell))
                return false;

            // Spawner l'InsectLairSpawner (va progressivement créer l'InsectLairEntrance)
            ThingDef spawnerDef = ThingDef.Named("InsectLairSpawner");
            Thing spawner = ThingMaker.MakeThing(spawnerDef);
            GenSpawn.Spawn(spawner, cell, map);

            // Enregistrer les threat points pour la vague (sera déclenchée quand le portal apparaît)
            MapComponent_InsectLairWaveSpawner waveSpawner = map.GetComponent<MapComponent_InsectLairWaveSpawner>();
            if (waveSpawner == null)
            {
                waveSpawner = new MapComponent_InsectLairWaveSpawner(map);
                map.components.Add(waveSpawner);
            }

            float points = parms.points;
            if (points <= 0f)
            {
                points = StorytellerUtility.DefaultThreatPointsNow(map);
            }

            // Enregistrer les points mais pas encore le portal (il n'existe pas encore)
            waveSpawner.RegisterThreatPoints(points, cell);

            // Lettre
            Find.LetterStack.ReceiveLetter(
                "Insect Lair Emergence",
                "The ground is cracking and shifting near your colony. Something is emerging from the depths! Move buildings and items away from the emergence point.",
                LetterDefOf.ThreatBig,
                new TargetInfo(cell, map)
            );

            return true;
        }

        private bool TryFindSpawnCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomCellNear(
                map.Center,
                map,
                Mathf.RoundToInt(map.Size.x * 0.3f),
                c => CanPlaceAt(c, map),
                out cell
            );
        }

        private bool CanPlaceAt(IntVec3 center, Map map)
        {
            CellRect rect = CellRect.CenteredOn(center, 6, 6).ExpandedBy(2);

            foreach (IntVec3 c in rect)
            {
                // Éviter fog, roofs, et terrains invalides (comme PitGate)
                if (!c.InBounds(map) || c.Fogged(map) || c.Roofed(map))
                    return false;

                TerrainDef terrain = c.GetTerrain(map);
                if (!terrain.affordances.Contains(TerrainAffordanceDefOf.Heavy))
                    return false;
            }

            return true;
        }
    }
}
