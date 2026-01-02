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
        private new CompResourceTrader_Steam Power => this.TryGetComp<CompResourceTrader_Steam>();
        private new bool PoweredOn => Power.ResourceOn;
        public override void Tick()
        {
            if (activeBill == null || !PoweredOn)
            {
                Power.UsedLastTick = false;
                if (workingSound == null) return;
                workingSound.End();
                workingSound = null;
                return;
            }
            if (!BoundPawnStateAllowsForming) return;
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

            if (thingDef == null) return;
            if (workingMote == null || workingMote.Destroyed || workingMote.def != thingDef)
            {
                workingMote = MoteMaker.MakeAttachedOverlay(this, thingDef, Vector3.zero);
            }
            workingMote.Maintain();
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
                Command command = BuildCopyCommandUtility.BuildCopyCommand(def, Stuff, StyleSourcePrecept as Precept_Building, StyleDef, styleOverridden: true, glowerColorOverride);
                if (command != null)
                {
                    yield return command;
                }
            }
            if (Faction == Faction.OfPlayer || def.building.alwaysShowRelatedBuildCommands)
            {
                foreach (Command item in BuildRelatedCommandUtility.RelatedBuildCommands(def))
                {
                    yield return item;
                }
            }
            if (!DebugSettings.ShowDevGizmos) yield break;
            Bill_Autonomous billAutonomous = ActiveBill;
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
            if (ActiveMechBill == null || ActiveMechBill.State == 0 || ActiveMechBill.State == FormingState.Formed) yield break;
            Command_Action commandAction2 = new Command_Action
            {
                action = ActiveMechBill.ForceCompleteAllCycles,
                defaultLabel = "DEV: Complete all cycles"
            };
            yield return commandAction2;
        }
    }
}
