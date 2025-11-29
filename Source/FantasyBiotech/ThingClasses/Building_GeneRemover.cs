using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace FantasyBiotech;

[StaticConstructorOnStartup]

public class Building_GeneRemover : Building_Enterable, IThingHolderWithDrawnPawn, IThingHolder
{
	public int ticksRemaining;

	[Unsaved]
	private Texture2D cachedInsertPawnTex;

	[Unsaved]
	private Sustainer sustainerWorking;

	[Unsaved]
	private Effecter progressBar;

	private const int TicksToExtract = 12000;

	private const float TicksToExtractFloat = TicksToExtract;

	private const float DoctorSkillToBleedModifier = 30f;
	private const float BleedStartingValue = 0.75f;



	private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	private static readonly SimpleCurve GeneCountChanceCurve = new SimpleCurve
	{
		new CurvePoint(1f, 0.7f),
		new CurvePoint(2f, 0.2f),
		new CurvePoint(3f, 0.08f),
		new CurvePoint(4f, 0.02f)
	};

	public Pawn ContainedPawn => innerContainer.FirstOrDefault() as Pawn;

	public override bool IsContentsSuspended => false;

	public Texture2D InsertPawnTex
	{
		get
		{
			if (cachedInsertPawnTex == null)
			{
				cachedInsertPawnTex = ContentFinder<Texture2D>.Get("UI/Gizmos/InsertPawn");
			}
			return cachedInsertPawnTex;
		}
	}

	public float HeldPawnDrawPos_Y => DrawPos.y + 0.03658537f;

	public float HeldPawnBodyAngle => Rotation.Opposite.AsAngle;

	public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

	public override Vector3 PawnDrawOffset => IntVec3.West.RotatedBy(Rotation).ToVector3() / def.size.x;

	public override void PostPostMake()
	{
		if (!ModLister.CheckBiotech("gene extractor"))
		{
			Destroy();
		}
		else
		{
			base.PostPostMake();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		sustainerWorking = null;
		if (progressBar != null)
		{
			progressBar.Cleanup();
			progressBar = null;
		}
		base.DeSpawn(mode);
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
		if (Working)
		{
			if (ContainedPawn == null)
			{
				Cancel();
				return;
			}
			TickEffects();
			if (ticksRemaining <= 0)
			{
				Finish();
			}
			return;
		}
		if (selectedPawn?.Dead ?? false)
		{
			Cancel();
		}
		if (progressBar == null) return;
		progressBar.Cleanup();
		progressBar = null;
	}

	private void TickEffects()
	{
		if (sustainerWorking == null || sustainerWorking.Ended)
		{
			sustainerWorking = SoundDefOf.GeneExtractor_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
		}
		else
		{
			sustainerWorking.Maintain();
		}
		progressBar ??= EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
		progressBar.EffectTick(new TargetInfo(Position + IntVec3.North.RotatedBy(Rotation), Map), TargetInfo.Invalid);
		MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
		if (mote == null) return;
		mote.progress = 1f - Mathf.Clamp01(ticksRemaining / TicksToExtractFloat);
		mote.offsetZ = ((Rotation == Rot4.North) ? 0.5f : (-0.5f));
	}

	public override AcceptanceReport CanAcceptPawn(Pawn pawn)
	{
		if (!pawn.IsColonist && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony && (!pawn.IsColonySubhuman || !pawn.IsGhoul))
		{
			return false;
		}
		if (selectedPawn != null && selectedPawn != pawn)
		{
			return false;
		}
		if (!pawn.RaceProps.Humanlike || pawn.IsQuestLodger())
		{
			return false;
		}
		if (innerContainer.Count > 0)
		{
			return "Occupied".Translate();
		}
		if (pawn.genes == null || !pawn.genes.GenesListForReading.Any((Gene x) => x.def.passOnDirectly))
		{
			return "PawnHasNoGenes".Translate(pawn.Named("PAWN"));
		}
		if (!pawn.genes.GenesListForReading.Any((Gene x) => x.def.biostatArc == 0))
		{
			return "PawnHasNoNonArchiteGenes".Translate(pawn.Named("PAWN"));
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.XenogerminationComa))
		{
			return "InXenogerminationComa".Translate();
		}
		return true;
	}

	private void Cancel()
	{
		startTick = -1;
		selectedPawn = null;
		sustainerWorking = null;
		innerContainer.TryDropAll(def.hasInteractionCell ? InteractionCell : base.Position, base.Map, ThingPlaceMode.Near);
	}

	public void Finish(Pawn pawn = null)
	{
		selectedPawn = null;
		sustainerWorking = null;
		List<GeneDef> genesToAdd;
		if (ContainedPawn != null)
		{
			Pawn containedPawn = ContainedPawn;
			Rand.PushState(containedPawn.thingIDNumber ^ startTick);
			genesToAdd = [];
			Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
			int num = Mathf.Min((int)GeneCountChanceCurve.RandomElementByWeight((CurvePoint p) => p.y).x, containedPawn.genes.GenesListForReading.Count((Gene x) => x.def.biostatArc == 0));
			for (int i = 0; i < num; i++)
			{
				if (!containedPawn.genes.GenesListForReading.TryRandomElementByWeight(SelectionWeight, out var result))
				{
					break;
				}
				genesToAdd.Add(result.def);
			}
			if (genesToAdd.Any())
			{
				genepack.Initialize(genesToAdd);
			}
			GeneUtility.ExtractXenogerm(containedPawn, Mathf.RoundToInt(60000f * GeneTuning.GeneExtractorRegrowingDurationDaysRange.RandomInRange));
			if (pawn != null)
			{
				ApplyHediff(pawn, containedPawn);
			}
			IntVec3 intVec = (def.hasInteractionCell ? InteractionCell : base.Position);
			innerContainer.TryDropAll(intVec, base.Map, ThingPlaceMode.Near);
			if (!containedPawn.Dead && (containedPawn.IsPrisonerOfColony || containedPawn.IsSlaveOfColony))
			{
				containedPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.XenogermHarvested_Prisoner);
			}
			if (genesToAdd.Any())
			{
				GenPlace.TryPlaceThing(genepack, intVec, base.Map, ThingPlaceMode.Near);
			}
			Messages.Message("GeneExtractionComplete".Translate(containedPawn.Named("PAWN")) + ": " + genesToAdd.Select((GeneDef x) => x.label).ToCommaList().CapitalizeFirst(), new LookTargets(containedPawn, genepack), MessageTypeDefOf.PositiveEvent);
			Rand.PopState();
		}
		startTick = -1;
		float SelectionWeight(Gene g)
		{
			if (genesToAdd.Contains(g.def))
			{
				return 0f;
			}
			if (g.def.biostatArc > 0)
			{
				return 0f;
			}
			if (g.def.endogeneCategory == EndogeneCategory.Melanin)
			{
				return 0f;
			}
			if (!GeneTuning.BiostatRange.Includes(g.def.biostatMet + genesToAdd.Sum((GeneDef x) => x.biostatMet)))
			{
				return 0f;
			}
			if (g.def.biostatCpx > 0)
			{
				return 3f;
			}
			return 1f;
		}
	}

	public static void ApplyHediff(Pawn pawn, Pawn containedPawn)
	{
		if (containedPawn != null)
		{
			float skillLevel = pawn.skills.GetSkill(SkillDefOf.Medicine).Level / DoctorSkillToBleedModifier;
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, containedPawn);
			hediff.Severity = BleedStartingValue - skillLevel;
			containedPawn.health.AddHediff(hediff);
		}
	}

	public override void TryAcceptPawn(Pawn pawn)
	{
		if (CanAcceptPawn(pawn))
		{
			selectedPawn = pawn;
			bool validPawn = pawn.Corpse?.DeSpawnOrDeselect() ?? pawn.DeSpawnOrDeselect();
			if (innerContainer.TryAddOrTransfer(pawn))
			{
				startTick = Find.TickManager.TicksGame;
				ticksRemaining = TicksToExtract;
			}
			if (validPawn)
			{
				Find.Selector.Select(pawn.Corpse != null ? pawn.Corpse : pawn, playSound: false, forceDesignatorDeselect: false);
			}
		}
	}

	public override void SelectPawn(Pawn pawn)
	{
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.XenogermReplicating))
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmExtractXenogermWillKill".Translate(pawn.Named("PAWN")), delegate
			{
				base.SelectPawn(pawn);
			}));
		}
		else
		{
			base.SelectPawn(pawn);
		}
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption;
		}
		if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		AcceptanceReport acceptanceReport = CanAcceptPawn(selPawn);
		if (acceptanceReport.Accepted)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
			{
				SelectPawn(selPawn);
			}), selPawn, this);
		}
		else if (SelectedPawn == selPawn && !selPawn.IsPrisonerOfColony)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
			{
				selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.EnterBuilding, this), JobTag.Misc);
			}), selPawn, this);
		}
		else if (!acceptanceReport.Reason.NullOrEmpty())
		{
			yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (Working)
		{
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = "CommandCancelExtraction".Translate(),
				defaultDesc = "CommandCancelExtractionDesc".Translate(),
				icon = CancelIcon,
				action = Cancel,
				activateSound = SoundDefOf.Designate_Cancel
			};
			yield return command_Action;
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action2 = new Command_Action
				{
					defaultLabel = "DEV: Finish extraction",
					action = () => Finish()
				};
				yield return command_Action2;
			}
			yield break;
		}
		if (selectedPawn != null)
		{
			Command_Action command_Action3 = new Command_Action
			{
				defaultLabel = "CommandCancelLoad".Translate(),
				defaultDesc = "CommandCancelLoadDesc".Translate(),
				icon = CancelIcon,
				activateSound = SoundDefOf.Designate_Cancel,
				action = delegate
				{
					innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
					if (selectedPawn.CurJobDef == JobDefOf.EnterBuilding)
					{
						selectedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					selectedPawn = null;
					startTick = -1;
					sustainerWorking = null;
				}
			};
			yield return command_Action3;
			yield break;
		}
		Command_Action insertPawnActions = new Command_Action
		{
			defaultLabel = "InsertPerson".Translate() + "...",
			defaultDesc = "InsertPersonGeneExtractorDesc".Translate(),
			icon = InsertPawnTex,
			action = delegate
			{
				List<FloatMenuOption> list = [];
				foreach (Pawn item in Map.mapPawns.AllPawnsSpawned.OrderBy(delegate(Pawn p)
				{
					if (p.IsColonist)
					{
						return 0;
					}
					return (p.IsPrisonerOfColony || p.IsSlaveOfColony) ? 1 : 2;
				}).ThenBy((Pawn p) => p.Label))
				{
					Pawn pawn = item;
					if (pawn.genes != null)
					{
						AcceptanceReport acceptanceReport = CanAcceptPawn(pawn);
						string text = pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap;
						if (!acceptanceReport.Accepted)
						{
							if (!acceptanceReport.Reason.NullOrEmpty())
							{
								list.Add(new FloatMenuOption(text + ": " + acceptanceReport.Reason, null, pawn, Color.white));
							}
						}
						else
						{
							Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);
							if (firstHediffOfDef != null)
							{
								text = text + " (" + firstHediffOfDef.LabelBase + ", " + firstHediffOfDef.TryGetComp<HediffComp_Disappears>().ticksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true).Colorize(ColoredText.SubtleGrayColor) + ")";
							}
							list.Add(new FloatMenuOption(text, delegate
							{
								SelectPawn(pawn);
							}, pawn, Color.white));
						}
					}
				}
				if (!list.Any())
				{
					list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
		yield return insertPawnActions;
	}

	public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
	{
		base.DynamicDrawPhaseAt(phase, drawLoc, flip);
		if (base.Working && ContainedPawn != null)
		{
			ContainedPawn.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc + PawnDrawOffset, null, neverAimWeapon: true);
		}
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (selectedPawn != null && innerContainer.Count == 0)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "WaitingForPawn".Translate(selectedPawn.Named("PAWN")).Resolve();
		}
		else if (base.Working && ContainedPawn != null)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = text + "ExtractingXenogermFrom".Translate(ContainedPawn.Named("PAWN")).Resolve();
			//text = ((text + "DurationLeft".Translate(ticksRemaining.ToStringTicksToPeriod()).Resolve()));
		}
		return text;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
	}
}
