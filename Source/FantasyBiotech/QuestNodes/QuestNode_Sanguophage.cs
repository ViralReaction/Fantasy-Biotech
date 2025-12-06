using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace FantasyBiotech
{
	public class QuestNode_Root_SanguophageAssault : QuestNode
	{
		private const string QuestTag = "VR_VampireAssault";

		private const int TicksToShuttleArrival = 180;

		private const int TicksToReinforcements = 10000;

		private const int TicksToBeginAssault = 5000;

		private static readonly SimpleCurve PointsToThrallCountCurve = new SimpleCurve
		{
			new CurvePoint(0f, 2f),
			new CurvePoint(2000f, 5f)
		};

		private static readonly SimpleCurve PointsToReinforcementsCountCurve = new SimpleCurve
		{
			new CurvePoint(2000f, 0f),
			new CurvePoint(2500f, 2f),
			new CurvePoint(5000f, 4f),
			new CurvePoint(8000f, 6f)
		};

		public override void RunInt()
		{
			if (!ModLister.CheckBiotech("Vampire assault"))
			{
				return;
			}
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = QuestGen_Get.GetMap();
			float x = slate.Get("points", 0f);
			int endTicks = 5240;
			string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("VR_VampireAssault");
			string attackedSignal = QuestGenUtility.HardcodedSignalWithQuestID("initialPawns.TookDamageFromPlayer");
			string raidArrivedSignal = QuestGen.GenerateNewSignal("RaidArrived");
			string defendTimeoutSignal = QuestGen.GenerateNewSignal("DefendTimeout");
			string beginAssaultSignal = QuestGen.GenerateNewSignal("BeginAssault");
			string assaultBeganSignal = QuestGen.GenerateNewSignal("AssaultBegan");
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved");
			slate.Set("map", map);
			List<FactionRelation> list = [];
			foreach (Faction item3 in Find.FactionManager.AllFactionsListForReading)
			{
				list.Add(new FactionRelation(item3, FactionRelationKind.Hostile));
			}
			Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDefOf.Sanguophages, list, hidden: true);
			faction.temporary = true;
			Find.FactionManager.Add(faction);
			quest.ReserveFaction(faction);
			List<Pawn> initialPawns = [];
			Pawn vampire = quest.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Sanguophage, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true));
			vampire.health.forceDowned = true;
			initialPawns.Add(vampire);
			slate.Set("vampire", vampire);
			int num = Mathf.RoundToInt(PointsToThrallCountCurve.Evaluate(x));
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SanguophageThrall, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true);
			for (int i = 0; i < num; i++)
			{
				Pawn item = quest.GeneratePawn(request);
				initialPawns.Add(item);
			}
			slate.Set("thrallCount", num);
			slate.Set("initialPawns", initialPawns);
			Thing thing = ThingMaker.MakeThing(ThingDefOf.ShuttleCrashed_Exitable);
			quest.SetFaction(Gen.YieldSingle(thing), faction);

			quest.End(QuestEndOutcome.Fail, 0, null, inSignal);
			int reinforcementsCount = Mathf.RoundToInt(PointsToReinforcementsCountCurve.Evaluate(x));
			if (reinforcementsCount > 0)
			{
				endTicks = 10060;
			}
			List<Pawn> reinforcements = null;
			if (reinforcementsCount > 0)
			{
				reinforcements = new List<Pawn>();
				for (int j = 0; j < reinforcementsCount; j++)
				{
					Pawn item2 = quest.GeneratePawn(request);
					reinforcements.Add(item2);
				}
				quest.BiocodeWeapons(reinforcements);
			}

			quest.Delay(180, delegate
			{
				quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, initialPawns, filterDeadPawnsFromLookTargets: false, "[sanguophageShuttleCrashedLetterText]", null, "[sanguophageShuttleCrashedLetterLabel]");
				QuestPart_PawnsArrive questPart_PawnsArrive = new QuestPart_PawnsArrive();
				questPart_PawnsArrive.pawns.AddRange(initialPawns);
				questPart_PawnsArrive.arrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
				questPart_PawnsArrive.mapParent = map.Parent;
				questPart_PawnsArrive.inSignal = raidArrivedSignal;
				questPart_PawnsArrive.sendStandardLetter = false;
				questPart_PawnsArrive.addPawnsToLookTargets = false;
				quest.AddPart(questPart_PawnsArrive);




				//quest.DefendPoint(map.Parent, vampire, shuttleCrashPosition, initialPawns, faction, null, null, 5f);
				quest.Delay(5000, delegate
				{
					quest.SignalPass(null, null, attackedSignal);
				}).debugLabel = "Assault delay";
				quest.AnySignal(new string[2]
				{
					attackedSignal, defendTimeoutSignal
				}, null, Gen.YieldSingle(beginAssaultSignal));
				quest.SignalPassActivable(delegate
				{
					quest.AnyPawnInCombatShape(initialPawns, delegate
					{
						QuestPart_AssaultColony questPart_AssaultColony2 = quest.AssaultColony(faction, map.Parent, initialPawns);
						questPart_AssaultColony2.canKidnap = false;
						questPart_AssaultColony2.canSteal = false;
						questPart_AssaultColony2.canTimeoutOrFlee = false;
						quest.Letter(LetterDefOf.ThreatSmall, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, initialPawns, filterDeadPawnsFromLookTargets: false, "[assaultBeginLetterText]", null, "[assaultBeginLetterLabel]");
					}, null, null, assaultBeganSignal);
				}, null, beginAssaultSignal, null, null, assaultBeganSignal);
				if (reinforcementsCount > 0)
				{
					quest.Delay(10000, delegate
					{
						quest.Letter(LetterDefOf.ThreatBig, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, reinforcements, filterDeadPawnsFromLookTargets: false, "[reinforcementsArrivedLetterText]", null, "[reinforcementsArrivedLetterLabel]");
						DropCellFinder.TryFindRaidDropCenterClose(out var spot, map);
						quest.DropPods(map.Parent, reinforcements, null, null, null, null, false, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.OngoingOnly, spot);
						QuestPart_AssaultColony questPart_AssaultColony = quest.AssaultColony(faction, map.Parent, reinforcements);
						questPart_AssaultColony.canSteal = false;
						questPart_AssaultColony.canTimeoutOrFlee = false;
					}).debugLabel = "Reinforcements delay";
				}
				quest.Delay(endTicks, delegate
				{
					QuestGen_End.End(quest, QuestEndOutcome.Success);
				}).debugLabel = "End delay";
			}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, null, tickHistorically: false, QuestPart.SignalListenMode.OngoingOnly, waitUntilPlayerHasHomeMap: true).debugLabel = "Arrival delay";
		}

		public override bool TestRunInt(Slate slate)
		{
			Map map = QuestGen_Get.GetMap();
			if (map == null)
			{
				return false;
			}
			return FactionDefOf.Sanguophages.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp);
		}

	}

}