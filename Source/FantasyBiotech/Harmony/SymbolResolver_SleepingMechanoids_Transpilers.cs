using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(SymbolResolver_SleepingMechanoids), nameof(SymbolResolver_SleepingMechanoids.CanResolve))]
    public static class SymbolResolver_SleepingMechanoids_CanResolve
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var target = AccessTools.PropertyGetter(typeof(Faction), nameof(Faction.OfMechanoids));
            var newMethod = AccessTools.Method(typeof(MechUtility), nameof(MechUtility.ConstructFaction));

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(target))
                {
                    code[i] = new CodeInstruction(OpCodes.Call, newMethod);
                }
            }

            return code;
        }
    }
    [HarmonyPatch(typeof(SymbolResolver_SleepingMechanoids), nameof(SymbolResolver_SleepingMechanoids.Resolve))]
    public static class SymbolResolver_SleepingMechanoids_Resolve
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var target = AccessTools.PropertyGetter(typeof(Faction), nameof(Faction.OfMechanoids));
            var newMethod = AccessTools.Method(typeof(MechUtility), nameof(MechUtility.ConstructFaction));

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(target))
                {
                    code[i] = new CodeInstruction(OpCodes.Call, newMethod);
                }
            }

            return code;
        }
    }
}