using RimWorld;
using Verse;
using Verse.AI;

namespace FantasyBiotech
{
    public class WorkGiver_ExtractGeneOnPawn : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(FantasyBiotechDefOf.VR_GeneRemover);
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Building_GeneRemover geneRemover)
            {
                return false;
            }
            if (JobDriver_OperateOnPawn.CanWork(pawn, geneRemover.ContainedPawn, geneRemover) is false)
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_GeneRemover geneRemover = t as Building_GeneRemover;
            return JobMaker.MakeJob(FantasyBiotechDefOf.VR_ExtractGeneOnPawn, t, geneRemover.ContainedPawn);
        }
    }
}
