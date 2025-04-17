using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(CompGenepackContainer), nameof(CompGenepackContainer.PowerOn), MethodType.Getter)]
    public static class CompGenepackContainer_PowerOn
    {
        public static bool Prefix(CompGenepackContainer __instance, ref bool __result)
        {
            Thing thing = __instance.parent;
            if (!DictionaryUtility.cachedRefuelComps.TryGetValue(thing, out var comp))
            {
                comp = thing.TryGetComp<CompRefuelable>();
                DictionaryUtility.cachedRefuelComps[thing] = comp;
            }
            if (comp != null)
            {
                __result = comp.HasFuel;
                return false;
            }
            return true;
        }

    }

}