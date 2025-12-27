using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
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
            bool? extension = def.GetModExtension<ConstructExtension>()?.isConstruct;
            if (extension == null || extension == false)
            {
	            return false;
            }
            return true;
        }

        public static bool ConstructSuitableForComplex(PawnKindDef def)
        {
	        if (def.RaceProps.IsMechanoid && !def.isGoodBreacher && def.isFighter && def.allowInMechClusters)
	        {
		        return def.GetModExtension<ConstructExtension>()?.isConstruct is true;
	        }
	        return false;

        }

        private static List<ThingDef> GetBuildingDefsForCluster(float points, IntVec2 size, bool canBeDormant, float? totalPoints, bool forceNoConditionCauser)
	{
		List<ThingDef> list = [];
		List<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) => def.building?.buildingTags != null && def.building.buildingTags.Contains("ConstructClusterMember") && (!totalPoints.HasValue || (float)def.building.minMechClusterPoints <= totalPoints)).ToList();
		if (canBeDormant)
		{
			if (Rand.Chance(0.5f))
			{
				list.Add(ThingDefOf.ActivatorCountdown);
			}
			if (Rand.Chance(0.5f))
			{
				int num2 = GenMath.RoundRandom(MechClusterGenerator.ActivatorProximitysCountCurve.Evaluate(points));
				for (int j = 0; j < num2; j++)
				{
					list.Add(ThingDefOf.ActivatorProximity);
				}
			}
		}
		float pointsLeft = points;
		ThingDef thingDef = source.Where(( x) => x.building.buildingTags.Contains("ConstructClusterCombatThreat")).MinBy(( x) => x.building.combatPower);
		for (pointsLeft = Mathf.Max(pointsLeft, thingDef.building.combatPower); pointsLeft > 0f && source.Where(( x) => x.building.combatPower <= pointsLeft && x.building.buildingTags.Contains("ConstructClusterCombatThreat")).TryRandomElement(out ThingDef result4); pointsLeft -= result4.building.combatPower)
		{
			list.Add(result4);
		}
		return list;
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

        public static bool AnyArtificerInPlayerFaction()
        {
	        foreach (Pawn allMaps_FreeColonist in PawnsFinder.AllMaps_FreeColonists)
	        {
		        if (IsArtificer(allMaps_FreeColonist))
		        {
			        return true;
		        }
	        }
	        return false;
        }


        public static bool IsArtificer(Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(FantasyBiotechDefOf.VR_ArtificerImplant);
        }

        public static bool AnyArtificerImplantInMap()
        {
	        List<Map> maps = Find.Maps;
	        for (int i = 0; i < maps.Count; i++)
	        {
		        if (maps[i].listerThings.ThingsOfDef(FantasyBiotechThingDefOf.VR_ArtificerImplant).Count > 0)
		        {
			        return true;
		        }
	        }
	        return false;
        }


    }
}
