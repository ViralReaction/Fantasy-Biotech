using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class CompPowerPlantNeedResource_Steam : CompPowerPlantNeedResource
    {
        // To fix map swapping need to null out from CompPowerTrader
        public override void PostSwapMap()
        {
        }

        public override float DesiredPowerOutput
        {
            get
            {
                for (int index = 0; index < compResourceTraders.Count; ++index)
                {
                    if (!compResourceTraders[index].ResourceOn)
                        return 0.0f;
                }
                return base.DesiredPowerOutput;
            }
        }

        public override string CompInspectStringExtra()
        {
            return null;
        }
    }
}
