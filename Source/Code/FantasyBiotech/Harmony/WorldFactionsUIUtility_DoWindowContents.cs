using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;


namespace FantasyBiotech
{
    [HarmonyPatch(typeof(WorldFactionsUIUtility), nameof(WorldFactionsUIUtility.DoWindowContents))]
    public class WorldFactionsUIUtility_DoWindowContents
    //dsfld class RimWorld.FactionDef RimWorld.FactionDefOf::Mechanoid
    {
	    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	    {
		    List<CodeInstruction> codes = instructions.ToList();
		    Type closureType = AccessTools.Inner(typeof(WorldFactionsUIUtility), "<>c__DisplayClass8_0");
		    FieldInfo factionsField = AccessTools.Field(closureType, "factions");
		    MethodInfo warnMethod = AccessTools.Method(typeof(WorldFactionsUIUtility_DoWindowContents), nameof(WarnIfMissing));
		    FieldInfo mechanoidField = AccessTools.Field(typeof(FactionDefOf), nameof(FactionDefOf.Mechanoid));
		    bool foundMechanoid = false;
		    bool foundInjection = false;
		    Label? insertAfterLabel = null;
		    foreach (CodeInstruction code in codes)
		    {
			    if (!foundMechanoid && code.opcode == OpCodes.Ldsfld && code.operand is FieldInfo fi && fi == mechanoidField)
			    {
				    foundMechanoid = true;
			    }
			    else if (foundMechanoid && code.opcode == OpCodes.Brtrue_S && code.operand is Label label)
			    {
				    insertAfterLabel = label;
				    break;
			    }
		    }

		    foreach (CodeInstruction code in codes)
		    {
			    yield return code;

			    if (!foundInjection && insertAfterLabel != null && code.labels.Contains(insertAfterLabel.Value))
			    {
				    yield return new CodeInstruction(OpCodes.Ldloc_0);
				    yield return new CodeInstruction(OpCodes.Ldfld, factionsField);
				    yield return new CodeInstruction(OpCodes.Ldloc_S, 21);
				    yield return new CodeInstruction(OpCodes.Call, warnMethod);
				    foundInjection = true;
			    }
		    }
		    if (!foundInjection)
		    {
			    Log.Error($"Fantasy Biotech :: Failed to find injection point in patch: {GenericUtility.GetClassName(MethodBase.GetCurrentMethod()?.DeclaringType)}");
		    }
	    }
	    public static void WarnIfMissing(List<FactionDef> factions, StringBuilder sb)
	    {
		    if (factions == null || sb == null) return;
		    if (!factions.Contains(FantasyBiotechDefOf.VR_Construct))
		    {
			    sb.AppendLine("Warning".Translate() + ": " + "MechanoidsDisabledContentWarning".Translate(FantasyBiotechDefOf.VR_Construct.label));
		    }
	    }
    }
}
