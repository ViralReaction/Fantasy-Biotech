using PipeSystem;
using RimWorld;
using static Verse.GenDraw;
using UnityEngine;
using Verse;
using System.Text;


namespace FantasyBiotech
{
    public class CompResourceStorage_SteamTank : CompResourceStorage
    {
        public new CompProperties_SteamTank Props => (CompProperties_SteamTank)props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            if (!respawningAfterLoad)
                RemovePipes();

            PipeNetManager = parent.Map.GetComponent<PipeNetManager>();
            if (TransmitResourceNow)
            {
                PipeNetManager.RegisterConnector(this);
                PipeSystemDebug.Message($"Registering {this}");
            }

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                StartSustainer();
            });

            graphicLinkedOverlay = LinkedPipes.GetOverlayFor(Props.pipeNet);
            powerComp = parent.TryGetComp<CompPowerTrader>();
            CachedCompResourceStorage.Cache(this);
            isBreakdownable = parent.TryGetComp<CompBreakdownable>() != null;
            ContentCanRot = Props.contentRequirePower && powerComp != null;

            pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
            // Fillable bar request
            if (parent.Rotation.IsHorizontal)
            {
                request = new FillableBarRequest
                {

                    center = parent.DrawPos + Props.horizontalCenterOffset + Vector3.up * 0.1f,
                    size = Props.horizontalBarSize,
                    fillPercent = AmountStoredPct,
                    filledMat = MaterialCreator.materials.TryGetValue(Props.pipeNet, MaterialCreator.BarFallbackMat),
                    unfilledMat = MaterialCreator.BarUnfilledMat,
                    margin = Props.margin,
                };
            }
            else
            {
                request = new FillableBarRequest
                {

                    center = parent.DrawPos + Props.centerOffset + Vector3.up * 0.1f,
                    size = Props.barSize,
                    fillPercent = AmountStoredPct,
                    filledMat = MaterialCreator.materials.TryGetValue(Props.pipeNet, MaterialCreator.BarFallbackMat),
                    unfilledMat = MaterialCreator.BarUnfilledMat,
                    margin = Props.margin,
                    rotation = parent.Rotation.Rotated(RotationDirection.Clockwise)
                };
            }
            // Extract gizmo
            if (Props.extractOptions != null)
            {
                extractResourceAmount = Props.extractOptions.ratio * Props.extractOptions.extractAmount;

                extractGizmo = new Command_Action()
                {
                    action = delegate
                    {
                        markedForExtract = !markedForExtract;
                        UpdateDesignation(parent);
                    },
                    defaultLabel = Props.extractOptions.labelKey.Translate(),
                    defaultDesc = Props.extractOptions.descKey.Translate(),
                    icon = Props.extractOptions.tex
                };
            }
            // Refill gizmo
            if (Props.refillOptions != null && !Props.refillOptions.alwaysRefill)
            {
                refillGizmo = new Command_Toggle()
                {
                    isActive = () => markedForRefill,
                    toggleAction = delegate
                    {
                        markedForRefill = !markedForRefill;
                        PipeNetManager.UpdateRefillableWith(parent);
                    },
                    defaultLabel = "PipeSystem_AllowManualRefill".Translate(),
                    defaultDesc = "PipeSystem_AllowManualRefillDesc".Translate(),
                    icon = TexCommand.ForbidOff
                };
                // Loading save, marked for refill: update refillables
                if (markedForRefill) PipeNetManager.UpdateRefillableWith(parent);
            }
            // Always refill: update refillables
            else if (Props.refillOptions != null && Props.refillOptions.alwaysRefill)
            {
                PipeNetManager.UpdateRefillableWith(parent);
            }
            // Transfer gizmo
            if (Props.addTransferGizmo)
            {
                transferGizmo = new Command_Action()
                {
                    action = delegate
                    {
                        markedForTransfer = !markedForTransfer;
                        if (markedForTransfer)
                        {
                            PipeNet.markedForTransfer.Add(this);
                            PipeNet.storages.Remove(this);
                        }
                        else
                        {
                            PipeNet.markedForTransfer.Remove(this);
                            PipeNet.storages.Add(this);
                        }
                        pipeNetOverlayDrawer?.ToggleStatic(parent, MaterialCreator.transferMat, markedForTransfer);
                    },
                    defaultLabel = "PipeSystem_TransferContent".Translate(),
                    defaultDesc = "PipeSystem_TransferContentDesc".Translate(),
                    icon = transferIcon
                };
                pipeNetOverlayDrawer?.ToggleStatic(parent, MaterialCreator.transferMat, markedForTransfer);
            }
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (Props.drawStorageBar)
            {
                request.fillPercent = AmountStoredPct;
                DrawFillableBar(request);
            }
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (Props.addStorageInfo)
            {
                // Convert SPU-ticks to SPU-days
                float storedSPUd = amountStored / GenDate.TicksPerDay;
                float maxSPUd = Props.storageCapacity / GenDate.TicksPerDay;
                sb.AppendInNewLine("PowerBatteryStored".Translate() + ": " + storedSPUd.ToString("F1") + " / " + maxSPUd.ToString("F0") + " SPUd");
            }
            if (markedForTransfer)
            {
                sb.AppendInNewLine("PipeSystem_MarkedToTransferContent".Translate());
            }
            var net = PipeNet;
            sb.AppendInNewLine("FantasyBiotech_SteamNetExcess".Translate((net.Production - net.Consumption).ToString("F0"), (net.Stored / GenDate.TicksPerDay).ToString("F1")));
            if (DebugSettings.godMode)
            {
                sb.AppendInNewLine(net.ToString());
            }
            return sb.ToString().TrimEndNewlines();
        }


    }
}
