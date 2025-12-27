using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(QuestNode_SpawnMechCluster), nameof(QuestNode_SpawnMechCluster.TestRunInt))]
    public static class QuestNode_SpawnMechCluster_TestRunInt
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
                    CodeInstruction replacement = new CodeInstruction(OpCodes.Call, newMethod)
                    {
                        labels = code[i].labels // transfer labels
                    };
                    code[i].labels = [];
                    code[i] = replacement;
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