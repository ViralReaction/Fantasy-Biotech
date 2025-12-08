using RimWorld;

namespace FantasyBiotech
{
    public class CompProperties_AbilitySteamBurst : CompProperties_AbilityEffect
    {
        public float radius = 6f;
        public int damage = 10;
        public float armorPen = 0.5f;

        public CompProperties_AbilitySteamBurst()
        {
            compClass = typeof(CompAbilityEffect_SteamBurst);
        }
    }
}
