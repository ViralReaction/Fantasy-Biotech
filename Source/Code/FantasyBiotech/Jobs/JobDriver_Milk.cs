using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class JobDriver_Milk : JobDriver_GatherAnimalBodyResourcesByConstruct
    {
        public override float WorkTotal => 400f;

        public override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompMilkable>();
        }
    }
}
