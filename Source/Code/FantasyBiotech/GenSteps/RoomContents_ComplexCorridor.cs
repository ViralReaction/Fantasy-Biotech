using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FantasyBiotech
{
    public class RoomContents_ComplexCorridor : RoomContents_Corridor
    {
        private static readonly FloatRange ShelvesPer10EdgeCells = new FloatRange(0.35f, 0.35f);

        private static readonly IntRange ShelfGroupSizeRange = new IntRange(1, 2);

        public override IntRange ExteriorDoorCount => new IntRange(2, 3);



        public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
        {
            base.FillRoom(map, room, faction, threatPoints);
            SpawnShelves(map, room);
        }

        private static void SpawnShelves(Map map, LayoutRoom room)
        {
            float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
            int count = Mathf.Max(Mathf.RoundToInt(ShelvesPer10EdgeCells.RandomInRange * num), 1);
            ThingDef shelfThing = ThingDefOf.Shelf;
            ThingDef shelfStuff = ThingDefOf.WoodLog;
            foreach (LayoutRoomDef thing in room.defs)
            {
                Log.Message(thing);
                RoomContentExtension things = thing.GetModExtension<RoomContentExtension>();
                if (things != null)
                {
                    if (DefDatabase<ThingDef>.AllDefsListForReading.Contains(things.shelfThing))
                    {
                        shelfThing = things.shelfThing ?? ThingDefOf.Shelf;
                        shelfStuff = things.shelfStuff ?? ThingDefOf.WoodLog;
                        break;
                    }
                }
            }
            RoomGenUtility.FillAroundEdges(shelfThing, count, ShelfGroupSizeRange, room, map, null, null, 1, 0, shelfStuff);
        }
    }
}
