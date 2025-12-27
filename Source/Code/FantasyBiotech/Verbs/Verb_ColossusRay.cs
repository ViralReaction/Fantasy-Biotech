using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse;

namespace FantasyBiotech
{

	public class Verb_ColossusRay : Verb_ShootBeam
	{
		private CompColossusRayExplosion comp;

		public override bool TryCastShot()
		{
			comp = caster.TryGetComp<CompColossusRayExplosion>();
			if (comp == null) return false;
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out ShootLine resultingLine);
			if (verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			if (EquipmentSource != null)
			{
				EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
				EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
			}
			lastShotTick = Find.TickManager.TicksGame;
			ticksToNextPathStep = TicksBetweenBurstShots;
			IntVec3 targetCell = InterpolatedPosition.Yto0().ToIntVec3();
			if (!TryGetHitCell(resultingLine.Source, targetCell, out IntVec3 hitCell))
			{
				return true;
			}
			HitCell(hitCell, resultingLine.Source);
			if (!verbProps.beamHitsNeighborCells) return true;
			hitCells.Add(hitCell);
			foreach (IntVec3 beamHitNeighbourCell in GetBeamHitNeighbourCells(resultingLine.Source, hitCell))
			{
				if (hitCells.Contains(beamHitNeighbourCell)) continue;
				HitCell(beamHitNeighbourCell, resultingLine.Source);
				hitCells.Add(beamHitNeighbourCell);
			}
			return true;
		}

		private void HitCell(IntVec3 cell, IntVec3 sourceCell)
		{
			if (!cell.InBounds(caster.Map)) return;
			comp.QueueExplosion(cell, caster);
			if (verbProps.beamSetsGroundOnFire && Rand.Chance(verbProps.beamChanceToStartFire))
			{
				FireUtility.TryStartFireIn(cell, caster.Map, 1f, caster);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && path == null)
			{
				comp = caster.TryGetComp<CompColossusRayExplosion>();
				path = [];
			}
		}
	}

}