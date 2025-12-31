using RimWorld;
using Verse;
namespace FantasyBiotech
{
    public class JobDriver_Shear : JobDriver_GatherAnimalBodyResourcesByConstruct
    {
        public override float WorkTotal => 1700f;

        public override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompShearable>();
        }
    }
}
