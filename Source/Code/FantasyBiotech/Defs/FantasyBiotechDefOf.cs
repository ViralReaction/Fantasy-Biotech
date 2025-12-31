using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [DefOf]
    public static class FantasyBiotechDefOf
    {
        [UsedImplicitly] public static FactionDef VR_Construct;
        public static HediffDef VR_ArtificerImplant;
        public static EffecterDef ButcherFlesh;
        public static SoundDef Recipe_ButcherCorpseFlesh;
        public static JobDef VR_ExtractGeneOnPawn;
        public static JobDef VR_RemoveImplant;
        public static ResearchProjectDef VR_ArchiteGenetics;
        public static ResearchProjectDef VR_MasterArchiteGenetics;
        public static EffecterDef VR_Steam_Burst;
        public static EffecterDef VR_Vat_Bubbles_Right;
        public static EffecterDef VR_Vat_Bubbles_Left;
        public static SketchResolverDef VR_ConstructCluster;
        public static LayoutDef VR_AncientComplex_Artificer_Loot;
        public static PawnKindDef VR_Artificer_Basic;
        public static JobDef VR_MilkConstruct;
        public static JobDef VR_ShearConstruct;

        static FantasyBiotechDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(FantasyBiotechDefOf));
    }

}
