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
        public static HediffDef VR_ArtificerImplant;
        public static EffecterDef ButcherFlesh;
        public static SoundDef Recipe_ButcherCorpseFlesh;
        public static JobDef VR_ExtractGeneOnPawn;
        public static ThingDef VR_GeneRemover;
        public static ThingDef VR_GraftForge;
        public static ResearchProjectDef VR_ArchiteGenetics;
        public static ResearchProjectDef VR_MasterArchiteGenetics;
        public static EffecterDef VR_Steam_Burst;

        static FantasyBiotechDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(FantasyBiotechDefOf));
    }

}
