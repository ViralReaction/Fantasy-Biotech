using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace FantasyBiotech
{
	public class QuestNode_SpawnConstructCluster : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		[NoTranslate]
		public SlateRef<string> tag;

		public SlateRef<float?> points;

		public SlateRef<IntVec3?> dropSpot;

		public override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_ConstructCluster questPart_ConstructCluster = new QuestPart_ConstructCluster
			{
				inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"),
				tag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate)),
				mapParent = slate.Get<Map>("map").Parent,
				sketch = GenerateSketch(slate),
				dropSpot = dropSpot.GetValue(slate) ?? IntVec3.Invalid
			};
			QuestGen.quest.AddPart(questPart_ConstructCluster);
			string text = "";
			if (questPart_ConstructCluster.sketch.pawns != null)
			{
				text += PawnUtility.PawnKindsToLineList(questPart_ConstructCluster.sketch.pawns.Select((m) => m.kindDef), "  - ", ColoredText.ThreatColor);
			}
			string[] array = (from t in questPart_ConstructCluster.sketch.buildingsSketch.Things
			                  where GenHostility.IsDefMechClusterThreat(t.def)
			                  group t by t.def.label).Select(delegate(IGrouping<string, SketchThing> grp)
			{
				int num = grp.Count();
				return num + " " + ((num > 1) ? Find.ActiveLanguageWorker.Pluralize(grp.Key, num) : grp.Key);
			}).ToArray();
			if (array.Any())
			{
				if (text != "")
				{
					text += "\n";
				}
				text += array.ToLineList(ColoredText.ThreatColor, "  - ");
			}
			if (text != "")
			{
				QuestGen.AddQuestDescriptionRules(new List<Rule>
				{
					new Rule_String("allThreats", text)
				});
			}
		}

		private MechClusterSketch GenerateSketch(Slate slate)
		{
			return ConstructClusterGenerator.GenerateClusterSketch(points.GetValue(slate) ?? slate.Get("points", 0f), slate.Get<Map>("map"));
		}

		public override bool TestRunInt(Slate slate)
		{
			if (!Find.Storyteller.difficulty.allowViolentQuests)
			{
				return false;
			}
			if (MechUtility.ConstructFaction() == null)
			{
				return false;
			}
			return slate.Get<Map>("map") != null;
		}
	}

}