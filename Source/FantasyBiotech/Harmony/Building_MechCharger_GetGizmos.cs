using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Building_MechCharger), nameof(Building_MechCharger.GetGizmos))]
    public static class Building_MechCharger_GetGizmos
    {
        public static bool Prefix(Building_MechCharger __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance is not Building_MechCharger_Steam) return true;
            __result = SteamChargerGizmo(__instance);
            return false;
        }

        private static IEnumerable<Gizmo> SteamChargerGizmo(Building_MechCharger __instance)
        {
            foreach (Gizmo gizmo in __instance.GetGizmos())
            {
                yield return gizmo;
            }
        }
    }
}