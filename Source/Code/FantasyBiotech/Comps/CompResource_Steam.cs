using System.Text;
using PipeSystem;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class CompResource_Steam : CompResource
    {

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (PipeNet == null)
            {
                sb.Append("PipeSystem_NotConnected".Translate(Resource.name));
                return sb.ToString().Trim();
            }

            Resource res = Resource;
            PipeNet net = PipeNet;
            if (res.onlyShowStored)
            {
                sb.Append($"{"PipeSystem_Stored".Translate(res.name)} {net.Stored:##0} {res.unit}");
            }
            else
            {
                sb.Append("FantasyBiotech_SteamNetExcess".Translate((net.Production - net.Consumption).ToString("F0"), (net.Stored * 100 / GenDate.TicksPerDay).ToString("F1")));
            }
            if (DebugSettings.godMode)
            {
                sb.AppendInNewLine(net.ToString());
            }

            return sb.ToString().Trim();
        }
    }
}
