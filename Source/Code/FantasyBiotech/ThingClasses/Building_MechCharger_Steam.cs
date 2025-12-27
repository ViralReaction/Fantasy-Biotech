using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using PipeSystem;
using System.Text;
using Verse.AI.Group;

namespace FantasyBiotech
{
    [StaticConstructorOnStartup]
    public class Building_MechCharger_Steam : Building_MechCharger
    {
        // 12 hours
        private const float ChargeRate = 0.0033333333f;
        private new CompResourceTrader Power => this.TryGetComp<CompResourceTrader>();

        private new bool IsPowered => Power.ResourceOn;

        private new CompWasteProducer WasteProducer => null;

        private RechargerMapComponent chargerMapComp;
        private List<CompResourceTrader> traders = [];

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            chargerMapComp = this.Map.GetComponent<RechargerMapComponent>();
            chargerMapComp.RegisterCharger(this);
            IEnumerable<CompResourceTrader> comps = this.GetComps<CompResourceTrader>();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            chargerMapComp.RemoveCharger(this);
            base.DeSpawn(mode);
        }

        public override void Tick()
        {
            if (this.comps != null)
            {
                for (int i = 0, count = this.comps.Count; i < count; i++)
                {
                    this.comps[i].CompTick();
                }
            }
            bool hasChargingMech = currentlyChargingMech != null;
            bool powerOn = Power.ResourceOn;

            if (hasChargingMech && (currentlyChargingMech.CurJobDef != JobDefOf.MechCharge || currentlyChargingMech.CurJob.targetA.Thing != this))
            {
                Log.Warning("Mech did not clean up its charging job properly");
                StopCharging();
            }
            if (hasChargingMech && powerOn)
            {
                Power.Notify_UsedThisTick();
                currentlyChargingMech.needs.energy.CurLevel += ChargeRate;

                if (moteCablePulse == null || moteCablePulse.Destroyed)
                {
                    moteCablePulse = MoteMaker.MakeInteractionOverlay(ThingDefOf.Mote_ChargingCablesPulse, this, new TargetInfo(InteractionCell, Map));
                }
                moteCablePulse?.Maintain();

                if (IsAttachedToMech)
                {
                    sustainerCharging ??= SoundDefOf.MechChargerCharging.TrySpawnSustainer(SoundInfo.InMap(this));
                    sustainerCharging.Maintain();

                    if (moteCharging == null || moteCharging.Destroyed)
                    {
                        moteCharging = MoteMaker.MakeAttachedOverlay(currentlyChargingMech, ThingDefOf.Mote_MechCharging, Vector3.zero);
                    }
                    moteCharging?.Maintain();
                }
            }
            else
            {
                Power.UsedLastTick = false;
                if (sustainerCharging != null)
                {
                    sustainerCharging.End();
                    sustainerCharging = null;
                }
            }
            if (wireExtensionTicks < 70)
            {
                wireExtensionTicks++;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.comps != null)
            {
                for (int i = 0; i < this.comps.Count; i++)
                {
                    foreach (Gizmo gizmo2 in this.comps[i].CompGetGizmosExtra())
                    {
                        yield return gizmo2;
                    }
                }
            }
            if (((this.def.BuildableByPlayer && this.def.passability != Traversability.Impassable && !this.def.IsDoor) || this.def.building.forceShowRoomStats) && Gizmo_RoomStats.GetRoomToShowStatsFor(this) != null && Find.Selector.SingleSelectedObject == this)
            {
                yield return new Gizmo_RoomStats(this);
            }
            Gizmo selectMonumentMarkerGizmo = QuestUtility.GetSelectMonumentMarkerGizmo(this);
            if (selectMonumentMarkerGizmo != null)
            {
                yield return selectMonumentMarkerGizmo;
            }
            if (this.def.Minifiable && Faction == Faction.OfPlayer)
            {
                yield return InstallationDesignatorDatabase.DesignatorFor(this.def);
            }
            ColorInt? glowerColorOverride = null;
            CompGlower comp;
            if ((comp = GetComp<CompGlower>()) != null && comp.HasGlowColorOverride)
            {
                glowerColorOverride = new ColorInt?(comp.GlowColor);
            }
            if (!this.def.building.neverBuildable)
            {
                Command command = BuildCopyCommandUtility.BuildCopyCommand(this.def, Stuff, StyleSourcePrecept as Precept_Building, this.StyleDef, true, glowerColorOverride);
                if (command != null)
                {
                    yield return command;
                }
            }
            if (Faction == Faction.OfPlayer || this.def.building.alwaysShowRelatedBuildCommands)
            {
                foreach (Command command2 in BuildRelatedCommandUtility.RelatedBuildCommands(this.def))
                {
                    yield return command2;
                }
            }
            Lord lord;
            if ((lord = this.GetLord()) != null && lord.CurLordToil != null)
            {
                foreach (Gizmo gizmo2 in lord.CurLordToil.GetBuildingGizmos(this))
                {
                    yield return gizmo2;
                }
            }
            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }
            if (currentlyChargingMech != null)
            {
                Command_Action commandAction5 = new Command_Action
                {
                    action = delegate
                    {
                        currentlyChargingMech.needs.TryGetNeed<Need_MechEnergy>().CurLevelPercentage = 1f;
                    },
                    defaultLabel = "DEV: Charge 100%"
                };
                yield return commandAction5;
            }
        }
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (this.def.drawerType == DrawerType.RealtimeOnly || !this.Spawned)
            {
                this.Graphic.Draw(drawLoc, flip ? this.Rotation.Opposite : this.Rotation, this);
            }
            SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);
            this.Comps_PostDraw();
            GenDraw.FillableBarRequest barDrawData = BarDrawData;
            GenDraw.DrawFillableBar(barDrawData);
            Vector3 a = drawLoc;
            float num = EaseInOutQuart(wireExtensionTicks / 70f);
            if (currentlyChargingMech == null)
            {
                num = 1f - num;
            }
            num = Mathf.Max(num, 0.32f);
            Vector3 b = Vector3.Lerp(drawLoc, InteractionCell.ToVector3Shifted(), num);
            b.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
            a.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
            GenDraw.DrawLineBetween(a, b, WireMaterial, 1f);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.currentlyChargingMech != null)
            {
                stringBuilder.Append("FantasyBiotech_SteamCharger_CurrentlyCharging".Translate(this.currentlyChargingMech.Label));
            }
            string text = this.InspectStringPartsFromComps();
            if (text.NullOrEmpty()) return stringBuilder.ToString();
            if (stringBuilder.Length > 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(text);
            return stringBuilder.ToString();
        }

        // public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        // {
        //     List<PawnKindDef> source = [];
        //     foreach (PawnKindDef pk in DefDatabase<PawnKindDef>.AllDefs)
        //     {
        //         if (IsCompatibleWithCharger(pk))
        //         {
        //             source.Add(pk);
        //         }
        //     }
        //     StringBuilder sb = new StringBuilder();
        //     foreach (PawnKindDef pk in source)
        //     {
        //         sb.AppendLine(" - " + pk.LabelCap.Resolve());
        //     }
        //     string text = sb.ToString();
        //     List<string> weightClassNames = [];
        //     for (int i = 0; i < def.building.requiredMechWeightClasses.Count; i++)
        //     {
        //         MechWeightClassDef w = def.building.requiredMechWeightClasses[i];
        //         weightClassNames.Add(w.ToString());
        //     }
        //
        //     string weightClassList = weightClassNames.ToCommaList().CapitalizeFirst();
        //
        //     List<Dialog_InfoCard.Hyperlink> hyperlinks = [];
        //     for (int i = 0; i < source.Count; i++)
        //     {
        //         PawnKindDef pk = source[i];
        //         hyperlinks.Add(new Dialog_InfoCard.Hyperlink(pk.race));
        //     }
        //
        //     yield return new StatDrawEntry(
        //         StatCategoryDefOf.Basics,
        //         "StatsReport_RechargerWeightClass".Translate(),
        //         weightClassList,
        //         "StatsReport_RechargerWeightClass_Desc".Translate() + ": " + "\n\n" + text,
        //         99999,
        //         null,
        //         hyperlinks
        //     );
        // }

        public bool CanPawnChargeCurrentlySteam(Pawn pawn)
        {
            if (pawn == null || !IsCompatibleWithCharger(pawn.kindDef) || !IsPowered)
            {
                return false;
            }
            if (currentlyChargingMech == null)
            {
                return true;
            }
            return currentlyChargingMech == pawn;

        }

    }
}