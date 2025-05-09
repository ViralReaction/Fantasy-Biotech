using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(MechClusterGenerator), "MechKindSuitableForCluster")]
    public static class MechClusterGenerator_MechKindSuitableForCluster
    {
        public static bool Prefix(ref bool __result, PawnKindDef def)
        {
            if (!def.RaceProps.IsMechanoid || def.isGoodBreacher ||
                !def.isFighter || !def.allowInMechClusters ||
                ModsConfig.BiotechActive && Find.BossgroupManager.ReservedByBossgroup(def) ||
                def.GetModExtension<ConstructExtension>()?.isConstruct == false)
            {
                __result = false;
                return false;
            }

            __result = true;
            return false;
        }
    }
}