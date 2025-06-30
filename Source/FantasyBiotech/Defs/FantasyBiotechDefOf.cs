using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [DefOf]
    public static class FantasyBiotechDefOf
    {
        [UsedImplicitly] public static FactionDef VR_Construct;
        public static ThingDef VR_GeneRack;

        public static ResearchProjectDef VR_BasicConstructTech;
        public static ResearchProjectDef VR_StandardConstructTech;
        public static ResearchProjectDef VR_HighConstructTech;
        public static ResearchProjectDef VR_UltraConstructTech;


        static FantasyBiotechDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(FantasyBiotechDefOf));
    }

}
