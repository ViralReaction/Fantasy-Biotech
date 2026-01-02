using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using PipeSystem;
using Verse.AI;

namespace FantasyBiotech
{
    public class Building_CommsConsole_Steam : Building_CommsConsole
    {
        private CompResourceTrader_Steam Power;
        private bool CanUseCommsNow_Steam => Power == null || Power.ResourceOn;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Power = this.TryGetComp<CompResourceTrader_Steam>();
            if (CanUseCommsNow_Steam)
            {
                LongEventHandler.ExecuteWhenFinished(AnnounceTradeMessages);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            FloatMenuOption failureReason = GetFailureReason_Steam(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
                yield break;
            }
            foreach (ICommunicable commTarget in GetCommTargets(myPawn))
            {
                FloatMenuOption floatMenuOption = commTarget.CommFloatMenuOption(this, myPawn);
                if (floatMenuOption != null)
                {
                    yield return floatMenuOption;
                }
            }
            foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption2;
            }
        }
        private FloatMenuOption GetFailureReason_Steam(Pawn myPawn)
        {
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            {
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            }
            if (Power is { ResourceOn: false })
            {
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, myPawn.Named("PAWN"))), null);
            }
            if (!GetCommTargets(myPawn).Any())
            {
                return new FloatMenuOption("CannotUseReason".Translate("NoCommsTarget".Translate()), null);
            }
            if (CanUseCommsNow_Steam) return null;
            Log.Error(myPawn?.ToString() + " could not use comm console for unknown reason.");
            return new FloatMenuOption("Cannot use now", null);
        }

        private void AnnounceTradeMessages()
        {
            List<PassingShip> ships = Map.passingShipManager.passingShips;
            for (int i = 0; i < ships.Count; i++)
            {
                if (ships[i] is not TradeShip { WasAnnounced: false } ship) continue;
                TaggedString baseLetterText = "FantasyBiotech_TraderArrival".Translate(ship.name, ship.def.label, (ship.Faction == null) ? "FantasyBiotech_TraderArrivalNoFaction".Translate() : "FantasyBiotech_TraderArrivalFromFaction".Translate(ship.Faction.Named("FACTION")));
                IncidentParms incidentParms = new IncidentParms
                {
                    target = Map,
                    traderKind = ship.TraderKind
                };
                IncidentWorker.SendIncidentLetter(ship.def.LabelCap, baseLetterText, LetterDefOf.PositiveEvent, incidentParms, LookTargets.Invalid, null);
                ship.WasAnnounced = true;
            }
        }
    }
}
