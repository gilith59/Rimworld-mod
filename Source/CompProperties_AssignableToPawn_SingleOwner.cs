using RimWorld;

namespace Decadents
{
    public class CompProperties_AssignableToPawn_SingleOwner : CompProperties_AssignableToPawn
    {
        public CompProperties_AssignableToPawn_SingleOwner()
        {
            this.compClass = typeof(CompAssignableToPawn_SingleOwner);
            this.maxAssignedPawnsCount = 1;
        }
    }
}
