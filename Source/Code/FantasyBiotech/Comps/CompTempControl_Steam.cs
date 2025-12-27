using System.Text;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class CompTempControl_Steam : CompTempControl
    {
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append((Props.inspectString ?? ((string)"TargetTemperature".Translate())) + ": ");
            stringBuilder.Append(TargetTemperature.ToStringTemperature("F0"));
            return stringBuilder.ToString();
        }
    }
}
