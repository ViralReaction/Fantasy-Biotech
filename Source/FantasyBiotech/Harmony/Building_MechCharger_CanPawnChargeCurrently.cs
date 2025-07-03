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
            ConstructExtension modExtension = pawn.def.GetModExtension<ConstructExtension>();
            if (modExtension?.isConstruct != true || __instance is not Building_MechCharger_Steam steamCharger) return true;
            __result = steamCharger.CanPawnChargeCurrentlySteam(pawn);
            return false;
        }
    }
}