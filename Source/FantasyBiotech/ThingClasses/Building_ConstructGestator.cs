using RimWorld;
using Verse.Sound;
using Verse;
using UnityEngine;
using PipeSystem;
using System.Collections.Generic;


namespace FantasyBiotech
{
    public class Building_ConstructGestator : Building_MechGestator
    {
        private new CompResourceTrader Power => this.TryGetComp<CompResourceTrader>();
        private new bool PoweredOn => Power.ResourceOn;
        public override void Tick()
        {
            innerContainer.ThingOwnerTick();
           

            if (activeBill == null || !PoweredOn)
            {
                Power.UsedLastTick = false;
                if (workingSound != null)
                {
                    workingSound.End();
                    workingSound = null;
                }
                return;
            }
            if (BoundPawnStateAllowsForming)
            {
                activeBill.BillTick();
                ThingDef thingDef = null;
                switch (ActiveMechBill.State)
                {
                    case FormingState.Forming:
                        Power.Notify_UsedThisTick();
                        thingDef = def.building.gestatorFormingMote.GetForRotation(Rotation);
                        break;

                    case FormingState.Preparing when ActiveMechBill.GestationCyclesCompleted > 0:
                        thingDef = def.building.gestatorCycleCompleteMote.GetForRotation(Rotation);
                        break;

                    case FormingState.Formed:
                        thingDef = def.building.gestatorFormedMote.GetForRotation(Rotation);
                        break;
                }

                if (thingDef != null)
                {
                    if (workingMote == null || workingMote.Destroyed || workingMote.def != thingDef)
                    {
                        workingMote = MoteMaker.MakeAttachedOverlay(this, thingDef, Vector3.zero);
                    }
                    workingMote.Maintain();
                }
            }
        }
        public override void Notify_FormingCompleted()
        {
            if (activeBill.CreateProducts() is not Pawn pawn) return;
            Messages.Message("GestationComplete".Translate() + ": " + pawn.kindDef.LabelCap, this, MessageTypeDefOf.PositiveEvent);
            innerContainer.ClearAndDestroyContents();
            innerContainer.TryAdd(pawn);
            SoundDefOf.MechGestatorBill_Completed.PlayOneShot(this);
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (comps == null)
            {
                yield break;
            }
            for (int i = 0; i < comps.Count; i++)
            {
                foreach (Gizmo item in comps[i].CompGetGizmosExtra())
                {
                    yield return item;
                }
            }
            ColorInt? glowerColorOverride = null;
            CompGlower comp;
            if ((comp = GetComp<CompGlower>()) != null && comp.HasGlowColorOverride)
            {
                glowerColorOverride = comp.GlowColor;
            }
            if (!def.building.neverBuildable)
            {
                var command = BuildCopyCommandUtility.BuildCopyCommand(def, Stuff, StyleSourcePrecept as Precept_Building, StyleDef, styleOverridden: true, glowerColorOverride);
                if (command != null)
                {
                    yield return command;
                }
            }
            if (Faction == Faction.OfPlayer || def.building.alwaysShowRelatedBuildCommands)
            {
                foreach (var item in BuildRelatedCommandUtility.RelatedBuildCommands(def))
                {
                    yield return item;
                }
            }
            var billAutonomous = ActiveBill;
            if (billAutonomous is { State: FormingState.Forming })
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ActiveBill.formingTicks -= ActiveBill.recipe.formingTicks * 0.25f;
                    },
                    defaultLabel = "DEV: Forming cycle +25%"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ActiveBill.formingTicks = 0f;
                    },
                    defaultLabel = "DEV: Complete cycle"
                };
            }

            if (!DebugSettings.ShowDevGizmos) yield break;
            if (ActiveMechBill == null || ActiveMechBill.State == 0 || ActiveMechBill.State == FormingState.Formed) yield break;
            var commandAction2 = new Command_Action
            {
                action = ActiveMechBill.ForceCompleteAllCycles,
                defaultLabel = "DEV: Complete all cycles"
            };
            yield return commandAction2;
        }
    }
}
