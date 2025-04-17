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
            if (__instance is Building_MechCharger_Steam)
            {
                __result = SteamChargerGizmo(__instance);
                return false;
            }
            return true;
        }

        public static IEnumerable<Gizmo> SteamChargerGizmo(Building_MechCharger __instance)
        {
            foreach (Gizmo gizmo in __instance.GetGizmos())
            {
                yield return gizmo;
            }
        }
    }
}