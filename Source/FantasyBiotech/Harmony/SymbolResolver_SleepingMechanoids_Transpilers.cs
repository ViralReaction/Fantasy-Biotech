using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(SymbolResolver_SleepingMechanoids), nameof(SymbolResolver_SleepingMechanoids.CanResolve))]
    public static class SymbolResolver_SleepingMechanoids_CanResolve
    {
        public static bool Prepare()
        {
            return MenuController.settings.replaceMechanoids;
        }
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

    [HarmonyPatch(typeof(SymbolResolver_SleepingMechanoids), nameof(SymbolResolver_SleepingMechanoids.Resolve))]
    public static class SymbolResolver_SleepingMechanoids_Resolve
    {
        public static bool Prepare()
        {
            return MenuController.settings.replaceMechanoids;
        }
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