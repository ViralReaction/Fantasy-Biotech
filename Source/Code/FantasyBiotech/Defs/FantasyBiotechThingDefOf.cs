using JetBrains.Annotations;
using RimWorld;
using Verse;
namespace FantasyBiotech

{
    [DefOf]
    public static class FantasyBiotechThingDefOf
    {
        [UsedImplicitly]
        public static ThingDef VR_GeneRack;
        public static ThingDef VR_ArtificerImplant;
        public static ThingDef VR_GeneRemover;
        public static ThingDef VR_GraftForge;
        public static ThingDef VR_ConstructBandNode;
        public static ThingDef VR_Construct_Lift;
        public static ThingDef VR_ArtificerSarcophagus;
        public static ThingDef VR_AncientBarrelBomb;

        [MayRequireIdeology]
        public static ThingDef VR_AncientEnemyTerminal;

        static FantasyBiotechThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(FantasyBiotechThingDefOf));
    }
}
