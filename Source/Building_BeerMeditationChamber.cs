using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;


namespace BeerophageMod
{
    public class Building_BeerMeditationChamber : Building
    {
        private CompRefuelable compRefuelable;
        private CompPowerTrader compPowerTrader;
        private int meditationTicks = 0;
        private const int MEDITATION_DURATION = 3600; // DEV TEST: 1 hour instead of 10 days (840000)
        private bool meditationActive = false;
        private Pawn meditatingPawn = null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compRefuelable = GetComp<CompRefuelable>();
            compPowerTrader = GetComp<CompPowerTrader>();
        }

        protected override void Tick()
        {
            base.Tick();
            
            if (this.IsHashIntervalTick(60)) // Check every second
            {
                ProcessMeditation();
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn))
            {
                yield return option;
            }

            if (selPawn.Drafted)
            {
                yield break;
            }

            // Check if pawn can meditate
            if (!CanPawnMeditate(selPawn, out string reason))
            {
                yield return new FloatMenuOption("CannotEnterMeditation".Translate() + " (" + reason + ")", null);
                yield break;
            }

            if (meditationActive && meditatingPawn != selPawn)
            {
                yield return new FloatMenuOption("CannotEnterMeditation".Translate() + " (occupied)", null);
                yield break;
            }

            if (meditationActive && meditatingPawn == selPawn)
            {
                yield return new FloatMenuOption("ExitMeditation".Translate(), () => {
                    Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("ExitBeerMeditation"), this);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
            else
            {
                yield return new FloatMenuOption("EnterMeditation".Translate(), () => {
                    Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EnterBeerMeditation"), this);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
        }

        private bool CanPawnMeditate(Pawn pawn, out string reason)
        {
            reason = "";

            // Check if beerophage
            if (!HasBeergenicGene(pawn))
            {
                reason = "not a beerophage";
                return false;
            }

            // Check if powered
            if (!compPowerTrader?.PowerOn == true)
            {
                reason = "no power";
                return false;
            }

            // Check if has fuel (for starting)
            if (!meditationActive && !compRefuelable?.HasFuel == true)
            {
                reason = "no beer fuel";
                return false;
            }

            // Check if already in meditation elsewhere
            var existingMeditation = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail("BeerMeditation"));
            if (existingMeditation != null && meditatingPawn != pawn)
            {
                reason = "already meditating";
                return false;
            }

            return true;
        }

        public void StartMeditation(Pawn pawn)
        {
            meditationActive = true;
            meditatingPawn = pawn;
            meditationTicks = 0;
            
            // Add meditation hediff that puts pawn in deep meditation state
            var hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("BeerMeditation"), pawn);
            pawn.health.AddHediff(hediff);
            
            Messages.Message("BeerMeditationStarted".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.PositiveEvent);
        }
        
        public bool IsPawnInMeditation(Pawn pawn)
        {
            return meditationActive && meditatingPawn == pawn;
        }

        public void ExitMeditation(bool forced = false)
        {
            if (!meditationActive || meditatingPawn == null) return;

            var pawn = meditatingPawn;
            
            meditationActive = false;
            meditatingPawn = null;
            
            // Remove meditation hediff
            var meditationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail("BeerMeditation"));
            if (meditationHediff != null)
            {
                pawn.health.RemoveHediff(meditationHediff);
            }
            
            if (forced)
            {
                // Apply interruption malus
                var interruptionHediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("InterruptedBeerMeditation"), pawn);
                pawn.health.AddHediff(interruptionHediff);
                
                Messages.Message("BeerMeditationInterrupted".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.NegativeEvent);
            }
            
            meditationTicks = 0;
        }

        private void ProcessMeditation()
        {
            if (!meditationActive || meditatingPawn == null) return;

            // Check if meditating pawn is still alive and on map
            if (meditatingPawn.Dead || meditatingPawn.Map != this.Map)
            {
                ExitMeditation(true);
                return;
            }

            // Check if powered - interrupt if power lost
            if (!compPowerTrader?.PowerOn == true)
            {
                ExitMeditation(true);
                return;
            }

            // Check if we still have fuel - pause if empty but don't interrupt
            if (!compRefuelable?.HasFuel == true)
            {
                // Meditation paused - waiting for refuel
                return;
            }
            
            // Continue meditation
            meditationTicks += 60; // Add 1 second worth of ticks
            
            // Consume beer fuel
            if (Rand.Chance(0.05f)) // 5% chance per second
            {
                compRefuelable.ConsumeFuel(1f);
            }

            // Check if meditation is complete
            if (meditationTicks >= MEDITATION_DURATION)
            {
                CompleteMeditation();
            }
        }

        private void CompleteMeditation()
        {
            if (meditatingPawn == null) return;

            var pawn = meditatingPawn;
            
            // Remove meditation hediff
            var meditationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail("BeerMeditation"));
            if (meditationHediff != null)
            {
                pawn.health.RemoveHediff(meditationHediff);
            }
            
            // Grant poing ivre mastery (30-day ability)
            var masteryHediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("PoingIvreMastery"), pawn);
            pawn.health.AddHediff(masteryHediff);
            
            Messages.Message("BeerMeditationCompleted".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.PositiveEvent);
            
            // End the pawn's current job to eject them from meditation
            if (pawn.jobs?.curJob != null)
            {
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
            
            // Reset state
            meditationActive = false;
            meditatingPawn = null;
            meditationTicks = 0;
        }

        private bool HasBeergenicGene(Pawn pawn)
        {
            var beergenicGene = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            return beergenicGene?.def?.defName == "Beergenic";
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

            if (meditationActive && meditatingPawn != null)
            {
                float progress = (float)meditationTicks / MEDITATION_DURATION;
                if (!string.IsNullOrEmpty(baseString))
                    baseString += "\n";
                baseString += meditatingPawn.LabelShort + " meditating: " + (progress * 100f).ToString("F1") + "%";
                
                int daysRemaining = Mathf.CeilToInt((MEDITATION_DURATION - meditationTicks) / 60000f);
                baseString += " (" + daysRemaining + " days remaining)";
                
                // Show pause status if out of fuel
                if (!compRefuelable?.HasFuel == true)
                {
                    baseString += "\n(PAUSED - No beer fuel)";
                }
            }

            return baseString;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref meditationTicks, "meditationTicks", 0);
            Scribe_Values.Look(ref meditationActive, "meditationActive", false);
            Scribe_References.Look(ref meditatingPawn, "meditatingPawn");
        }
    }
}