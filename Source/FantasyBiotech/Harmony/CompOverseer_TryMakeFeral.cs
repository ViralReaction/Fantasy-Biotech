using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FantasyBiotech
{
    [HarmonyPatch(typeof(CompOverseerSubject), "TryMakeFeral")]
    public static class CompOverseerSubject_TryMakeFeral
    {
        public static bool Prefix(CompOverseerSubject __instance, ref bool __result)
        {
            if (__instance.parent.def.GetModExtension<ConstructExtension>()?.isConstruct != true) return true;
            __result = TryMakerFeralConstruct(__instance);
            return false;
        }

        private static bool TryMakerFeralConstruct(CompOverseerSubject __instance)
        {
            List<Pawn> tmpFeralPawns = __instance.tmpFeralPawns;
            tmpFeralPawns.Clear();
            tmpFeralPawns.Add(__instance.Parent);
            foreach (Thing item in GenRadial.RadialDistinctThingsAround(__instance.parent.Position, __instance.parent.Map, __instance.Props.feralCascadeRadialDistance, useCenter: true))
            {
                if (item is Pawn pawn && !tmpFeralPawns.Contains(pawn) && CanGoFeral(pawn))
                {
                    tmpFeralPawns.Add(pawn);
                }
            }
            for (int i = 0; i < tmpFeralPawns.Count; i++)
            {
                tmpFeralPawns[i].OverseerSubject.ForceFeral();
            }
            Dictionary<PawnKindDef, int> counts = new Dictionary<PawnKindDef, int>();
            foreach (PawnKindDef pawnKindDef in tmpFeralPawns.Select(pawn => pawn.kindDef))
            {
                if (counts.TryGetValue(pawnKindDef, out int c))
                {
                    counts[pawnKindDef] = c + 1;
                }
                else
                {
                    counts.Add(pawnKindDef, 1);
                }
            }
            List<string> lines = new List<string>(counts.Count);
            foreach (KeyValuePair<PawnKindDef, int> kvp in counts)
            {
                lines.Add($"{kvp.Key.LabelCap} x{kvp.Value}");
            }

            TaggedString body = "FantasyBiotech_LetterLabelConstructsFeral".Translate(MechUtility.ConstructFaction(), lines.ToLineList(" - "));

            Find.LetterStack.ReceiveLetter("FantasyBiotech_LetterConstructsFeral".Translate(), body, LetterDefOf.ThreatBig, tmpFeralPawns);

            for (int j = 0; j < tmpFeralPawns.Count; j++)
            {
                tmpFeralPawns[j].GetLord()?.Notify_PawnLost(tmpFeralPawns[j], PawnLostCondition.ForcedToJoinOtherLord);
            }
            LordMaker.MakeNewLord(MechUtility.ConstructFaction(), new LordJob_ExitMapBest(LocomotionUrgency.Jog, canDig: false, canDefendSelf: true), __instance.parent.MapHeld, tmpFeralPawns);
            for (int k = 0; k < tmpFeralPawns.Count; k++)
            {
                if (tmpFeralPawns[k].CurJob != null)
                {
                    tmpFeralPawns[k].jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
            tmpFeralPawns.Clear();
            return true;
        }
        private static bool CanGoFeral(Pawn pawn)
        {
            if (!pawn.Spawned || !pawn.Awake())
            {
                return false;
            }
            return pawn.IsColonyMechRequiringMechanitor();
        }
    }
}
