using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class CompSteamSpray : ThingComp
    {
        private IntermittentSteamSprayer steamSprayer;

        private CompProperties_SteamSpray Props => (CompProperties_SteamSpray)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            steamSprayer = new IntermittentSteamSprayer(parent);
        }

        public override void CompTick()
        {
            base.CompTick();
            steamSprayer.SteamSprayerTick();
        }
    }
}
