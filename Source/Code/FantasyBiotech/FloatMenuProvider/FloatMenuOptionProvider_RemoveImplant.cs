using RimWorld;
using Verse;
using Verse.AI;

namespace FantasyBiotech
{
    public class FloatMenuOptionProvider_RemoveImplant : FloatMenuOptionProvider
    {
        public override bool Drafted => true;

        public override bool Undrafted => true;

        public override bool Multiselect => false;

        public override bool RequiresManipulation => true;

        public override bool AppliesInt(FloatMenuContext context)
        {
            return ModsConfig.BiotechActive;
        }

        public override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            Corpse corpse = clickedThing as Corpse;
            if (corpse == null)
            {
                return null;
            }
            if (!corpse.InnerPawn.health.hediffSet.HasHediff(FantasyBiotechDefOf.VR_ArtificerImplant))
            {
                return null;
            }
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Extract".Translate() + " " + FantasyBiotechDefOf.VR_ArtificerImplant.label, delegate
            {
                Job job = JobMaker.MakeJob(FantasyBiotechDefOf.VR_RemoveImplant, corpse);
                context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }), context.FirstSelectedPawn, new LocalTargetInfo(corpse));
        }
    }
}
