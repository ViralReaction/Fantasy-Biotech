using System.Collections.Generic;
using Verse;

namespace FantasyBiotech
{
    public static class RadialUtility
    {
        public static IEnumerable<Pawn> GetPawnsInRadius(IntVec3 center, Map map, float radius)
        {
            if (map == null)
            {
                yield break;
            }
            var checkRadius = radius * radius;
            foreach (Thing thing in map.mapPawns.AllPawnsSpawned)
            {
                if (thing is Pawn pawn && pawn.Position.DistanceToSquared(center) <= checkRadius)
                {
                    yield return pawn;
                }
            }
        }

    }
}
