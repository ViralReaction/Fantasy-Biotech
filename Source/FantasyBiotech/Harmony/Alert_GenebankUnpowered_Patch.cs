using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Alert_GenebankUnpowered))]
    [HarmonyPatch(MethodType.Constructor)]
    public static class Alert_GenebankUnpowered_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            bool foundInjection = false;
            bool secondInjection = false;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode != OpCodes.Ldstr || code[i].operand is not string str) continue;
                switch (str)
                {
                    case "AlertGenebankUnpowered":
                        code[i].operand = "Fantasy_AlertGenebankUnfueled";
                        foundInjection = true;
                        break;
                    case "AlertGenebankUnpoweredDesc":
                        code[i].operand = "Fantasy_AlertGenebankUnfueledDesc";
                        secondInjection = true;
                        break;
                }
            }
            if (!foundInjection)
            {
                Log.Error($"Fantasy Biotech :: Failed to find first injection point in patch: {GenericUtility.GetClassName(MethodBase.GetCurrentMethod()?.DeclaringType)}");
            }
            if (!secondInjection)
            {
                Log.Error($"Fantasy Biotech :: Failed to find first injection point in patch: {GenericUtility.GetClassName(MethodBase.GetCurrentMethod()?.DeclaringType)}");
            }
            foreach (CodeInstruction c in code) yield return c;
        }
    }
}