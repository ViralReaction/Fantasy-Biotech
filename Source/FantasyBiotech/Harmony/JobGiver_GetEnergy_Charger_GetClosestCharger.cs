using HarmonyLib;
using RimWorld;
using UnityEngine.Networking;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(JobGiver_GetEnergy_Charger), "GetClosestCharger")]
    public static class JobGiver_GetEnergy_Charger_GetClosestCharger
    {
        public static bool Prefix(ref Building_MechCharger __result, Pawn mech, Pawn carrier, bool forced)
        {

            __result = MechUtility.GetClosestCharger(carrier, mech, forced);
            return false;
        }

    }
}
