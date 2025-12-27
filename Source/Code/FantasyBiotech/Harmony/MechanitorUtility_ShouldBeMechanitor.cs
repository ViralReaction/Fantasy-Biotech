using HarmonyLib;
using RimWorld;
using Verse;
namespace FantasyBiotech.FantasyBiotech.Harmony
{
    [HarmonyPatch(typeof(MechanitorUtility), "ShouldBeMechanitor")]
    public static class MechanitorUtility_ShouldBeMechanitor
    {
        public static bool Postfix(bool __result, Pawn pawn)
        {
            if (!__result)
            {
                __result = pawn.Faction.IsPlayerSafe() && pawn.health.hediffSet.HasHediff(FantasyBiotechDefOf.VR_ArtificerImplant);
            }
            return __result;

        }
    }
}
