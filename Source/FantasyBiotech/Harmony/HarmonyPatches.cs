using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
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
