using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Linq;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(SitePartWorker_SleepingMechanoids), nameof(SitePartWorker_SleepingMechanoids.GetMechanoidsCount))]
    public static class SitePartWorker_SleepingMechanoids_GetMechanoidsCount
    {
        public static bool Prepare()
        {
            return MenuController.settings.replaceMechanoids;
        }
        public static bool Prefix(ref int __result, CompGenepackContainer __instance, Site site, SitePartParams parms)
        {
            __result = PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
            {
                tile = site.Tile,
                faction = MechUtility.ConstructFaction(),
                groupKind = PawnGroupKindDefOf.Combat,
                points = parms.threatPoints,
                seed = SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms)
            }).Count();
            return false;
        }
        
    }

   
}
