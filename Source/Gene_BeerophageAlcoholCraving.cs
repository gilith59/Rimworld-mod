using RimWorld;
using Verse;

namespace BeerophageMod
{
    public class Gene_BeerophageAlcoholCraving : Gene
    {
        private int ticksSinceLastCraving = 0;
        private const int CRAVING_INTERVAL_MIN = 480000; // 8 days minimum
        private const int CRAVING_INTERVAL_MAX = 960000; // 16 days maximum

        public override void PostAdd()
        {
            base.PostAdd();
            // Start with a random interval
            ticksSinceLastCraving = Rand.Range(0, CRAVING_INTERVAL_MIN);
        }

        public override void Tick()
        {
            base.Tick();
            
            ticksSinceLastCraving++;
            
            // Check for craving every day
            if (ticksSinceLastCraving % 60000 == 0) // Every day
            {
                CheckForAlcoholCraving();
            }
        }

        private void CheckForAlcoholCraving()
        {
            // Don't add craving if already has one
            if (pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("BeerophageAlcoholCraving")))
                return;

            // Don't add craving if currently has alcohol effects
            if (pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("BeerophageAlcoholEffect")))
                return;

            // Check if enough time has passed since last craving
            int randomInterval = Rand.Range(CRAVING_INTERVAL_MIN, CRAVING_INTERVAL_MAX);
            if (ticksSinceLastCraving >= randomInterval)
            {
                // Trigger alcohol craving
                var craving = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("BeerophageAlcoholCraving"), pawn);
                pawn.health.AddHediff(craving);
                
                // Reset timer
                ticksSinceLastCraving = 0;
                
                Messages.Message(pawn.LabelShort + " is craving alcohol.", pawn, MessageTypeDefOf.NeutralEvent);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksSinceLastCraving, "ticksSinceLastCraving", 0);
        }
    }
}