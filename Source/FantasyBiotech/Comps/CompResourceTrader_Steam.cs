using PipeSystem;
using RimWorld;
using System.Text;
using Verse;

namespace FantasyBiotech
{
    public class CompResourceTrader_Steam : CompResourceTrader
    {
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Consumption >= 0f)
            {
                sb.AppendFormat("{0} {1:##0} {2}", "PipeSystem_ResourceNeeded".Translate(this.Resource.name), (this.Consumption), this.Resource.unit);
            }
            else
            {
                sb.AppendFormat("{0} {1:##0} {2}", "PipeSystem_ResourceOutput".Translate(this.Resource.name), this.ResourceOn ? -(this.Consumption) : 0.0, this.Resource.unit);
            }
            if (PipeNet == null)
            {
                sb.Append("PipeSystem_NotConnected".Translate(this.Resource.name));
                return sb.ToString().Trim();
            }
            if (!ResourceOn)
            {
                sb.AppendInNewLine("FantasyBiotech_PipeSystem_NotConnected".Translate());
                return sb.ToString().Trim();
            }
            var net = PipeNet;
            sb.AppendInNewLine("FantasyBiotech_SteamNetExcess".Translate((net.Production - net.Consumption).ToString("F0"), (net.Stored / GenDate.TicksPerDay).ToString("F1")));
            if (DebugSettings.ShowDevGizmos)
                sb.AppendInNewLine(this.DebugString);
            return sb.ToString().Trim();
        }
    }
}