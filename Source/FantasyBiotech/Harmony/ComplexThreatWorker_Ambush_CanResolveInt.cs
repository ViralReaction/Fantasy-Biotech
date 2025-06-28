using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;
using Verse;

namespace FantasyBiotech
{

    [HarmonyPatch(typeof(ComplexThreatWorker_Ambush), nameof(ComplexThreatWorker_Ambush.CanResolveInt))]
    public static class ComplexThreatWorker_Ambush_CanResolveInt
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            MethodInfo target = AccessTools.PropertyGetter(typeof(Faction), nameof(Faction.OfMechanoids));
            MethodInfo newMethod = AccessTools.Method(typeof(MechUtility), nameof(MechUtility.ConstructFaction));
            bool foundInjection = false;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(target))
                {
                    foundInjection = true;
                    code[i] = new CodeInstruction(OpCodes.Call, newMethod);
                }
            }
            if (!foundInjection)
            {
                Log.Error($"Fantasy Biotech :: Failed to find injection point in patch: {GenericUtility.GetClassName(MethodBase.GetCurrentMethod()?.DeclaringType)}");
            }
            return code;

        }
    }
}