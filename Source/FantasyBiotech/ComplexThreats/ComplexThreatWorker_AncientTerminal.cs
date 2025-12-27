
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FantasyBiotech
{
	public class ComplexThreatWorker_AncientTerminal : ComplexThreatWorker
	{
		private const string SignalPrefix = "RaidSignal";

		public override bool CanResolveInt(ComplexResolveParams parms)
		{
			if (!ModsConfig.IdeologyActive) return false;
			if (base.CanResolveInt(parms) && TryFindRandomEnemyFaction(parms, out Faction _))
			{
				return ComplexUtility.TryFindRandomSpawnCell(FantasyBiotechDefOf.VR_AncientEnemyTerminal, parms.room, parms.map, out IntVec3 spawnPosition);
			}
			return false;
		}

		public override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
		{
			ComplexUtility.TryFindRandomSpawnCell(FantasyBiotechDefOf.VR_AncientEnemyTerminal, parms.room, parms.map, out IntVec3 spawnPosition);
			Thing thing = GenSpawn.Spawn(FantasyBiotechDefOf.VR_AncientEnemyTerminal, spawnPosition, parms.map);
			TryFindRandomEnemyFaction(parms, out Faction faction);
			float num = Mathf.Max(parms.points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat) * 1.05f);
			IncidentParms incidentParms = new IncidentParms
			{
				forced = true,
				target = parms.map,
				points = num,
				faction = faction,
				raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
				raidStrategy = RaidStrategyDefOf.ImmediateAttack
			};
			CompHackable compHackable = thing.TryGetComp<CompHackable>();
			string text = compHackable.hackingCompletedSignal;
			if (text.NullOrEmpty())
			{
				text = (compHackable.hackingCompletedSignal = SignalPrefix + Find.UniqueIDsManager.GetNextSignalTagID());
			}
			SignalAction_Incident obj = (SignalAction_Incident)ThingMaker.MakeThing(ThingDefOf.SignalAction_Incident);
			obj.incident = IncidentDefOf.RaidEnemy;
			obj.incidentParms = incidentParms;
			obj.signalTag = text;
			GenSpawn.Spawn(obj, parms.room.rects[0].CenterCell, parms.map);
			threatPointsUsed += num;
		}

		private static bool TryFindRandomEnemyFaction(ComplexResolveParams parms, out Faction faction)
		{
			if (parms.hostileFaction != null && parms.hostileFaction.def.humanlikeFaction)
			{
				faction = parms.hostileFaction;
				return true;
			}
			faction = Find.FactionManager.RandomRaidableEnemyFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
			return faction != null;
		}
	}
}