using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace FantasyBiotech
{

    [HarmonyPatch(typeof(ComplexThreatWorker_Ambush), nameof(ComplexThreatWorker_Ambush.CanResolveInt))]
    public static class ComplexThreatWorker_Ambush_CanResolveInt
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