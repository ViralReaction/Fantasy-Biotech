using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Alert_GenebankUnpowered))]
    [HarmonyPatch(MethodType.Constructor)]
    public static class Alert_GenebankUnpowered_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldstr && code[i].operand is string str)
                {
                    if (str == "AlertGenebankUnpowered")
                        code[i].operand = "Fantasy_AlertGenebankUnfueled";
                    else if (str == "AlertGenebankUnpoweredDesc")
                        code[i].operand = "Fantasy_AlertGenebankUnfueledDesc";
                }

            }
            foreach (var c in code) yield return c;
        }
    }
}