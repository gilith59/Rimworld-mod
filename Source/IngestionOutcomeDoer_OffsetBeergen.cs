using RimWorld;
using Verse;
using System.Collections.Generic;

namespace BeerophageMod
{
    public class IngestionOutcomeDoer_OffsetBeergen : IngestionOutcomeDoer
    {
        public float offset;
        public ChemicalDef toleranceChemical;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            // Find beergenic gene
            var beergenicGene = pawn.genes?.GetGene(DefDatabase<GeneDef>.GetNamed("Beergenic", false));
            if (beergenicGene is Gene_Hemogen hemogenGene)
            {
                // Calculate total offset based on ingested count
                float totalOffset = offset * ingestedCount;
                
                // Apply tolerance factor if chemical tolerance is specified
                if (toleranceChemical != null)
                {
                    Hediff tolerance = pawn.health.hediffSet.GetFirstHediffOfDef(toleranceChemical.toleranceHediff);
                    if (tolerance != null)
                    {
                        // Simple tolerance reduction - higher tolerance = less effect
                        totalOffset *= (1f - (tolerance.Severity * 0.5f));
                    }
                }

                // Restore beergen
                hemogenGene.Value = UnityEngine.Mathf.Min(1f, hemogenGene.Value + totalOffset);
                
                // Show effect message
                Messages.Message("BeergenRestoredFrom".Translate(ingested.def.label, (totalOffset * 100f).ToString("F0")), 
                    pawn, MessageTypeDefOf.PositiveEvent, false);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (parentDef.IsIngestible)
            {
                yield return new StatDrawEntry(
                    StatCategoryDefOf.BasicsNonPawn,
                    "Beergen restoration",
                    (offset * 100f).ToString("F0") + "%",
                    "Restores beergen when consumed by beergenic pawns",
                    1100);
            }
        }
    }
}