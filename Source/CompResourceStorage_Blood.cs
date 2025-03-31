using PipeSystem;
using UnityEngine;
using Verse;
using System.Reflection;



namespace FantasyBiotech
{
    public class CompResourceStorage_Blood : CompResourceStorage
    {
        public Graphic midGraphic;
        public Graphic fullGraphic;

        private enum FillLevelStage { Low, Mid, Full }
        private FillLevelStage currentStage;
        public override void CompTick()
        {
            base.CompTick();

            var newStage = GetCurrentStage();
            if (newStage != currentStage)
            {
                currentStage = newStage;
                typeof(Thing).GetField("graphicInt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(parent, CurrentGraphic);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, 1);
            }
        }

        private FillLevelStage GetCurrentStage()
        {
            if (AmountStoredPct < 0.25f) return FillLevelStage.Low;
            if (AmountStoredPct < 0.75f) return FillLevelStage.Mid;
            return FillLevelStage.Full;
        }

        public Graphic CurrentGraphic
        {
            get
            {
                return currentStage switch
                {
                    FillLevelStage.Low => midGraphic,
                    FillLevelStage.Mid => midGraphic,
                    FillLevelStage.Full => fullGraphic,
                    _ => null,
                };
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Color color = parent.Stuff?.stuffProps.color ?? Color.white;
            var drawSize = parent.def.graphicData.drawSize;
            var shader = parent.def.graphicData.shaderType?.Shader ?? ShaderDatabase.Cutout;
            midGraphic = GraphicDatabase.Get<Graphic_Single>(
            "Things/Building/Misc/VRE_HemogenVat/VRE_HemogenVat_Low",
            shader,
            drawSize,
            color
            );

            fullGraphic = GraphicDatabase.Get<Graphic_Single>(
            "Things/Building/Misc/VRE_HemogenVat/VRE_HemogenVat_Full",
            shader,
            drawSize,
            color
        );
        }

    }
}
