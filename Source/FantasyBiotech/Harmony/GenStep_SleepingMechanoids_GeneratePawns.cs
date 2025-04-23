using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(GenStep_SleepingMechanoids), nameof(GenStep_SleepingMechanoids.GeneratePawns))]
    public static class GenStep_SleepingMechanoids_GeneratePawns
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
