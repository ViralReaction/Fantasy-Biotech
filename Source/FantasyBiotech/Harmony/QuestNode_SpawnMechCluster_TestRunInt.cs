using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(QuestNode_SpawnMechCluster), nameof(QuestNode_SpawnMechCluster.TestRunInt))]
    public static class QuestNode_SpawnMechCluster_TestRunInt
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