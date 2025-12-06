using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
namespace FantasyBiotech
{
    public class QuestNode_StartingMechWalkIn : QuestNode_Root_MechanitorStartingMech
    {
        public override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;
            Pawn pawn = slate.Get<Pawn>("asker");
            PawnKindDef pawnKindDef = mechTypes.RandomElement();
            Map map = pawn.MapHeld;
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, pawn.Faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
            Pawn mech = quest.GeneratePawn(request);
            slate.Set("mechanitor", pawn);
            slate.Set("mech", pawnKindDef);
            quest.Delay(600, delegate
            {
                quest.PawnsArrive(Gen.YieldSingle(mech), null, map.Parent, PawnsArrivalModeDefOf.EdgeWalkIn, sendStandardLetter:false);
                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(mech), filterDeadPawnsFromLookTargets: false, "[arrivalLetterText]", null, "[arrivalLetterLabel]");
                quest.AssignMechToMechanitor(pawn, mech);
                QuestGen_End.End(quest, QuestEndOutcome.Success);

            });
            quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("asker.Killed"));
        }
    }
}
