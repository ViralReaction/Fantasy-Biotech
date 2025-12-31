using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.CacheWorkGiversInOrder))]
    public static class PawnWorkSettings_CacheWorkGiversInOrder
    {

        public static void Postfix(Pawn_WorkSettings __instance)
        {
            List<WorkGiverDef> extension = __instance.pawn.def.GetModExtension<WorkGiverExtension>()?.workGivers;
            if (extension != null)
            {
                for (int i = 0; i < extension.Count; i++)
                {
                    WorkGiverDef workGiverDef = extension[i];
                    WorkGiver worker = workGiverDef.Worker;
                    __instance.workGiversInOrderNormal.Add(worker);
                    //__instance.workGiversInOrderEmerg.Add(worker);
                }
            }
        }
        
    }

   
}
