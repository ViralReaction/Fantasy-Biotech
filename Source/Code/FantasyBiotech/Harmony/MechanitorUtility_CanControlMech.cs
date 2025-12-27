using HarmonyLib;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(MechanitorUtility), "CanControlMech")]
    public static class MechanitorUtility_CanControlMech
    {
        public static bool Prefix(Pawn pawn, Pawn mech, ref AcceptanceReport __result)
        {
            ConstructExtension extension = mech.def.GetModExtension<ConstructExtension>();
            if (extension?.isConstruct == true && !MechUtility.IsArtificer(pawn))
            {
                __result = "FantasyBiotech.CannotControlConstruct";
                return false;
            }
            return true;
        }
    }
}
