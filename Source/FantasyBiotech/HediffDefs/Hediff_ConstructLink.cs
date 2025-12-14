using RimWorld;
using Verse;

namespace FantasyBiotech
{

    public class Hediff_Constructlink : HediffWithComps
    {
        private const int LearningOpportunityCheckInterval = 300;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (!ModLister.CheckBiotech("Mechlink"))
            {
                pawn.health.RemoveHediff(this);
                return;
            }
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
            if (pawn.Spawned)
            {
                Find.LetterStack.ReceiveLetter("LetterLabelConstructlinkInstalled".Translate() + ": " + pawn.LabelShortCap, "LetterConstructlinkInstalled".Translate(pawn.Named("PAWN")), LetterDefOf.PositiveEvent, pawn);
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            pawn.mechanitor?.Notify_MechlinkRemoved();
        }

        public override void PostTickInterval(int delta)
        {
            base.PostTickInterval(delta);
            if (pawn.Spawned && pawn.IsHashIntervalTick(300, delta))
            {
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.Mechanitors, OpportunityType.Important);
            }
        }
    }
}
