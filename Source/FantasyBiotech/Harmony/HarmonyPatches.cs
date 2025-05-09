using System.Reflection;
using Verse;
using HarmonyLib;


namespace FantasyBiotech
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("VR.Fantasy.Biotech");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
