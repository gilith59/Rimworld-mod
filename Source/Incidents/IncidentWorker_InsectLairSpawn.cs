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

            // Empêcher incidents multiples - vérifier s'il y a déjà un InsectLairEntrance actif
            if (map.listerThings.ThingsOfDef(InsectLairDefOf.InsectLairEntrance).Any() ||
                map.listerThings.ThingsOfDef(InsectLairDefOf.InsectLairSpawner).Any())
            {
                Log.Warning("[InsectLairIncident] Cannot spawn - InsectLair already active on map");
                return false;
            }

            // Chercher emplacement 6x6
            if (!TryFindSpawnCell(map, out IntVec3 cell))
                return false;

            // Spawner l'InsectLairSpawner (va progressivement créer l'InsectLairEntrance)
            Thing spawner = ThingMaker.MakeThing(InsectLairDefOf.InsectLairSpawner);
            GenSpawn.Spawn(spawner, cell, map);

            // Enregistrer les threat points pour la vague (sera déclenchée quand le portal apparaît)
            MapComponent_InsectLairWaveSpawner waveSpawner = map.GetComponent<MapComponent_InsectLairWaveSpawner>();
            if (waveSpawner == null)
            {
                waveSpawner = new MapComponent_InsectLairWaveSpawner(map);
                map.components.Add(waveSpawner);
            }

            // Ajouter monitor pour vérifier la mort du boss depuis la surface
            MapComponent_InsectLairMonitor monitor = map.GetComponent<MapComponent_InsectLairMonitor>();
            if (monitor == null)
            {
                monitor = new MapComponent_InsectLairMonitor(map);
                map.components.Add(monitor);
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
