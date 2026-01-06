using RimWorld;
using Verse;

namespace Decadents
{
    public class Gene_BeergenEmpowerment : Gene
    {
        private Hediff cachedEmpowermentHediff;

        public override void PostAdd()
        {
            base.PostAdd();
            CheckAndApplyEmpowerment();
        }

        public override void Tick()
        {
            base.Tick();
            
            // Check every 60 ticks (1 second)
            if (pawn.IsHashIntervalTick(60))
            {
                CheckAndApplyEmpowerment();
            }
        }

        private void CheckAndApplyEmpowerment()
        {
            if (pawn?.genes == null || pawn?.health?.hediffSet == null) return;

            // Find the beergenic gene
            var beergenicGene = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
            if (beergenicGene?.def?.defName != "Beergenic") return;

            float beergenLevel = beergenicGene.Value;
            bool shouldHaveBonus = beergenLevel >= 0.7f; // 70% threshold

            var empowermentDef = DefDatabase<HediffDef>.GetNamedSilentFail("BeergenEmpowerment");
            if (empowermentDef == null) return;

            var currentBonus = pawn.health.hediffSet.GetFirstHediffOfDef(empowermentDef);
            bool hasBonus = currentBonus != null;

            if (shouldHaveBonus && !hasBonus)
            {
                // Add empowerment hediff
                var hediff = HediffMaker.MakeHediff(empowermentDef, pawn);
                pawn.health.AddHediff(hediff);
                cachedEmpowermentHediff = hediff;
            }
            else if (!shouldHaveBonus && hasBonus)
            {
                // Remove empowerment hediff
                pawn.health.RemoveHediff(currentBonus);
                cachedEmpowermentHediff = null;
            }
        }

        public override void PostRemove()
        {
            // Clean up empowerment hediff when gene is removed
            if (cachedEmpowermentHediff != null && pawn?.health?.hediffSet != null)
            {
                pawn.health.RemoveHediff(cachedEmpowermentHediff);
            }
            base.PostRemove();
        }
    }
}