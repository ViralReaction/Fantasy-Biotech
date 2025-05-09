using System.Linq;
using PipeSystem;

namespace FantasyBiotech
{
    public class CompPowerPlantNeedResource_Steam : CompPowerPlantNeedResource
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.compResourceTraders = this.parent.GetComps<CompResourceTrader>().ToList<CompResourceTrader>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override float DesiredPowerOutput
        {
            get
            {
                for (int index = 0; index < compResourceTraders.Count; ++index)
                {
                    if (!this.compResourceTraders[index].ResourceOn)
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
