using System.Collections.Generic;
using Verse;

namespace FantasyBiotech
{
    public class RechargerMapComponent : MapComponent
    {

        public RechargerMapComponent(Map map) : base(map)
        {

        }

        public override void MapGenerated()
        {
            base.MapGenerated();
            GetChargerMap();
        }

        public void GetChargerMap()
        {
            foreach (var thing in this.map.listerBuildings.allBuildingsColonist)
            {
                if (thing is Building_MechCharger_Steam steamCharger)
                {
                    RegisterCharger(steamCharger);
                }
            }
        }

        public void RegisterCharger(Building_MechCharger_Steam thing)
        {
            allChargers.Add(thing);
        }

        public void RemoveCharger(Building_MechCharger_Steam thing)
        {
            allChargers.Remove(thing);
        }


        public List<Building_MechCharger_Steam> allChargers = [];
    }
}