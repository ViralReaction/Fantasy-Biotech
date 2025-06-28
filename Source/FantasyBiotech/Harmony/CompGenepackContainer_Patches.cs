using HarmonyLib;
using RimWorld;
using Verse;

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
    [HarmonyPatch(typeof(CompGenepackContainer), nameof(CompGenepackContainer.PowerOn), MethodType.Getter)]
    public static class CompGenepackContainer_PowerOn
    {
        public static bool Prefix(CompGenepackContainer __instance, ref bool __result)
        {
            Thing thing = __instance.parent;
            if (!DictionaryUtility.cachedRefuelComps.TryGetValue(thing, out CompRefuelable comp))
            {
                comp = thing.TryGetComp<CompRefuelable>();
                DictionaryUtility.cachedRefuelComps[thing] = comp;
            }
            if (comp == null) return true;
            __result = comp.HasFuel;
            return false;
        }

    }
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
