using System.Collections.Generic;
using RimWorld;
using Verse.AI;
using Verse;

namespace FantasyBiotech
{
    [StaticConstructorOnStartup]
    public static class MechUtility
    {

        public static Building_MechCharger GetClosestCharger(Pawn carrier, Pawn mech, bool forced)
        {
            Danger danger = forced ? Danger.Deadly : Danger.Some;
            Building_MechCharger closestCharger = null;
            float closestDist = float.MaxValue;

            List<Building_MechCharger_Steam> potentialChargers = mech.Map.GetComponent<RechargerMapComponent>()?.allChargers;
            if (potentialChargers == null || potentialChargers.Count == 0) return null;

            ReservationManager reservationManager = mech.Map.reservationManager;

            for (int i = 0; i < potentialChargers.Count; i++)
            {
                Building_MechCharger_Steam charger = potentialChargers[i];
                if (!carrier.CanReach(charger, PathEndMode.InteractionCell, danger)) continue;
                bool isReserved = reservationManager.ReservedBy(charger, carrier);
                if ((!forced && isReserved) || (forced && KeyBindingDefOf.QueueOrder.IsDownEvent && isReserved)) continue;
                if (charger.IsForbidden(carrier) || !carrier.CanReserve(charger, 1, -1, null, forced)) continue;
                if (!charger.CanPawnChargeCurrently(mech)) continue;
                float dist = (charger.Position - mech.Position).LengthHorizontalSquared;
                if (!(dist < closestDist)) continue;
                closestDist = dist;
                closestCharger = charger;
            }

            return closestCharger;
        }
        
        public static bool ConstructSuitableForCluster(PawnKindDef def)
        {
            if (!def.RaceProps.IsMechanoid || def.isGoodBreacher || !def.isFighter || !def.allowInMechClusters) return false;
            if (ModsConfig.BiotechActive && Find.BossgroupManager.ReservedByBossgroup(def)) return false;
            return def.GetModExtension<ConstructExtension>()?.isConstruct != false;
        }
        
        private static Faction _cachedConstructFaction;
        public static Faction ConstructFaction()
        {
            if (_cachedConstructFaction != null) return _cachedConstructFaction;
            for (int i = 0; i < Find.FactionManager.AllFactionsListForReading.Count; i++)
            {
                Faction findFaction = Find.FactionManager.AllFactionsListForReading[i];
                if (findFaction.def != FantasyBiotechDefOf.VR_Construct) continue;
                _cachedConstructFaction = findFaction;
                return _cachedConstructFaction;
            }

            return Faction.OfMechanoids;
        }

        public static void ClearFactionCache()
        {
            _cachedConstructFaction = null;
        }

        public static bool IsArtificer(Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(FantasyBiotechDefOf.VR_ArtificerImplant);
        }

    }
}
