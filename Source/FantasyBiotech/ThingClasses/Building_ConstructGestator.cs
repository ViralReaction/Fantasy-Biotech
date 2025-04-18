using RimWorld;
using Verse.Sound;
using Verse;
using UnityEngine;
using PipeSystem;


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
                        Power.Notify_UsedThisTick();
                        thingDef = def.building.gestatorCycleCompleteMote.GetForRotation(Rotation);
                        break;

                    case FormingState.Formed:
                        Power.UsedLastTick = false;
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
    }
}
