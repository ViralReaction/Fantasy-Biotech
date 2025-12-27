using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class ComplexThreatWorker_SleepingCosntructs : ComplexThreatWorker_SleepingThreat
    {
        public override bool CanResolveInt(ComplexResolveParams parms)
        {
            if (base.CanResolveInt(parms))
            {
                if (parms.hostileFaction != null)
                {
                    return parms.hostileFaction == MechUtility.ConstructFaction();
                }
                return true;
            }
            return false;
        }

        public override IEnumerable<PawnKindDef> GetPawnKindsForPoints(float points)
        {
            return PawnUtility.GetCombatPawnKindsForPoints(MechUtility.ConstructSuitableForComplex, points);
        }
    }
}
