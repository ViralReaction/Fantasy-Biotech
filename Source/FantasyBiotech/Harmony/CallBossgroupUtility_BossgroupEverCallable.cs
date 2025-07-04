using HarmonyLib;
using RimWorld;
using Verse;


namespace FantasyBiotech
{
    [HarmonyPatch(typeof(CallBossgroupUtility), "BossgroupEverCallable")]
    public static class CallBossgroupUtility_BossgroupEverCallable
    {
        public static bool Prefix(BossgroupDef def, ref AcceptanceReport __result)
        {
            if (!def.HasModExtension<BossGroupExtension>()) return true;
            __result = false;
            return false;
        }
    }
}
