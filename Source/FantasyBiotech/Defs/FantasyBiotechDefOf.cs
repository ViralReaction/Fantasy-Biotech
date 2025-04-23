using Verse;
using RimWorld;
using PipeSystem;

namespace FantasyBiotech
{
    [DefOf]
    public static class FantasyBiotechDefOf
    {
        public static FactionDef VR_Construct;

        static FantasyBiotechDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(FantasyBiotechDefOf));
    }

}
