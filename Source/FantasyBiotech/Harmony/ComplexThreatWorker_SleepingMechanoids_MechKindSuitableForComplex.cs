using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(ComplexThreatWorker_SleepingMechanoids), nameof(ComplexThreatWorker_SleepingMechanoids.MechKindSuitableForComplex))]
    public static class ComplexThreatWorker_SleepingMechanoids_MechKindSuitableForComplex
    {
        public static void Postfix(PawnKindDef def, ref bool __result)
        {
            if (__result)
            {
                __result = def.GetModExtension<ConstructExtension>()?.isConstruct ?? false;
            }
        }

    }
}