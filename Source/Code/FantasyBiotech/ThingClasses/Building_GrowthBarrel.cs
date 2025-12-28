using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace FantasyBiotech
{
	[StaticConstructorOnStartup]
	public class Building_GrowthBarrel : Building_GrowthVat
	{
		[Unsaved(false)]
		private Graphic cachedLidGraphic;

		[Unsaved(false)]
		private Effecter bubbleEffecterLeft;
		private Graphic LidGraphic
		{
			get
			{
				if (cachedLidGraphic == null)
				{
					Color stuffColor = Stuff.stuffProps.color;
					Log.Message(stuffColor.ToString());
					cachedLidGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Building/Misc/GrowthBarrel/GrowthBarrelFull", ShaderDatabase.CutoutComplex, def.graphicData.drawSize, stuffColor);
				}
				return cachedLidGraphic;
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (!CompsPreventDrawing())
			{
				if (def.drawerType == DrawerType.RealtimeOnly || !Spawned)
				{
					Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);
				}
				SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);
			}
			Comps_DrawAt(drawLoc, flip);
			Comps_PostDraw();
			if (Working && selectedPawn == null && selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
			{
				LidGraphic.Draw(DrawPos + Altitudes.AltIncVect * 0.50f, Rot4.North, this);
			}

		}
		public override void Tick()
		{
			if (comps != null)
			{
				int i = 0;
				for (int count = comps.Count; i < count; i++)
				{
					comps[i].CompTick();
				}
			}
			if (selectedPawn is not { Destroyed: false } && selectedEmbryo is not { Destroyed: false })
			{
				OnStop();
			}
			foreach (Thing item in innerContainer)
			{
				if (item is HumanEmbryo humanEmbryo2 && humanEmbryo2 != selectedEmbryo)
				{
					innerContainer.TryDrop(item, InteractionCell, Map, ThingPlaceMode.Near, 1, out Thing _);
				}
			}
			if (Working)
			{
				GrowthVatWorking();
			}
			else
			{
				TryGrowEmbryo();
				bubbleEffecter?.Cleanup();
				bubbleEffecterLeft?.Cleanup();
				bubbleEffecterLeft = null;
				bubbleEffecter = null;
			}
		}

		private void GrowthVatWorking()
		{
			if (selectedPawn != null)
			{
				if (selectedPawn.ageTracker.AgeBiologicalYearsFloat >= 18f)
				{
					Messages.Message("OccupantEjectedFromGrowthVat".Translate(selectedPawn.Named("PAWN")) + ": " + "PawnIsTooOld".Translate(selectedPawn.Named("PAWN")), selectedPawn, MessageTypeDefOf.NeutralEvent);
					Finish();
					return;
				}
				if (innerContainer.Contains(selectedPawn))
				{
					int ticks = Mathf.RoundToInt(20f * selectedPawn.GetStatValue(StatDefOf.GrowthVatOccupantSpeed));
					selectedPawn.ageTracker.Notify_TickedInGrowthVat(ticks);
					VatLearning?.Tick();
					VatLearning?.PostTick();
				}
				float num = BiostarvationDailyOffset / 60000f * HediffDefOf.BioStarvation.maxSeverity;
				Hediff firstHediffOfDef = selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BioStarvation);
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity += num;
					if (firstHediffOfDef.ShouldRemove)
					{
						selectedPawn.health.RemoveHediff(firstHediffOfDef);
					}
				}
				else if (num > 0f)
				{
					Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BioStarvation, selectedPawn);
					hediff.Severity = num;
					selectedPawn.health.AddHediff(hediff);
				}
			}
			else if (selectedEmbryo != null)
			{
				if (EmbryoGestationTicksRemaining <= 0)
				{
					Finish();
					return;
				}
				embryoStarvation = Mathf.Clamp01(embryoStarvation + BiostarvationDailyOffset / 60000f);
			}
			if (BiostarvationSeverityPercent >= 1f)
			{
				Fail();
				return;
			}
			if (sustainerWorking == null || sustainerWorking.Ended)
			{
				sustainerWorking = SoundDefOf.GrowthVat_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			}
			else
			{
				sustainerWorking.Maintain();
			}
			containedNutrition = Mathf.Clamp(containedNutrition - NutritionConsumedPerDay / 60000f, 0f, 2.1474836E+09f);
			if (containedNutrition <= 0f)
			{
				TryAbsorbNutritiousThing();
			}
			// if (GlowMotePerRotation == null)
			// {
			// 	GlowMotePerRotation = new Dictionary<Rot4, ThingDef>
			// 	{
			// 		{
			// 			Rot4.South, ThingDefOf.Mote_VatGlowVertical
			// 		},
			// 		{
			// 			Rot4.East, ThingDefOf.Mote_VatGlowHorizontal
			// 		},
			// 		{
			// 			Rot4.West, ThingDefOf.Mote_VatGlowHorizontal
			// 		},
			// 		{
			// 			Rot4.North, ThingDefOf.Mote_VatGlowVertical
			// 		}
			// 	};
			// }
			// if (this.IsHashIntervalTick(132))
			// {
			// 	MoteMaker.MakeStaticMote(DrawPos, MapHeld, GlowMotePerRotation[Rotation]);
			// }
			bubbleEffecterLeft ??= FantasyBiotechDefOf.VR_Vat_Bubbles_Left.SpawnAttached(this, MapHeld);
			bubbleEffecter ??= FantasyBiotechDefOf.VR_Vat_Bubbles_Right.SpawnAttached(this, MapHeld);
			bubbleEffecter.EffectTick(this, this);
			bubbleEffecterLeft.EffectTick(this, this);
		}

		public override AcceptanceReport CanAcceptPawn(Pawn pawn)
		{
			if (Working)
			{
				return "Occupied".Translate();
			}
			if (selectedEmbryo != null)
			{
				return "EmbryoSelected".Translate();
			}
			if (pawn.ageTracker.AgeBiologicalYearsFloat >= 18f)
			{
				return "TooOld".Translate(pawn.Named("PAWN"), 18f.Named("AGEYEARS"));
			}
			if (selectedPawn != null && selectedPawn != pawn)
			{
				return "WaitingForPawn".Translate(selectedPawn.Named("PAWN"));
			}
			if (pawn.health.hediffSet.HasHediff(HediffDefOf.BioStarvation))
			{
				return "PawnBiostarving".Translate(pawn.Named("PAWN"));
			}
			return pawn.IsColonist && !pawn.IsQuestLodger();
		}
	}
}
