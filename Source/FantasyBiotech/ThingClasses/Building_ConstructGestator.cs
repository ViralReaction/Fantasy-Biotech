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
        public new CompResourceTrader Power => this.TryGetComp<CompResourceTrader>();
        public new bool PoweredOn => Power.ResourceOn;
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
            Pawn pawn = activeBill.CreateProducts() as Pawn;
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
                Command command = BuildCopyCommandUtility.BuildCopyCommand(def, base.Stuff, base.StyleSourcePrecept as Precept_Building, StyleDef, styleOverridden: true, glowerColorOverride);
                if (command != null)
                {
                    yield return command;
                }
            }
            if (base.Faction == Faction.OfPlayer || def.building.alwaysShowRelatedBuildCommands)
            {
                foreach (Command item in BuildRelatedCommandUtility.RelatedBuildCommands(def))
                {
                    yield return item;
                }
            }
            Bill_Autonomous bill_Autonomous = ActiveBill;
            if (bill_Autonomous != null && bill_Autonomous.State == FormingState.Forming)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ActiveBill.formingTicks -= (float)ActiveBill.recipe.formingTicks * 0.25f;
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
            if (DebugSettings.ShowDevGizmos)
            {
                if (ActiveMechBill != null && ActiveMechBill.State != 0 && ActiveMechBill.State != FormingState.Formed)
                {
                    Command_Action command_Action2 = new Command_Action();
                    command_Action2.action = ActiveMechBill.ForceCompleteAllCycles;
                    command_Action2.defaultLabel = "DEV: Complete all cycles";
                    yield return command_Action2;
                }
            }
        }
    }
}
