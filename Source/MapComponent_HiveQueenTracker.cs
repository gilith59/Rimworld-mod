using RimWorld;
using Verse;

namespace InsectLairIncident
{
    // Track si HiveQueen est morte pour permettre collapse
    public class MapComponent_HiveQueenTracker : MapComponent
    {
        private Pawn queen;
        private bool queenDead = false;

        public MapComponent_HiveQueenTracker(Map map) : base(map)
        {
        }

        public void RegisterQueen(Pawn pawn)
        {
            queen = pawn;
        }

        public bool IsQueenDead()
        {
            if (queen == null)
                return true; // Pas de queen = peut collapse

            if (queenDead)
                return true;

            if (queen.Dead || queen.Destroyed)
            {
                queenDead = true;
                Messages.Message(
                    "The Hive Queen has been defeated! The insect lair can now be collapsed.",
                    MessageTypeDefOf.PositiveEvent
                );
                return true;
            }

            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref queen, "queen");
            Scribe_Values.Look(ref queenDead, "queenDead");
        }
    }
}
