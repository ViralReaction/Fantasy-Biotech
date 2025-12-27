using System.Collections.Generic;
using RimWorld;

namespace FantasyBiotech
{
    public class StorytellerCompProperties_ArtificerComplexQuest : StorytellerCompProperties
    {
        public IncidentDef incident;

        public int mtbDays = 60;

        public float minSpacingDays;

        public float existingArtificerOrImplantMTBFactor = 1f;

        public List<QuestScriptDef> blockedByQueuedOrActiveQuests;

        public StorytellerCompProperties_ArtificerComplexQuest()
        {
            compClass = typeof(StorytellerComp_ArtificerComplexQuest);
        }
    }
}
