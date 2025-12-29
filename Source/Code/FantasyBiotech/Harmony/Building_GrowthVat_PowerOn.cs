using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Building_GrowthVat), "PowerOn", MethodType.Getter)]
    public static class Building_GrowthVat_PowerOn
    {
        public static bool Prepare()
        {
            return MenuController.settings.medievalGrowthVat;
        }
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }

    }
}
