﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(MechClusterGenerator), nameof(MechClusterGenerator.GenerateClusterSketch))]
    public static class MechClusterGenerator_GenerateClusterSketch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            MethodInfo target = AccessTools.Method(typeof(MechClusterGenerator), "MechKindSuitableForCluster");
            MethodInfo replace = AccessTools.Method(typeof(MechUtility), "ConstructSuitableForCluster");
            bool foundInjection = false;

            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldftn && code[i].operand is MethodInfo method && method == target)
                {
                    code[i].operand = replace;
                    foundInjection = true;
                    break;
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
