using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace InsectLairIncident
{
    // Gère le spawn des vagues d'insectoïdes qui sortent du portal
    public class MapComponent_InsectLairWaveSpawner : MapComponent
    {
        private InsectLairEntrance portalToSpawnFrom;
        private float threatPoints;
        private IntVec3 expectedPortalPosition;
        private int ticksUntilNextWave = 60; // 1 seconde de délai pour la première
        private bool firstWaveSpawned = false;
        private bool waitingForPortal = false;

        // Wave interval configuré dans les settings (défaut: 60000 = 1 jour)
        private int waveIntervalTicks = 60000;

        // Portal search optimization
        private int portalSearchTicks = 0;
        private const int PORTAL_SEARCH_INTERVAL = 60; // Check every 1 second instead of every tick

        private static readonly List<PawnKindDef> insectoidKinds = new List<PawnKindDef>();

        // Geneline choisie pour cette cave (VFE Insectoids support)
        private GenelineData chosenGeneline;

        public MapComponent_InsectLairWaveSpawner(Map map) : base(map)
        {
        }

        // Appelé par l'IncidentWorker pour enregistrer les threat points
        public void RegisterThreatPoints(float points, IntVec3 position)
        {
            // Lire les settings
            InsectLairSettings settings = InsectLairMod.GetSettings();

            this.threatPoints = points * settings.threatPointsMultiplier;
            this.expectedPortalPosition = position;
            this.waitingForPortal = true;
            this.firstWaveSpawned = false;
            this.waveIntervalTicks = settings.waveIntervalTicks;

            // Choisir une geneline aléatoire (VFE ou vanilla)
            this.chosenGeneline = GenelineHelper.ChooseRandomGeneline();

            // NOTE: On enregistrera la geneline quand le portal sera détecté (pour avoir son thingIDNumber)
            // Log.Message($"[InsectLairIncident] Geneline choisie: {chosenGeneline.defName} (Boss: {chosenGeneline.boss.defName})");
        }

        // Retourne la geneline choisie (pour l'IncidentWorker)
        public GenelineData GetChosenGeneline()
        {
            return chosenGeneline;
        }

        // Appelé quand le portal est détecté
        private void OnPortalDetected(InsectLairEntrance portal)
        {
            // Enregistrer globalement avec le portal ID
            GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
            if (globalComp != null)
            {
                globalComp.RegisterGeneline(portal.thingIDNumber, chosenGeneline);
            }

            this.portalToSpawnFrom = portal;
            this.waitingForPortal = false;
            this.ticksUntilNextWave = 60; // 1 seconde avant la première vague
            // Log.Message("[InsectLairIncident] Portal detected, wave spawning will begin in 1 second");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // Si on attend le portal, chercher s'il est apparu (throttled search)
            if (waitingForPortal)
            {
                portalSearchTicks++;
                if (portalSearchTicks >= PORTAL_SEARCH_INTERVAL)
                {
                    portalSearchTicks = 0;
                    SearchForPortal();
                }
                return; // Ne pas spawner de vagues tant que le portal n'est pas là
            }

            // Si le portal est détruit ou n'existe pas, arrêter
            if (portalToSpawnFrom == null || portalToSpawnFrom.Destroyed)
                return;

            // Vérifier si le boss est mort (cheap check)
            if (portalToSpawnFrom.PocketMap != null)
            {
                MapComponent_HiveQueenTracker tracker = portalToSpawnFrom.PocketMap.GetComponent<MapComponent_HiveQueenTracker>();
                if (tracker != null && tracker.IsBossDead())
                {
                    return; // Boss mort = plus de vagues!
                }
            }

            ticksUntilNextWave--;

            if (ticksUntilNextWave <= 0)
            {
                SpawnInsectoidWave();

                // Après la première vague, programmer les vagues récurrentes
                if (!firstWaveSpawned)
                {
                    firstWaveSpawned = true;
                    ticksUntilNextWave = waveIntervalTicks;
                }
                else
                {
                    ticksUntilNextWave = waveIntervalTicks;
                }
            }
        }

        private void SearchForPortal()
        {
            // Use cached DefOf reference for optimal performance
            foreach (Thing thing in map.listerThings.ThingsOfDef(InsectLairDefOf.InsectLairEntrance))
            {
                if (thing is InsectLairEntrance portal && thing.Position.InHorDistOf(expectedPortalPosition, 10f))
                {
                    Log.Message($"[InsectLairIncident] Portal detected at {portal.Position}, wave spawning enabled");
                    OnPortalDetected(portal);
                    break;
                }
            }
        }

        private void SpawnInsectoidWave()
        {
            IntVec3 portalPos = portalToSpawnFrom.Position;

            // Utiliser les insectes de la geneline choisie
            List<PawnKindDef> waveInsects = chosenGeneline?.insects;
            if (waveInsects == null || waveInsects.Count == 0)
            {
                // Fallback vanilla
                waveInsects = new List<PawnKindDef>
                {
                    PawnKindDefOf.Megascarab,
                    PawnKindDefOf.Spelopede,
                    PawnKindDefOf.Megaspider
                };
            }

            float pointsRemaining = threatPoints * 0.4f; // 40% des threat points
            // Log.Message($"[InsectLairIncident] Spawning {chosenGeneline.defName} wave with {pointsRemaining} points");

            List<Pawn> spawnedInsects = new List<Pawn>();

            // Spawner des insectoïdes jusqu'à épuiser les points
            while (pointsRemaining > 0f && spawnedInsects.Count < 30)
            {
                // Choisir un insecte de la geneline aléatoirement
                PawnKindDef kind = waveInsects.RandomElementByWeight(k =>
                    1f / Mathf.Max(1f, k.combatPower)
                );

                // Créer le pawn
                Pawn insect = PawnGenerator.GeneratePawn(kind, Faction.OfInsects);

                // Trouver une position près du portal (sortie de la cave)
                IntVec3 spawnCell;
                if (CellFinder.TryFindRandomCellNear(portalPos, map, 8,
                    c => c.Standable(map) && !c.Fogged(map) && c.GetFirstPawn(map) == null,
                    out spawnCell))
                {
                    GenSpawn.Spawn(insect, spawnCell, map);
                    spawnedInsects.Add(insect);
                    pointsRemaining -= kind.combatPower;

                    // Effet visuel de spawn
                    FleckMaker.Static(spawnCell, map, FleckDefOf.PsycastAreaEffect, 1f);
                }
                else
                {
                    insect.Destroy();
                    break;
                }
            }

            // Mettre tous les insectes en mode agressif
            foreach (Pawn insect in spawnedInsects)
            {
                insect.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            }

            // Log.Message($"[InsectLairIncident] Spawned {spawnedInsects.Count} insects from cave entrance");

            // Message au joueur
            Messages.Message(
                $"A wave of {spawnedInsects.Count} insects emerges from the cave entrance!",
                new TargetInfo(portalPos, map),
                MessageTypeDefOf.ThreatBig
            );
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref portalToSpawnFrom, "portalToSpawnFrom");
            Scribe_Values.Look(ref threatPoints, "threatPoints", 0f);
            Scribe_Values.Look(ref expectedPortalPosition, "expectedPortalPosition");
            Scribe_Values.Look(ref ticksUntilNextWave, "ticksUntilNextWave", 60);
            Scribe_Values.Look(ref firstWaveSpawned, "firstWaveSpawned", false);
            Scribe_Values.Look(ref waitingForPortal, "waitingForPortal", false);
            Scribe_Values.Look(ref waveIntervalTicks, "waveIntervalTicks", 60000);
            Scribe_Values.Look(ref portalSearchTicks, "portalSearchTicks", 0);
            Scribe_Deep.Look(ref chosenGeneline, "chosenGeneline");
        }
    }
}
