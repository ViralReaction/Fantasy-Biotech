using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(SymbolResolver_Interior_SleepingMechanoids), nameof(SymbolResolver_Interior_SleepingMechanoids.Resolve))]
    public static class SymbolResolver_Interior_SleepingMechanoids_Resolve
    {
        public static bool Prepare()
        {
            return MenuController.settings.replaceMechanoids;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            MethodInfo target = AccessTools.PropertyGetter(typeof(Faction), nameof(Faction.OfMechanoids));
            MethodInfo newMethod = AccessTools.Method(typeof(MechUtility), nameof(MechUtility.ConstructFaction));
            bool foundInjection = false;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(target))
                {
                    code[i].opcode = OpCodes.Call;
                    code[i].operand = newMethod;
                    foundInjection = true;
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
