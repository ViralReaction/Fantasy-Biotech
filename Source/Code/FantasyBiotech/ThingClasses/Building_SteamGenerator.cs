using Verse;
using PipeSystem;
using System.Collections.Generic;
using RimWorld;

namespace FantasyBiotech
{
    public class Building_SteamGenerator : Building
    {

        private List<CompResourceTrader> _traders = [];
        private CompRefuelable _compRefuelable;
        private int _tradersCount;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            IEnumerable<CompResourceTrader> compResourceTrader = this.GetComps<CompResourceTrader>();
            this._traders = new List<CompResourceTrader>(compResourceTrader);
            this._tradersCount = this._traders.Count;
            this._compRefuelable = this.GetComp<CompRefuelable>();
        }

        public override void Tick()
        {
            base.Tick();
            if (_compRefuelable == null) return;
            if (_compRefuelable.HasFuel)
            {
                for (int i = 0; i < this._tradersCount; i++)
                {
                    this.BroadcastCompSignal(_traders[i].OnSignal);
                }

            }
            else
            {
                for (int i = 0; i < this._tradersCount; i++)
                {
                    this.BroadcastCompSignal(_traders[i].OffSignal);
                }
            }

        }
    }
}
