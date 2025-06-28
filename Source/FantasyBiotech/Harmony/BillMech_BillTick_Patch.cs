using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Bill_Mech), "BillTick")]
    public static class BillMech_BillTick_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            MethodInfo getGestator = AccessTools.PropertyGetter(typeof(Bill_Mech), "Gestator");
            MethodInfo originalCall = AccessTools.Method(typeof(Building_WorkTableAutonomous), "Notify_FormingCompleted");
            MethodInfo newMethod = AccessTools.Method(typeof(BillMech_BillTick_Patch), nameof(CustomNotify));

            for (int i = 0; i < code.Count - 2; i++)
            {
                if (code[i].opcode == OpCodes.Ldarg_0 &&
                    code[i + 1].Calls(getGestator) &&
                    code[i + 2].Calls(originalCall))
                {
                    code[i + 2] = new CodeInstruction(OpCodes.Call, newMethod);
                    break;
                }
            }

            return code;
        }

        public static void CustomNotify(Building_MechGestator Gestator)
        {
            if (Gestator is Building_ConstructGestator gestator)
            {
                gestator.Notify_FormingCompleted();
                return;
            }
            Gestator.Notify_FormingCompleted();
        }
    }

}