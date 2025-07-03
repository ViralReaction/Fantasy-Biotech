using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class CompUseEffect_CallBossgroup_Steam : CompUseEffect_CallBossgroup
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Find.FactionManager.FirstFactionOfDef(FantasyBiotechDefOf.VR_Construct).deactivated)
            {
                return "VR_ConstructsDisabled".Translate();
            }
            if (!MechanitorUtility.IsMechanitor(p))
            {
                return "RequiresMechanitor".Translate();
            }
            return Props.bossgroupDef.Worker.CanResolve(p);
        }

    }
}
