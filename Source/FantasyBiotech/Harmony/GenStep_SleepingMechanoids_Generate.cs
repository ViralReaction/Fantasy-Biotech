using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace FantasyBiotech
{

    [HarmonyPatch(typeof(GenStep_SleepingMechanoids), nameof(GenStep_SleepingMechanoids.Generate))]
    public static class GenStep_SleepingMechanoids_Generate
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
                    var replacement = new CodeInstruction(OpCodes.Call, newMethod)
                    {
                        labels = code[i].labels // transfer labels
                    };
                    code[i].labels = []; 
                    code[i] = replacement;
                }
            }
            return code;

        }
    }
}