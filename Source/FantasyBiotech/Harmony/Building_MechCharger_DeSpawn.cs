using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Building_MechCharger), nameof(Building_MechCharger.DeSpawn))]
    public static class Building_MechCharger_DeSpawn
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();

            MethodInfo getWasteProducer = AccessTools.PropertyGetter(typeof(Building_MechCharger), nameof(Building_MechCharger.WasteProducer));
            MethodInfo newMethod = AccessTools.Method(typeof(Building_MechCharger_DeSpawn), nameof(WasteProducerCheck));

            for (int i = 0; i < code.Count - 4; i++)
            {
                if (code[i].opcode == OpCodes.Ldarg_0 && code[i+1].Calls(getWasteProducer))
                {
                    code.RemoveRange(i + 1, 5);
                    code.Insert(i + 1, new CodeInstruction(OpCodes.Call, newMethod));
                    break;
                }
            }

            return code;
        }

        public static void WasteProducerCheck(Building_MechCharger charger)
        {
            if (charger is Building_MechCharger_Steam) return;
            charger.WasteProducer.ProduceWaste(Mathf.CeilToInt(charger.wasteProduced));
        }
    }

}