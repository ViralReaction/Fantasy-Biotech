using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
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
            Log.Message($"[GenepackCheck] Checking fuel for: {thing}");

            if (!DictionaryUtility.cachedRefuelComps.TryGetValue(thing, out CompRefuelable comp))
            {
                Log.Message($"[GenepackCheck] CompRefuelable not cached for {thing}, attempting to retrieve.");
                comp = thing.TryGetComp<CompRefuelable>();
                DictionaryUtility.cachedRefuelComps[thing] = comp;

                if (comp != null)
                    Log.Message($"[GenepackCheck] Retrieved and cached CompRefuelable: {comp}");
                else
                    Log.Message($"[GenepackCheck] No CompRefuelable found on {thing}");
            }
            else
            {
                Log.Message($"[GenepackCheck] Using cached CompRefuelable for {thing}");
            }

            if (comp == null)
            {
                Log.Message($"[GenepackCheck] No CompRefuelable, skipping override.");
                return true;
            }

            __result = comp.HasFuel;
            Log.Message($"[GenepackCheck] CompRefuelable.HasFuel = {__result} for {thing}");
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
    // [HarmonyPatch(typeof(CompGenepackContainer), "CompTick")]
    // public static class CompGenepackContainer_CompTick_Patch
    // {
    //     public static void Postfix(CompGenepackContainer __instance)
    //     {
    //         if (__instance.parent.IsHashIntervalTick(250))
    //         {
    //             __instance.innerContainer.DoTick();
    //         }
    //     }
    // }
}
