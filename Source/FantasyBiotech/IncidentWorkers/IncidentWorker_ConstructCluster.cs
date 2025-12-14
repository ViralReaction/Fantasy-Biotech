using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class IncidentWorker_ConstructCluster : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            if (!ModsConfig.RoyaltyActive)
            {
                return false;
            }
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            return MechUtility.ConstructFaction() != null;
        }

        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            MechClusterSketch sketch = ConstructClusterGenerator.GenerateClusterSketch(parms.points, map);
            IntVec3 center = MechClusterUtility.FindClusterPosition(map, sketch, 100, 0.5f);
            if (!center.IsValid)
            {
                return false;
            }

            IEnumerable<Thing> targets = from t in ConstructClusterGenerator.SpawnCluster(center, map, sketch, dropInPods: false, canAssaultColony: true, parms.questTag)
                                         where t.def != ThingDefOf.Wall && t.def != ThingDefOf.Barricade
                                         select t;
            SendStandardLetter(parms, new LookTargets(targets));
            return true;
        }

    }
}
