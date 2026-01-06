using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Decadents
{
    public class JobDriver_ExitBeerMeditation : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil exitMeditation = new Toil();
            exitMeditation.initAction = delegate ()
            {
                Building_BeerMeditationChamber chamber = (Building_BeerMeditationChamber)job.targetA.Thing;
                chamber.ExitMeditation(false); // Voluntary exit, no malus
            };
            exitMeditation.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return exitMeditation;
        }
    }
}