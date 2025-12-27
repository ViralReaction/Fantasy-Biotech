using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FantasyBiotech
{
    public class QuestPart_ConstructCluster : QuestPart
    {
        public MechClusterSketch sketch;

        public string inSignal;

        public string tag;

        public MapParent mapParent;

        public IntVec3 dropSpot = IntVec3.Invalid;

        private IntVec3 spawnedClusterPos = IntVec3.Invalid;

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
                {
                    yield return questLookTarget;
                }
                if (spawnedClusterPos.IsValid && mapParent is { HasMap: true })
                {
                    yield return new GlobalTargetInfo(spawnedClusterPos, mapParent.Map);
                }
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (mapParent is not { HasMap: true } || !quest.IsParentSuitableForQuest(mapParent))
            {
                mapParent = quest.TryFindNewSuitableMapParentForRetarget();
            }
            if (signal.tag != inSignal || mapParent is not { HasMap: true }) return;
            spawnedClusterPos = dropSpot;
            if (spawnedClusterPos == IntVec3.Invalid)
            {
                spawnedClusterPos = MechClusterUtility.FindClusterPosition(mapParent.Map, sketch, 100, 0.5f);
            }
            if (spawnedClusterPos == IntVec3.Invalid) return;
            ConstructClusterGenerator.SpawnCluster(spawnedClusterPos, mapParent.Map, sketch, dropInPods: true, canAssaultColony: false, tag);
            Find.LetterStack.ReceiveLetter("LetterLabelConstructClusterArrived".Translate(), "LetterMechConstructArrived".Translate(), LetterDefOf.ThreatBig, new TargetInfo(spawnedClusterPos, mapParent.Map), null, quest);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref tag, "tag");
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Deep.Look(ref sketch, "sketch");
            Scribe_Values.Look(ref dropSpot, "dropSpot");
            Scribe_Values.Look(ref spawnedClusterPos, "spawnedClusterPos");
        }
    }
}
