using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(PassingShip), nameof(PassingShip.Depart))]
    public class PassingShip_Depart
    {
        public static bool Prefix(PassingShip __instance)
        {
            if (!MenuController.settings.replaceTradeShipIncident) return true;
            if (__instance.Map.listerBuildings.ColonistsHaveBuilding(( b) => b.def.IsCommsConsole))
            {
                Messages.Message("FantasyBiotech_MessageShipHasLeftCommsRange".Translate(__instance.FullTitle), MessageTypeDefOf.SituationResolved);
            }
            __instance.passingShipManager.RemoveShip(__instance);
            return false;
        }

    }
}