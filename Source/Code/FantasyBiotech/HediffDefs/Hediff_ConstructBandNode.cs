using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FantasyBiotech
{

    public class Hediff_ConstructBandNode : Hediff
    {
        private const int BandNodeCheckInterval = 60;

        private int cachedTunedConstructBandwidthCount;

        private HediffStage curStage;

        public int AdditionalBandwidth => cachedTunedConstructBandwidthCount;

        public override bool ShouldRemove => cachedTunedConstructBandwidthCount == 0;

        public override HediffStage CurStage
        {
            get
            {
                if (curStage != null || cachedTunedConstructBandwidthCount <= 0) return curStage;
                StatModifier statModifier = new StatModifier
                {
                    stat = StatDefOf.MechBandwidth,
                    value = cachedTunedConstructBandwidthCount
                };
                curStage = new HediffStage
                {
                    statOffsets = [statModifier]
                };
                return curStage;
            }
        }

        public override void PostTickInterval(int delta)
        {
            base.PostTickInterval(delta);
            if (pawn.IsHashIntervalTick(BandNodeCheckInterval, delta))
            {
                RecacheBandNodes();
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            RecacheBandNodes();
        }

        public void RecacheBandNodes()
        {
            int num = cachedTunedConstructBandwidthCount;
            cachedTunedConstructBandwidthCount = 0;
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building item in maps[i].listerBuildings.AllBuildingsColonistOfDef(FantasyBiotechDefOf.VR_ConstructBandNode))
                {
                    if (item.TryGetComp<CompBandNode>().tunedTo == pawn && item.TryGetComp<CompPowerTrader>().PowerOn)
                    {
                        cachedTunedConstructBandwidthCount++;
                    }
                }
            }
            if (num != cachedTunedConstructBandwidthCount)
            {
                curStage = null;
                pawn.mechanitor?.Notify_BandwidthChanged();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cachedTunedConstructBandwidthCount, "cachedTunedConstructBandwidthCount", 0);
        }
    }

}