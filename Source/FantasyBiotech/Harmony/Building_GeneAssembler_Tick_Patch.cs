﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Building_GeneAssembler), "Tick")]
    public static class Building_GeneAssembler_Tick_Patch
    {
        /// <summary>
        /// Removes the conditional check for Gen.IsHashIntervalTick(this, 250)
        /// from the target method and replaces it with an unconditional jump.
        /// This causes the method to always skip the associated block of code,
        /// as if the original condition had returned false.
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            MethodInfo targetMethod = AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) });
            bool foundInjection = false;
            for (int i = 0; i < code.Count - 3; i++)
            {
                if (
                    code[i].opcode == OpCodes.Ldarg_0 &&
                    code[i + 1].opcode == OpCodes.Ldc_I4 && Convert.ToInt32(code[i + 1].operand) == 250 &&
                    code[i + 2].Calls(targetMethod) &&
                    code[i + 3].opcode == OpCodes.Brfalse_S
                )
                {
                    Label skipLabel = (Label)code[i + 3].operand;
                    code.RemoveRange(i, 4);
                    code.Insert(i, new CodeInstruction(OpCodes.Br_S, skipLabel));
                    foundInjection = true;
                    break;
                }
            }
            if (!foundInjection)
            {
                Log.Error($"Fantasy Biotech :: Failed to find injection point in patch: {GenericUtility.GetClassName(MethodBase.GetCurrentMethod()?.DeclaringType)}");
            }
            foreach (CodeInstruction c in code) yield return c;
        }
    }
}