using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Building_GeneAssembler), nameof(Building_GeneAssembler.PowerOn), MethodType.Getter)]
    public static class Building_GeneAssembler_PowerOn
    {
        /// <summary>
        /// Forcibly returns true at the beginning of the method,
        /// effectively short-circuiting all original logic.
        /// The original method instructions are still emitted (for reference),
        /// but are unreachable due to the early return.
        /// </summary>
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
            yield return new CodeInstruction(OpCodes.Ret);
            foreach (var c in code) yield return c;
        }
    }
}