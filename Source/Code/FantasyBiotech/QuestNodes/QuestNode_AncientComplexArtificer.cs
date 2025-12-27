using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FantasyBiotech
{
    public class QuestNode_Root_Loot_AncientComplexArtificer : QuestNode_Root_Loot_AncientComplex_Mechanitor
    {
        public override LayoutDef LayoutDef => FantasyBiotechDefOf.VR_AncientComplex_Artificer_Loot;

        public override SitePartDef SitePartDef => SitePartDefOf.AncientComplex_Mechanitor;

    }
}
