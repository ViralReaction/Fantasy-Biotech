using HarmonyLib;

using Verse.Profile;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
    public static class MemoryUtility_ClearAllMapsAndWorld
    {
        public static void Postfix()
        {
            MechUtility.ClearFactionCache();
        }

    }
}
