﻿using PipeSystem;
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
            if (Consumption >= 0f)
            {
                sb.AppendFormat("{0} {1:##0} {2}", "PipeSystem_ResourceNeeded".Translate(Resource.name), (Consumption), Resource.unit);
            }
            else
            {
                sb.AppendFormat("{0} {1:##0} {2}", "PipeSystem_ResourceOutput".Translate(Resource.name), ResourceOn ? -(Consumption) : 0.0, Resource.unit);
            }
            if (PipeNet == null)
            {
                sb.Append("PipeSystem_NotConnected".Translate(Resource.name));
                return sb.ToString().Trim();
            }
            if (!ResourceOn)
            {
                sb.AppendInNewLine("FantasyBiotech_PipeSystem_NotConnected".Translate());
                return sb.ToString().Trim();
            }
            PipeNet net = PipeNet;
            sb.AppendInNewLine("FantasyBiotech_SteamNetExcess".Translate((net.Production - net.Consumption).ToString("F0"), (net.Stored * 100 / GenDate.TicksPerDay).ToString("F1")));
            if (DebugSettings.ShowDevGizmos)
                sb.AppendInNewLine(DebugString);
            return sb.ToString().Trim();
        }
    }
}