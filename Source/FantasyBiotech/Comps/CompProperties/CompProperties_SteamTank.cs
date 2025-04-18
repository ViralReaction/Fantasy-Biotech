using PipeSystem;
using UnityEngine;

namespace FantasyBiotech
{
    public class CompProperties_SteamTank : CompProperties_ResourceStorage
    {
        public CompProperties_SteamTank()
        {
            compClass = typeof(CompResourceStorage_SteamTank);
        }
        public Vector2 horizontalBarSize = new Vector2(1.3f, 0.4f);
        public Vector3 horizontalCenterOffset = new Vector3(0, 0);
    }
}
