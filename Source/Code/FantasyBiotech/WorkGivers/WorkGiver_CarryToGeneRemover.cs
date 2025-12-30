using RimWorld;
using Verse;
namespace FantasyBiotech
{
    public class WorkGiver_CarryToGeneRemover : WorkGiver_CarryToBuilding
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(FantasyBiotechThingDefOf.VR_GeneRemover);

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !ModsConfig.BiotechActive;
        }
    }

}
