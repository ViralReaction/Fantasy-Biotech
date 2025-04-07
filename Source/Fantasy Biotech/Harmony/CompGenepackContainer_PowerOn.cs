using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(CompGenepackContainer), nameof(CompGenepackContainer.PowerOn), MethodType.Getter)]
    public static class CompGenepackContainer_PowerOn
    {
        public static bool Prefix(CompGenepackContainer __instance, ref bool __result)
        {
            var thing = __instance.parent;
            if (!cachedRefuelComps.TryGetValue(thing, out var comp))
            {
                comp = thing.TryGetComp<CompRefuelable>();
                cachedRefuelComps[thing] = comp;
            }
            if (comp != null)
            {
                __result = comp.HasFuel;
                return false;
            }
            return true;
        }
        private static readonly Dictionary<Thing, CompRefuelable> cachedRefuelComps = [];
    }

}