using RimWorld;
using Verse;

namespace BeerophageMod
{
    public class HediffComp_GrantAbility : HediffComp
    {
        public HediffCompProperties_GrantAbility Props
        {
            get { return (HediffCompProperties_GrantAbility)props; }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            
            if (Props.abilityDef != null && Pawn?.abilities != null)
            {
                Pawn.abilities.GainAbility(Props.abilityDef);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            
            if (Props.abilityDef != null && Pawn?.abilities != null)
            {
                Pawn.abilities.RemoveAbility(Props.abilityDef);
            }
        }

    }

    public class HediffCompProperties_GrantAbility : HediffCompProperties
    {
        public AbilityDef abilityDef;

        public HediffCompProperties_GrantAbility()
        {
            compClass = typeof(HediffComp_GrantAbility);
        }
    }
}