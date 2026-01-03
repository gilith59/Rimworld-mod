using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BeerophageMod
{
    public class Building_BeerCasket : Building_Bed
    {
        private CompRefuelable compRefuelable;
        private CompPowerTrader compPowerTrader;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compRefuelable = GetComp<CompRefuelable>();
            compPowerTrader = GetComp<CompPowerTrader>();
        }

        protected override void Tick()
        {
            base.Tick();
            
            // Only process every 60 ticks (1 second)
            if (this.IsHashIntervalTick(60))
            {
                ProcessBeergenRestoration();
            }
        }

        private void ProcessBeergenRestoration()
        {
            // Check if powered and has fuel
            if (compPowerTrader?.PowerOn != true || compRefuelable?.HasFuel != true)
                return;

            // Check if someone is sleeping in the casket
            var sleeper = GetCurOccupant(0);
            if (sleeper == null || !sleeper.jobs.curDriver.asleep)
                return;

            // Check if sleeper has beergenic gene
            var beergenicGene = sleeper.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (beergenicGene?.def?.defName != "Beergenic")
                return;
            // Restore small amount of beergen while sleeping and consuming beer
            float restoreAmount = 0.001f; // Small amount per second
            beergenicGene.Value = Mathf.Min(1f, beergenicGene.Value + restoreAmount);
            
            // Consume beer fuel slowly
            if (Rand.Chance(0.1f)) // 10% chance per second to consume fuel
            {
                compRefuelable.ConsumeFuel(0.1f);
            }
        }

        public override string GetInspectString()
        {
            string baseString = base.GetInspectString();
            
            if (compRefuelable != null)
            {
                if (!string.IsNullOrEmpty(baseString))
                    baseString += "\n";
                baseString += "Beer: " + compRefuelable.Fuel.ToString("F1") + " / " + compRefuelable.Props.fuelCapacity.ToString("F1");
            }

            var sleeper = GetCurOccupant(0);
            if (sleeper != null)
            {
                var beergenicGene = sleeper.genes?.GetFirstGeneOfType<Gene_Hemogen>();
                if (beergenicGene?.def?.defName == "Beergenic")
                {
                    if (!string.IsNullOrEmpty(baseString))
                        baseString += "\n";
                    baseString += "Beergen: " + (beergenicGene.Value * 100f).ToString("F0") + "%";
                }
            }

            return baseString;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            // Add refuel gizmo if needed
            if (compRefuelable != null)
            {
                foreach (Gizmo gizmo in compRefuelable.CompGetGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }
    }
}