using RimWorld;
using Verse;

namespace BeerophageMod
{
    public class CompAbilityEffect_PoingIvre : CompAbilityEffect
    {
        public new CompProperties_AbilityPoingIvre Props
        {
            get { return (CompProperties_AbilityPoingIvre)props; }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!base.CanApplyOn(target, dest))
                return false;

            Pawn pawn = parent.pawn;
            if (pawn == null)
                return false;

            // Check if pawn has enough beergen
            var beergenicGene = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (beergenicGene?.def?.defName != "Beergenic")
                return false;

            if (beergenicGene.Value < Props.beergenCost)
                return false;

            // Check if already active
            if (pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("PoingIvreActive")))
                return false;

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            // Always apply to the caster, not the target
            Pawn pawn = parent.pawn;
            if (pawn == null)
                return;

            // Consume beergen
            var beergenicGene = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (beergenicGene?.def?.defName == "Beergenic")
            {
                beergenicGene.Value = UnityEngine.Mathf.Max(0f, beergenicGene.Value - Props.beergenCost);
            }

            // Apply active poing ivre effect
            var activeHediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("PoingIvreActive"), pawn);
            if (Props.durationTicks > 0)
            {
                var comp = activeHediff.TryGetComp<HediffComp_Disappears>();
                if (comp != null)
                {
                    comp.ticksToDisappear = Props.durationTicks;
                }
            }
            pawn.health.AddHediff(activeHediff);

            Messages.Message("PoingIvreActivated".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override bool GizmoDisabled(out string reason)
        {
            reason = null;

            Pawn pawn = parent.pawn;
            if (pawn == null)
            {
                reason = "No pawn";
                return true;
            }

            // Check beergen
            var beergenicGene = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (beergenicGene?.def?.defName != "Beergenic")
            {
                reason = "Not a beerophage";
                return true;
            }

            if (beergenicGene.Value < Props.beergenCost)
            {
                reason = string.Format("Requires {0:F0}% beergen", Props.beergenCost * 100);
                return true;
            }

            // Check if already active
            if (pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("PoingIvreActive")))
            {
                reason = "Already active";
                return true;
            }

            return false;
        }
    }

    public class CompProperties_AbilityPoingIvre : CompProperties_AbilityEffect
    {
        public float beergenCost = 0.25f;
        public int durationTicks = 3600;

        public CompProperties_AbilityPoingIvre()
        {
            compClass = typeof(CompAbilityEffect_PoingIvre);
        }
    }
}