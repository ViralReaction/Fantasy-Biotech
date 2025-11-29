using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech;

public class WorldTechLevel_Patch
{
    private static Type WorldTechLevel_EffectiveTechLevels => AccessTools.TypeByName("WorldTechLevel.DefTechLevels");

    [HarmonyPatch]
    public static class WorldTechLevel_EffectiveTechLevels_Patch
    {
        public static bool Prepare()
        {
            return WorldTechLevel_EffectiveTechLevels != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("WorldTechLevel.DefTechLevels:ResearchProjectDefFirstPass", [typeof(ResearchProjectDef)]);
        }

        public static void Postfix(ref TechLevel __result, ResearchProjectDef def)
        {
            if (def.GetModExtension<ResearchTechLevelOverride>()?.canOverride == true)
            {
                #if Debug
                Log.Message(def.label + " : " + "Overriding World Tech Level settings. Tech level set to " + def.techLevel + ".");
                #endif
                __result = def.techLevel;
            }
        }
    }
}