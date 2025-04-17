using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(CompGenepackContainer), nameof(CompGenepackContainer.PostDestroy))]
    public static class CompGenepackContainer_PostDestroy
    {
        public static bool Prefix(CompGenepackContainer __instance)
        {
            DictionaryUtility.cachedRefuelComps.Remove(__instance.parent);
            return true;
        }
    }
}
