using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Decadents
{
    public class IngestionOutcomeDoer_BeerophageAlcoholFilter : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            // Check if pawn is a beerophage
            if (!IsBeerophage(pawn))
                return;

            // Replace vanilla alcohol effects with beerophage version
            var alcoholHigh = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);
            if (alcoholHigh != null)
            {
                float severity = alcoholHigh.Severity;
                
                // Remove the vanilla hediff
                pawn.health.RemoveHediff(alcoholHigh);
                
                // Add positive-only alcohol effect with same severity
                var beerophageAlcoholEffect = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("BeerophageAlcoholEffect"), pawn);
                beerophageAlcoholEffect.Severity = severity;
                pawn.health.AddHediff(beerophageAlcoholEffect);
            }

            // Remove alcohol addiction for beerophages
            var alcoholAddiction = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail("AlcoholAddiction"));
            if (alcoholAddiction != null)
            {
                pawn.health.RemoveHediff(alcoholAddiction);
            }

            // Remove alcohol tolerance buildup
            var alcoholTolerance = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail("AlcoholTolerance"));
            if (alcoholTolerance != null)
            {
                pawn.health.RemoveHediff(alcoholTolerance);
            }
            
            // Remove alcohol craving when drinking
            var alcoholCraving = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail("BeerophageAlcoholCraving"));
            if (alcoholCraving != null)
            {
                pawn.health.RemoveHediff(alcoholCraving);
            }
        }

        private bool IsBeerophage(Pawn pawn)
        {
            var beergenicGene = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            return beergenicGene?.def?.defName == "Beergenic";
        }
    }
}