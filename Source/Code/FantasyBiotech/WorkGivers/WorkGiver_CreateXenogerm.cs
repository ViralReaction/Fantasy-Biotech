using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class WorkGiver_CreateXenogerm : RimWorld.WorkGiver_CreateXenogerm
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(FantasyBiotechThingDefOf.VR_GraftForge);

    }
}
