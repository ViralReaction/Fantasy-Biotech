using System.Collections.Generic;
using Verse;

namespace FantasyBiotech
{
    public static class RadialUtility
    {
        public static IEnumerable<Pawn> GetPawnsInRadius(IntVec3 center, Map map, float radius)
        {
            if (map == null) yield break;
            float checkRadius = radius * radius;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.Position.DistanceToSquared(center) <= checkRadius) yield return pawn;
            }
        }

    }
}
