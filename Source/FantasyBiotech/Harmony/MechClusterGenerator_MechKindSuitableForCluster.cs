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
            Log.Message("TEsting MechKindSuitableForCluster");

            if (def.GetModExtension<ConstructExtension>()?.isConstruct ?? false)
            {
                Log.Message($"Def: {def.defName} isConstruct: true" );
                __result = true;
            }
            return false;
            // if (!__result) return;
            //
            //
            // bool isConstruct = def.GetModExtension<ConstructExtension>()?.isConstruct ?? false;
            // Log.Message($"Def: {def.defName} isConstruct: {isConstruct}" );
            // if (MenuController.settings.replaceMechanoids)
            // {
            //     __result = isConstruct;
            // }
            // else
            // {
            //     __result = !isConstruct;
            // }
        }
    }
}