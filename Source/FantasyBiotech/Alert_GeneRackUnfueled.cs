using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FantasyBiotech
{
    public class Alert_GeneRackUnfueled : Alert
    {
        private List<GlobalTargetInfo> targets = [];

        public Alert_GeneRackUnfueled()
        {
            defaultLabel = "Fantasy_AlertGenebankUnfueled".Translate();
            defaultExplanation = "Fantasy_AlertGenebankUnfueledDesc".Translate();
            requireBiotech = true;
        }

        public override AlertReport GetReport()
        {
            GetTargets();
            return AlertReport.CulpritsAre(targets);
        }

        private void GetTargets()
        {
            targets.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building building in maps[i].listerBuildings.AllBuildingsColonistOfDef(FantasyBiotechDefOf.VR_GeneRack))
                {
                    CompGenepackContainer compGenepackContainer = building.TryGetComp<CompGenepackContainer>();
                    CompRefuelable refuelable = building.TryGetComp<CompRefuelable>();
                    if (refuelable == null || refuelable.HasFuel)
                    {
                        continue;
                    }
                    if (compGenepackContainer != null && compGenepackContainer.innerContainer.Any())
                    {
                        targets.Add(building);
                    }
                }
            }
        }
    }
}
