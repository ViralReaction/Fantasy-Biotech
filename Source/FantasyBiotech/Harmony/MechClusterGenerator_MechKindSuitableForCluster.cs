using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(MechClusterGenerator), "MechKindSuitableForCluster")]
    public static class MechClusterGenerator_MechKindSuitableForCluster
    {
        public static bool Prepare()
        {
            return MenuController.settings.replaceMechanoids;
        }
        public static bool Prefix(ref bool __result, PawnKindDef def)
        {
            if (!def.RaceProps.IsMechanoid || def.isGoodBreacher || !def.isFighter || !def.allowInMechClusters)
            {

                __result = false;
                return false;
            }
            if (ModsConfig.BiotechActive && Find.BossgroupManager.ReservedByBossgroup(def))
            {
                __result = false;
                return false;
            }

            if (def.GetModExtension<ConstructExtension>()?.isConstruct ?? false)
            {
                __result = true;
            }
            return false;
        }
    }
}