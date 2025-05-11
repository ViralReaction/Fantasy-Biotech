using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech;

public class WorldTechLevel_Patch
{
    private static Type WorldTechLevel_EffectiveTechLevels => AccessTools.TypeByName("WorldTechLevel.EffectiveTechLevels");

    [HarmonyPatch]
    public static class WorldTechLevel_EffectiveTechLevels_Patch
    {
        public static bool Prepare()
        {
            return WorldTechLevel_EffectiveTechLevels != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("WorldTechLevel.EffectiveTechLevels:ResearchProjectDefFirstPass", [typeof(ResearchProjectDef)
            ]);
        }

        public static bool Prefix(ref TechLevel __result, ResearchProjectDef def)
        {
            __result = def.techLevel;
            return false;
        }
    }
}