using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;
using Verse;

namespace FantasyBiotech
{

    [HarmonyPatch(typeof(GenStep_SleepingMechanoids), nameof(GenStep_SleepingMechanoids.Generate))]
    public static class GenStep_SleepingMechanoids_Generate
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
                    CodeInstruction replacement = new CodeInstruction(OpCodes.Call, newMethod)
                    {
                        labels = code[i].labels // transfer labels
                    };
                    code[i].labels = []; 
                    code[i] = replacement;
                    foundInjection = true;
                }
            }
            if (!foundInjection)
            {
                Log.Error($"Fantasy Biotech :: Failed to find injection point in patch: {GenericUtility.GetClassName(MethodBase.GetCurrentMethod()?.DeclaringType)}");
            }
            return code;

        }
    }
    [HarmonyPatch(typeof(GenStep_SleepingMechanoids), nameof(GenStep_SleepingMechanoids.GeneratePawns))]
    public static class GenStep_SleepingMechanoids_GeneratePawns
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
                    code[i] = new CodeInstruction(OpCodes.Call, newMethod);
                    foundInjection = true;
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