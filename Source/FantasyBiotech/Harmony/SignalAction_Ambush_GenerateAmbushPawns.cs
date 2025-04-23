using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(SignalAction_Ambush), "GenerateAmbushPawns")]
    public static class SignalAction_Ambush_GenerateAmbushPawns
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