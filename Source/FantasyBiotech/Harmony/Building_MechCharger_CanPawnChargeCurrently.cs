using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Building_MechCharger), "CanPawnChargeCurrently")]
    public static class Building_MechCharger_CanPawnChargeCurrently
    {
        public static bool Prefix(ref bool __result, Building_MechCharger __instance, Pawn pawn)
        {
            if (__instance is Building_MechCharger_Steam steamCharger)
            {
                __result = steamCharger.CanPawnChargeCurrentlySteam(pawn);
                return false;

            }
            return true;
        }
    }
}