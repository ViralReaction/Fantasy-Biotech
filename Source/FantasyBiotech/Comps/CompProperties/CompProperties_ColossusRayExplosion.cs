using RimWorld;
using Verse;


namespace FantasyBiotech
{
    public class CompProperties_ColossusRayExplosion : CompProperties
    {
        public int defaultDelayTicks = 60;
        public float radius = 2.9f;
        public int damageAmount = 30;
        public DamageDef damageDef;
        public float armorPenetration = 0.4f;

        public CompProperties_ColossusRayExplosion()
        {
            compClass = typeof(CompColossusRayExplosion);
        }
    }
}
