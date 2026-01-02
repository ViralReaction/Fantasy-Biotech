using System.Linq;
using PipeSystem;

namespace FantasyBiotech
{
    public class CompPowerPlantNeedResource_Steam : CompPowerPlantNeedResource
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compResourceTraders = parent.GetComps<CompResourceTrader>().ToList();
            base.PostSpawnSetup(respawningAfterLoad);
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
