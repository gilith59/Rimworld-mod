using RimWorld;
using Verse;

namespace Decadents
{
    public class CompBeergenRestoration : ThingComp
    {
        public CompProperties_BeergenRestoration Props
        {
            get { return (CompProperties_BeergenRestoration)props; }
        }

        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(250)) // Check every ~4 seconds
            {
                ProcessBeergenRestoration();
            }
        }

        private void ProcessBeergenRestoration()
        {
            if (!(parent is Building_Bed bed))
                return;

            var sleeper = bed.GetCurOccupant(0);
            if (sleeper == null || !sleeper.jobs.curDriver.asleep)
                return;

            // Check if sleeper has beergenic gene
            var beergenicGene = sleeper.genes?.GetGene(DefDatabase<GeneDef>.GetNamed("Beergenic", false));
            if (beergenicGene is Gene_Hemogen hemogenGene)
            {
                // Restore beergen slowly while sleeping
                float restoreAmount = Props.beergenPerTick;
                hemogenGene.Value = UnityEngine.Mathf.Min(1f, hemogenGene.Value + restoreAmount);
            }
        }
    }

    public class CompProperties_BeergenRestoration : CompProperties
    {
        public float beergenPerTick = 0.0001f; // Small restoration per tick

        public CompProperties_BeergenRestoration()
        {
            compClass = typeof(CompBeergenRestoration);
        }
    }
}