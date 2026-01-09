using RimWorld;
using Verse;
using Verse.Sound;
using System.Linq;

namespace InsectLairIncident
{
    // Track si HiveQueen est morte pour permettre collapse
    public class MapComponent_HiveQueenTracker : MapComponent
    {
        private Pawn queen;
        private bool queenDead = false;
        private int ticksUntilAutoCollapse = -1;
        private int autoCollapseDelay = 180000; // 72 heures par défaut, configuré dans settings
        private bool discoveryMessageShown = false;
        private Map parentMap; // La map de la colonie (surface)

        public MapComponent_HiveQueenTracker(Map map) : base(map)
        {
        }

        public void RegisterQueen(Pawn pawn, Map colonyMap = null)
        {
            queen = pawn;
            parentMap = colonyMap;

            // Lire le délai d'auto-collapse depuis les settings
            InsectLairSettings settings = InsectLairMod.GetSettings();
            autoCollapseDelay = settings.autoCollapseDelayTicks;

            // Ne pas afficher le message ici - attendre que le joueur la voie
        }

        private string GetBossDiscoveryMessage(string bossDefName)
        {
            switch (bossDefName)
            {
                case "VFEI2_Empress":
                    return "You've discovered the Empress! This massive insectoid queen commands her swarm with terrifying intelligence. Defeat her to collapse the lair.";
                case "VFEI2_Titantick":
                    return "You've discovered the Titantick! This explosive behemoth is heavily armored and extremely dangerous. Defeat it to collapse the lair.";
                case "VFEI2_Teramantis":
                    return "You've discovered the Teramantis! This colossal mantis-like creature is a apex predator. Defeat it to collapse the lair.";
                case "VFEI2_Gigamite":
                    return "You've discovered the Gigamite! This enormous mite can spit devastating acid. Defeat it to collapse the lair.";
                case "VFEI2_Silverfish":
                    return "You've discovered the Silverfish! This armored insectoid is nearly impenetrable. Defeat it to collapse the lair.";
                case "HiveQueen":
                    return "You've discovered the Hive Queen! This ancient insectoid monarch rules the depths. Defeat her to collapse the lair.";
                default:
                    return $"You've discovered the boss: {bossDefName}! Defeat it to collapse the lair.";
            }
        }

        public bool IsQueenDead()
        {
            if (queen == null)
                return true;

            if (queenDead)
                return true;

            if (queen.Dead || queen.Destroyed)
            {
                queenDead = true;
                ticksUntilAutoCollapse = autoCollapseDelay;

                // Letter visible partout avec son
                string bossName = queen.kindDef.LabelCap;
                float collapseDays = autoCollapseDelay / 60000f; // 60000 ticks = 1 jour
                Find.LetterStack.ReceiveLetter(
                    "Boss Defeated!",
                    $"The {bossName} has been defeated! The insect lair will automatically collapse in {collapseDays:F1} days.",
                    LetterDefOf.PositiveEvent,
                    new LookTargets(queen)
                );

                SoundDefOf.Quest_Concluded.PlayOneShotOnCamera(null);

                return true;
            }

            return false;
        }

        // Aliases publics pour l'Alert et autres composants
        public bool IsBossDead() => IsQueenDead();

        public int GetTicksUntilCollapse() => ticksUntilAutoCollapse;

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // Vérifier mort du boss (important même si on n'est pas sur cette map)
            IsQueenDead();

            // Vérifier si la queen est visible par un colonist
            if (!discoveryMessageShown && queen != null && !queen.Dead && !queen.Destroyed)
            {
                // Vérifier si au moins un colonist peut la voir
                foreach (Pawn colonist in map.mapPawns.FreeColonistsSpawned)
                {
                    if (GenSight.LineOfSight(colonist.Position, queen.Position, map, true))
                    {
                        discoveryMessageShown = true;
                        string bossMessage = GetBossDiscoveryMessage(queen.kindDef.defName);

                        Find.LetterStack.ReceiveLetter(
                            "Boss Discovered!",
                            bossMessage,
                            LetterDefOf.ThreatBig,
                            new LookTargets(queen)
                        );

                        SoundDefOf.Quest_Accepted.PlayOneShotOnCamera(null);
                        break;
                    }
                }
            }

            // Timer auto-collapse
            if (queenDead && ticksUntilAutoCollapse > 0)
            {
                ticksUntilAutoCollapse--;

                if (ticksUntilAutoCollapse <= 0)
                {
                    // Trouver et détruire l'InsectLairEntrance sur la map parent
                    TriggerAutoCollapse();
                }
            }
        }

        private void TriggerAutoCollapse()
        {
            if (parentMap == null || !parentMap.IsPlayerHome)
            {
                Log.Warning("[InsectLairIncident] Cannot auto-collapse: parent map not found");
                return;
            }

            // Trouver le portal InsectLairEntrance sur la map parent
            MapPortal portal = parentMap.listerThings.AllThings
                .OfType<MapPortal>()
                .FirstOrDefault(p => p.def.defName == "InsectLairEntrance");

            if (portal != null)
            {
                Find.LetterStack.ReceiveLetter(
                    "Lair Collapsed",
                    "The insect lair has collapsed! The entrance has sealed itself.",
                    LetterDefOf.NeutralEvent,
                    new LookTargets(portal)
                );

                SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(portal.Position, parentMap));

                // Utiliser Seal() au lieu de Destroy() car InsectLairEntrance est non-destroyable
                CompSealable comp = portal.GetComp<CompSealable>();
                if (comp != null)
                {
                    comp.Seal();
                }
                else
                {
                    Log.Warning("[InsectLairIncident] CompSealable not found on portal");
                }
            }
            else
            {
                Log.Warning("[InsectLairIncident] Could not find InsectLairEntrance to collapse");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref queen, "queen");
            Scribe_References.Look(ref parentMap, "parentMap");
            Scribe_Values.Look(ref queenDead, "queenDead");
            Scribe_Values.Look(ref ticksUntilAutoCollapse, "ticksUntilAutoCollapse", -1);
            Scribe_Values.Look(ref autoCollapseDelay, "autoCollapseDelay", 180000);
            Scribe_Values.Look(ref discoveryMessageShown, "discoveryMessageShown", false);
        }
    }
}
