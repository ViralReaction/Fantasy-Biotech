using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class StorytellerComp_SingleOnceFixed : StorytellerComp
    {
        protected int IntervalsPassed => Find.TickManager.TicksGame / 1000;

        private StorytellerCompProperties_SingleOnceFixed Props => (StorytellerCompProperties_SingleOnceFixed)props;

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            int num = IntervalsPassed;
            if (Props.minColonistCount > 0)
            {
                if (target.StoryState.lastFireTicks.ContainsKey(Props.incident) || PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count < Props.minColonistCount)
                {
                    yield break;
                }
                num -= target.StoryState.GetTicksFromColonistCount(Props.minColonistCount) / 1000;
            }
            if (num != Props.fireAfterDaysPassed * 60)
            {
                yield break;
            }
            if (Props.skipIfAnyColonistIsArtificer)
            {
                List<Pawn> allMapsCaravansAndTravellingTransporters_Alive_FreeColonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
                for (int i = 0; i < allMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count; i++)
                {
                    if (MechUtility.IsArtificer(allMapsCaravansAndTravellingTransporters_Alive_FreeColonists[i]))
                    {
                        yield break;
                    }
                }
            }
            Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
            if (!Props.skipIfOnExtremeBiome || (anyPlayerHomeMap != null && !anyPlayerHomeMap.Biome.isExtremeBiome))
            {
                IncidentDef incident = Props.incident;
                if (incident.TargetAllowed(target))
                {
                    yield return new FiringIncident(incident, this, GenerateParms(incident.category, target));
                }
            }
        }
    }

}