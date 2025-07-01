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

        static FantasyBiotechDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(FantasyBiotechDefOf));
    }

}
