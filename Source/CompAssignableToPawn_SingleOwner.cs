using RimWorld;
using System.Linq;
using Verse;

namespace Decadents
{
    // Custom component that enforces single owner assignment
    public class CompAssignableToPawn_SingleOwner : CompAssignableToPawn
    {
        public override void TryAssignPawn(Pawn pawn)
        {
            // Unassign all existing owners before assigning new one
            if (this.AssignedPawnsForReading.Count > 0)
            {
                foreach (Pawn existingOwner in this.AssignedPawnsForReading.ToList())
                {
                    this.TryUnassignPawn(existingOwner, false);
                }
            }
            base.TryAssignPawn(pawn);
        }
    }
}
