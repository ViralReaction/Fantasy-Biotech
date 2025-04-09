using HarmonyLib;
using RimWorld;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(CompGenepackContainer), nameof(CompGenepackContainer.PostDeSpawn))]
    public static class CompGenepackContainer_PostDeSpawn
    {
        public static void Postfix(CompGenepackContainer __instance)
        {
            DictionaryUtility.cachedRefuelComps.Remove(__instance.parent);
        }
    }
}
