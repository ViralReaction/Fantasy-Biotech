using RimWorld;

namespace FantasyBiotech
{
    public class StorytellerCompProperties_SingleOnceFixed : StorytellerCompProperties
    {
        public IncidentDef incident;

        public int fireAfterDaysPassed;

        public bool skipIfOnExtremeBiome;

        public int minColonistCount;

        public bool skipIfAnyColonistIsArtificer;

        public StorytellerCompProperties_SingleOnceFixed()
        {
            compClass = typeof(StorytellerComp_SingleOnceFixed);
        }
    }
}
