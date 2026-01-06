using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Decadents
{
    public class JobDriver_EnterBeerMeditation : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            
            Toil enterAndMeditate = new Toil();
            enterAndMeditate.initAction = delegate ()
            {
                Building_BeerMeditationChamber chamber = (Building_BeerMeditationChamber)job.targetA.Thing;
                
                // Move pawn to chamber center and start meditation
                pawn.Position = chamber.Position;
                pawn.jobs.posture = PawnPosture.LayingInBed;
                
                chamber.StartMeditation(pawn);
            };
            enterAndMeditate.tickAction = delegate ()
            {
                // Keep pawn in meditation posture
                pawn.jobs.posture = PawnPosture.LayingInBed;
                
                // Freeze all needs during meditation
                if (pawn.needs != null)
                {
                    if (pawn.needs.rest != null && pawn.needs.rest.CurLevel < 0.8f)
                    {
                        pawn.needs.rest.CurLevel += 0.001f; // Restore rest
                    }
                    if (pawn.needs.food != null)
                    {
                        pawn.needs.food.CurLevel = System.Math.Max(pawn.needs.food.CurLevel, 0.5f); // Prevent starvation
                    }
                    if (pawn.needs.joy != null)
                    {
                        pawn.needs.joy.CurLevel = System.Math.Max(pawn.needs.joy.CurLevel, 0.3f); // Prevent recreation loss
                    }
                }
            };
            enterAndMeditate.defaultCompleteMode = ToilCompleteMode.Never;
            enterAndMeditate.handlingFacing = true;
            yield return enterAndMeditate;
        }
    }
}