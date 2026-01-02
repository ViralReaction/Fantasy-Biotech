using System.Linq;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
	public class IncidentWorker_TradeMessage : IncidentWorker
	{
		private const int MaxShips = 5;

		public override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			return ((Map)parms.target).passingShipManager.passingShips.Count < MaxShips;
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (parms.traderKind == null && !DefDatabase<TraderKindDef>.AllDefs.Where(( x) => CanSpawn(map, x)).TryRandomElementByWeight(( traderDef) => traderDef.CalculatedCommonality, out parms.traderKind))
			{
				return false;
			}
			TradeShip tradeShip = new TradeShip(parms.traderKind, GetFaction(parms.traderKind))
			{
				WasAnnounced = false
			};
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && (b.GetComp<CompPowerTrader>() == null || b.GetComp<CompPowerTrader>().PowerOn)))
			{
				SendStandardLetter(tradeShip.def.LabelCap, "FantasyBiotech_TraderArrival".Translate(tradeShip.name, tradeShip.def.label, (tradeShip.Faction == null) ? "FantasyBiotech_TraderArrivalNoFaction".Translate() : "FantasyBiotech_TraderArrivalFromFaction".Translate(tradeShip.Faction.Named("FACTION"))), LetterDefOf.PositiveEvent, parms, LookTargets.Invalid);
				tradeShip.WasAnnounced = true;
			}
			map.passingShipManager.AddShip(tradeShip);
			tradeShip.GenerateThings();
			return true;
		}

		private static Faction GetFaction(TraderKindDef trader)
		{
			if (trader.faction == null)
			{
				return null;
			}
			if (!Find.FactionManager.AllFactions.Where((Faction f) => f.def == trader.faction).TryRandomElement(out var result))
			{
				return null;
			}
			return result;
		}

		private static bool CanSpawn(Map map, TraderKindDef trader)
		{
			if (!trader.orbital )
			{
				return false;
			}
			if (trader.faction == null)
			{
				return true;
			}
			Faction faction = GetFaction(trader);
			if (faction == null)
			{
				return false;
			}
			for (int i = 0; i < map.mapPawns.FreeColonists.Count; i++)
			{
				Pawn freeColonist = map.mapPawns.FreeColonists[i];
				if (freeColonist.CanTradeWith(faction, trader).Accepted)
				{
					return true;
				}
			}
			return false;
		}
	}
}
