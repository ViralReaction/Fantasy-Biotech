using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FantasyBiotech
{
    [StaticConstructorOnStartup]
    public class Building_DynamoCoreScanner : Building_SubcoreScanner
    {
       
        public override AcceptanceReport CanAcceptPawn(Pawn selPawn)
        {
            if (!selPawn.IsColonist && !selPawn.IsSlaveOfColony && !selPawn.IsPrisonerOfColony)
            {
                return false;
            }
            if (selectedPawn != null && selectedPawn != selPawn)
            {
                return false;
            }
            if (!PowerOn)
            {
                return "CannotUseNoPower".Translate();
            }
            if (State != SubcoreScannerState.WaitingForOccupant)
            {
                switch (State)
                {
                    case SubcoreScannerState.Inactive:
                        return "SubcoreScannerNotInit".Translate();
                    case SubcoreScannerState.WaitingForIngredients:
                        {
                            StringBuilder stringBuilder = new StringBuilder("SubcoreScannerRequiresIngredients".Translate() + ": ");
                            bool flag = false;
                            for (int i = 0; i < def.building.subcoreScannerFixedIngredients.Count; i++)
                            {
                                IngredientCount ingredientCount = def.building.subcoreScannerFixedIngredients[i];
                                int num = innerContainer.TotalStackCountOfDef(ingredientCount.FixedIngredient);
                                int num2 = (int)ingredientCount.GetBaseCount();
                                if (num < num2)
                                {
                                    if (flag)
                                    {
                                        stringBuilder.Append(", ");
                                    }
                                    stringBuilder.Append($"{ingredientCount.FixedIngredient.LabelCap} x{num2 - num}");
                                    flag = true;
                                }
                            }
                            return stringBuilder.ToString();
                        }
                    case SubcoreScannerState.Occupied:
                        return "SubcoreScannerOccupied".Translate();
                }
            }
            else
            {
                if (selPawn.IsQuestLodger())
                {
                    return "CryptosleepCasketGuestsNotAllowed".Translate();
                }
                if (selPawn.health.hediffSet.HasHediff(HediffDefOf.ScanningSickness))
                {
                    return "SubcoreScannerPawnHasSickness".Translate(HediffDefOf.ScanningSickness.label);
                }
                if (selPawn.DevelopmentalStage.Baby())
                {
                    return "SubcoreScannerBabyNotAllowed".Translate();
                }
            }
            return true;
        }

        public override void TryAcceptPawn(Pawn pawn)
        {
            if ((bool)CanAcceptPawn(pawn))
            {
                bool num = pawn.DeSpawnOrDeselect();
                if (pawn.holdingOwner != null)
                {
                    pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer);
                }
                else
                {
                    innerContainer.TryAdd(pawn);
                }
                if (num)
                {
                    Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
                }
                fabricationTicksLeft = def.building.subcoreScannerTicks;
            }
        }

        public void EjectBuildingContents()
        {
            Pawn occupant = Occupant;
            if (occupant == null)
            {
                innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);
            }
            else
            {
                if (def.building.subcoreScannerHediff != null)
                {
                    occupant.health?.AddHediff(def.building.subcoreScannerHediff);
                }
                if (DestroyOccupantBrain)
                {
                    DestroyOccupant();
                }
                
                for (int i = innerContainer.Count - 1; i >= 0; i--)
                {
                    if (innerContainer[i] is Pawn || innerContainer[i] is Corpse)
                    {
                        innerContainer.TryDrop(innerContainer[i], InteractionCell, base.Map, ThingPlaceMode.Near, 1, out var _);
                    }
                }
                innerContainer.ClearAndDestroyContents();
            }
            selectedPawn = null;
            initScanner = false;
        }

        private void DestroyOccupant()
        {
            Pawn occupant = Occupant;
            DamageInfo dinfo = new DamageInfo(DamageDefOf.ExecutionCut, 9999f, 999f, -1f, null, occupant.health.hediffSet.GetBrain());
            dinfo.SetIgnoreInstantKillProtection(ignore: true);
            dinfo.SetAllowDamagePropagation(val: false);
            occupant.forceNoDeathNotification = true;
            occupant.Destroy();
            occupant.forceNoDeathNotification = false;
            ThoughtUtility.GiveThoughtsForPawnExecuted(occupant, null, PawnExecutionKind.Ripscanned);
            Messages.Message("FantasyBiotech_MessagePawnKilledRipscanner".Translate(occupant.Named("PAWN")), occupant, MessageTypeDefOf.NegativeHealthEvent);
            
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
            if (MotePerRotation == null)
            {
                MotePerRotation = new Dictionary<Rot4, ThingDef>
            {
                {
                    Rot4.South,
                    ThingDefOf.SoftScannerGlow_South
                },
                {
                    Rot4.East,
                    ThingDefOf.SoftScannerGlow_East
                },
                {
                    Rot4.West,
                    ThingDefOf.SoftScannerGlow_West
                },
                {
                    Rot4.North,
                    ThingDefOf.SoftScannerGlow_North
                }
            };
                MotePerRotationRip = new Dictionary<Rot4, ThingDef>
            {
                {
                    Rot4.South,
                    ThingDefOf.RipScannerGlow_South
                },
                {
                    Rot4.East,
                    ThingDefOf.RipScannerGlow_East
                },
                {
                    Rot4.West,
                    ThingDefOf.RipScannerGlow_West
                },
                {
                    Rot4.North,
                    ThingDefOf.RipScannerGlow_North
                }
            };
            }
            SubcoreScannerState state = State;
            if (state == SubcoreScannerState.Occupied)
            {
                fabricationTicksLeft--;
                if (fabricationTicksLeft <= 0)
                {
                    if (!DestroyOccupantBrain)
                    {
                        Messages.Message("FantasyBiotech_MessageSubcoreSoftscannerCompleted".Translate(Occupant.Named("PAWN")), Occupant, MessageTypeDefOf.PositiveEvent);
                    }
                    EjectBuildingContents();
                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(def.building.subcoreScannerOutputDef), InteractionCell, base.Map, ThingPlaceMode.Near);
                    if (def.building.subcoreScannerComplete != null)
                    {
                        def.building.subcoreScannerComplete.PlayOneShot(this);
                    }
                }
                if (workingMote == null || workingMote.Destroyed)
                {
                    workingMote = MoteMaker.MakeAttachedOverlay(this, DestroyOccupantBrain ? MotePerRotationRip[base.Rotation] : MotePerRotation[base.Rotation], Vector3.zero);
                }
                workingMote.Maintain();
                if (DestroyOccupantBrain)
                {
                    if (effectHusk == null)
                    {
                        effectHusk = EffecterDefOf.RipScannerHeadGlow.Spawn(this, base.MapHeld, HuskEffectOffsets[base.Rotation]);
                    }
                    effectHusk.EffectTick(this, this);
                }
                if (progressBarEffecter == null)
                {
                    progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                }
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                mote.progress = 1f - (float)fabricationTicksLeft / (float)def.building.subcoreScannerTicks;
                mote.offsetZ = -0.8f;
                if (def.building.subcoreScannerWorking != null)
                {
                    if (sustainerWorking == null || sustainerWorking.Ended)
                    {
                        sustainerWorking = def.building.subcoreScannerWorking.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                    }
                    else
                    {
                        sustainerWorking.Maintain();
                    }
                }
            }
            else
            {
                effectHusk?.Cleanup();
                effectHusk = null;
                progressBarEffecter?.Cleanup();
                progressBarEffecter = null;
            }
            if (state == SubcoreScannerState.Occupied)
            {
                if (def.building.subcoreScannerStartEffect != null)
                {
                    if (effectStart == null)
                    {
                        effectStart = def.building.subcoreScannerStartEffect.Spawn();
                        effectStart.Trigger(this, new TargetInfo(InteractionCell, base.Map));
                    }
                    effectStart.EffectTick(this, new TargetInfo(InteractionCell, base.Map));
                }
            }
            else
            {
                effectStart?.Cleanup();
                effectStart = null;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!initScanner)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "SubcoreScannerStart".Translate();
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("SubcoreScannerProduces".Translate() + " " + def.building.subcoreScannerOutputDef.label + ".");
                stringBuilder.Append("\n\n");
                stringBuilder.Append("DurationHours".Translate() + ": " + def.building.subcoreScannerTicks.ToStringTicksToPeriod());
                stringBuilder.Append("\n\n");
                string text = def.building.subcoreScannerFixedIngredients.Select((IngredientCount i) => i.Summary).ToCommaList(useAnd: true);
                stringBuilder.Append("SubcoreScannerStartDesc".Translate(def.label, text));
                command_Action.defaultDesc = stringBuilder.ToString();
                command_Action.icon = InitScannerIcon.Texture;
                command_Action.action = delegate
                {
                    initScanner = true;
                };
                command_Action.activateSound = SoundDefOf.Tick_Tiny;
                yield return command_Action;
            }
            else if (base.SelectedPawn == null)
            {
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "InsertPerson".Translate() + "...";
                command_Action2.defaultDesc = "InsertPersonSubcoreScannerDesc".Translate(def.label);
                command_Action2.icon = InsertPersonIcon.Texture;
                command_Action2.action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    IReadOnlyList<Pawn> allPawnsSpawned = base.Map.mapPawns.AllPawnsSpawned;
                    for (int j = 0; j < allPawnsSpawned.Count; j++)
                    {
                        Pawn pawn = allPawnsSpawned[j];
                        AcceptanceReport acceptanceReport = CanAcceptPawn(pawn);
                        if (!acceptanceReport.Accepted)
                        {
                            if (!acceptanceReport.Reason.NullOrEmpty())
                            {
                                list.Add(new FloatMenuOption(pawn.LabelShortCap + ": " + acceptanceReport.Reason, null, pawn, Color.white));
                            }
                        }
                        else
                        {
                            list.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                            {
                                if (def.building.destroyBrain)
                                {
                                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRipscanPawn".Translate(pawn.Named("PAWN")), delegate
                                    {
                                        SelectPawn(pawn);
                                    }, destructive: true));
                                }
                                else
                                {
                                    SelectPawn(pawn);
                                }
                            }, pawn, Color.white));
                        }
                    }
                    if (!list.Any())
                    {
                        list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                };
                if (!PowerOn)
                {
                    command_Action2.Disable("NoPower".Translate().CapitalizeFirst());
                }
                else if (State == SubcoreScannerState.WaitingForIngredients)
                {
                    StringBuilder stringBuilder2 = new StringBuilder("SubcoreScannerWaitingForIngredientsDesc".Translate().CapitalizeFirst() + ":\n");
                    AppendIngredientsList(stringBuilder2);
                    command_Action2.Disable(stringBuilder2.ToString());
                }
                yield return command_Action2;
            }
            if (initScanner)
            {
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = ((State == SubcoreScannerState.Occupied) ? "CommandCancelSubcoreScan".Translate() : "CommandCancelLoad".Translate());
                command_Action3.defaultDesc = ((State == SubcoreScannerState.Occupied) ? "CommandCancelSubcoreScanDesc".Translate() : "CommandCancelLoadDesc".Translate());
                command_Action3.icon = CancelLoadingIcon;
                command_Action3.action = delegate
                {
                    if (State == SubcoreScannerState.Occupied && DestroyOccupantBrain)
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmCancelRipscan".Translate(Occupant.Named("PAWN")), EjectContents, destructive: true));
                    }
                    else
                    {
                        EjectContents();
                    }
                };
                command_Action3.activateSound = SoundDefOf.Designate_Cancel;
                yield return command_Action3;
            }
            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }
            if (State == SubcoreScannerState.Occupied)
            {
                Command_Action command_Action4 = new Command_Action();
                command_Action4.defaultLabel = "DEV: Complete";
                command_Action4.action = delegate
                {
                    fabricationTicksLeft = 0;
                };
                yield return command_Action4;
            }
            Command_Action command_Action5 = new Command_Action();
            command_Action5.defaultLabel = (debugDisableNeedForIngredients ? "DEV: Enable Ingredients" : "DEV: Disable Ingredients");
            command_Action5.action = delegate
            {
                debugDisableNeedForIngredients = !debugDisableNeedForIngredients;
            };
            yield return command_Action5;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initScanner, "initScanner", defaultValue: false);
            Scribe_Values.Look(ref fabricationTicksLeft, "fabricationTicksLeft", 0);
        }
        
    }
}