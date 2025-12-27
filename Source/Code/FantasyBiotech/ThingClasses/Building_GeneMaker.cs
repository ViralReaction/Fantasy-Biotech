using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FantasyBiotech
{
    [StaticConstructorOnStartup]
    public class Building_GraftForge : Building_GeneAssembler
    {

        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(250))
            {
                bool flag = lastWorkedTick + 250 + 2 >= Find.TickManager.TicksGame;
            }
            if (Working && this.IsHashIntervalTick(180))
            {
                CheckAllContainersValid();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {

            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "Recombine".Translate() + "...";
            command_Action.defaultDesc = "RecombineDesc".Translate();
            command_Action.icon = RecombineIcon.Texture;
            command_Action.action = delegate
            {
                Find.WindowStack.Add(new Dialog_CreateGenepack(this));
            };
            if (!def.IsResearchFinished)
            {
                command_Action.Disable("MissingRequiredResearch".Translate() + ": " + (from x in def.researchPrerequisites
                                                                                       where !x.IsFinished
                                                                                       select x.label).ToCommaList(useAnd: true).CapitalizeFirst());
            }
            else if (!GetGenepacks(includePowered: true, includeUnpowered: false).Any())
            {
                command_Action.Disable("CannotUseReason".Translate("NoGenepacksAvailable".Translate().CapitalizeFirst()));
            }
            yield return command_Action;
            if (Working)
            {
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "CancelXenogerm".Translate();
                command_Action2.defaultDesc = "CancelXenogermDesc".Translate();
                command_Action2.action = Reset;
                command_Action2.icon = CancelIcon;
                yield return command_Action2;
                if (DebugSettings.ShowDevGizmos)
                {
                    Command_Action command_Action3 = new Command_Action();
                    command_Action3.defaultLabel = "DEV: Finish xenogerm";
                    command_Action3.action = Finish;
                    yield return command_Action3;
                }
            }
            if (comps == null)
            {
                yield break;
            }
            for (int i = 0; i < comps.Count; i++)
            {
                foreach (Gizmo item in comps[i].CompGetGizmosExtra())
                {
                    yield return item;
                }
            }

            if (((def.BuildableByPlayer && def.passability != Traversability.Impassable && !def.IsDoor) || def.building.forceShowRoomStats) && Gizmo_RoomStats.GetRoomToShowStatsFor(this) != null && Find.Selector.SingleSelectedObject == this)
            {
                yield return new Gizmo_RoomStats(this);
            }
            Gizmo selectMonumentMarkerGizmo = QuestUtility.GetSelectMonumentMarkerGizmo(this);
            if (selectMonumentMarkerGizmo != null)
            {
                yield return selectMonumentMarkerGizmo;
            }
            if (def.Minifiable && (base.Faction == Faction.OfPlayer || def.building.alwaysUninstallable))
            {
                yield return InstallationDesignatorDatabase.DesignatorFor(def);
            }
            ColorInt? glowerColorOverride = null;
            CompGlower comp = GetComp<CompGlower>();
            if (comp != null && comp.HasGlowColorOverride)
            {
                glowerColorOverride = comp.GlowColor;
            }
            if (!def.building.neverBuildable)
            {
                Command command = BuildCopyCommandUtility.BuildCopyCommand(def, base.Stuff, base.StyleSourcePrecept as Precept_Building, StyleDef, styleOverridden: true, glowerColorOverride);
                if (command != null)
                {
                    yield return command;
                }
            }
            if (base.Faction == Faction.OfPlayer || def.building.alwaysShowRelatedBuildCommands)
            {
                foreach (Command item in BuildRelatedCommandUtility.RelatedBuildCommands(def))
                {
                    yield return item;
                }
            }
        }

    }
}
