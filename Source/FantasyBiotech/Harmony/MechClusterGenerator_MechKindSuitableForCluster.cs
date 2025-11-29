using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(MechClusterGenerator), "MechKindSuitableForCluster")]
    public static class MechClusterGenerator_MechKindSuitableForCluster
    {
        public static void Postfix(ref bool __result, PawnKindDef def)
        {
            if (__result)
            {
                if (!MenuController.settings.replaceMechanoids)
                {
                    __result = !def.GetModExtension<ConstructExtension>()?.isConstruct == true;
                }
                else
                {
                    __result = def.GetModExtension<ConstructExtension>()?.isConstruct == false;
                }
            }
        }
    }
}