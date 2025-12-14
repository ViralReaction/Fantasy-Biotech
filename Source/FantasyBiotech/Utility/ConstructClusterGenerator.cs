using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace FantasyBiotech
{
	public static class ConstructClusterGenerator
	{
		private const string MechClusterMemberTag = "ConstructClusterMember";

		public const string MechClusterMemberGoodTag = "MechClusterMemberGood";

		public const string MechClusterMemberLampTag = "MechClusterMemberLamp";

		public const string MechClusterActivatorTag = "MechClusterActivator";

		private const string MechClusterCombatThreatTag = "ConstructClusterCombatThreat";

		public const string MechClusterProblemCauserTag = "MechClusterProblemCauser";

		private const float MaxPoints = 10000f;

		public static readonly SimpleCurve PointsToPawnsChanceCurve = [new CurvePoint(400f, 0.75f)];

		public static readonly SimpleCurve PawnPointsRandomPercentOfTotalCurve =
		[
			new CurvePoint(0.2f, 0f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(0.8f, 0f)
		];

		private static readonly FloatRange SizeRandomFactorRange = new FloatRange(0.8f, 2f);

		private static readonly SimpleCurve PointsToSizeCurve =
		[
			new CurvePoint(400f, 7f),
			new CurvePoint(1000f, 10f),
			new CurvePoint(2000f, 20f),
			new CurvePoint(5000f, 25f)
		];

		private static readonly SimpleCurve WallsChanceCurve = new SimpleCurve
		{
			new CurvePoint(400f, 0.35f),
			new CurvePoint(1000f, 0.5f)
		};

		private static readonly SimpleCurve PointsToLiftCountCurve =
		[
			new CurvePoint(0f,    1f),
			new CurvePoint(2000f,  2f),
			new CurvePoint(4000f, 3f),
			new CurvePoint(5000f, 4f),
			new CurvePoint(8000f, 6f)
		];

		private const float ActivatorCountdownChance = 0.5f;

		private const float ActivatorProximityChance = 0.5f;

		private static readonly SimpleCurve ActivatorProximitysCountCurve = new SimpleCurve
		{
			new CurvePoint(600f, 1f),
			new CurvePoint(1800f, 2f),
			new CurvePoint(3000f, 3f),
			new CurvePoint(5000f, 4f)
		};

		private static readonly SimpleCurve LampBuildingMinCountCurve = new SimpleCurve
		{
			new CurvePoint(400f, 1f),
			new CurvePoint(1000f, 2f)
		};

		private static readonly SimpleCurve LampBuildingMaxCountCurve = new SimpleCurve
		{
			new CurvePoint(400f, 1f),
			new CurvePoint(1000f, 4f),
			new CurvePoint(2000f, 6f)
		};

		public static MechClusterSketch GenerateClusterSketch(float points, Map map, bool startDormant = true, bool forceNoConditionCauser = false)
		{
			if (!ModLister.CheckRoyalty("Mech cluster") || !ModsConfig.RoyaltyActive)
			{
				return new MechClusterSketch(new Sketch(), [], startDormant);
			}
			points = Mathf.Min(points, MaxPoints);

			float num = points;
			List<MechClusterSketch.Mech> list = null;
			List<PawnKindDef> source = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(MechUtility.ConstructSuitableForCluster).ToList();
			list = [];
			float a = Rand.ByCurve(PawnPointsRandomPercentOfTotalCurve) * num;
			float pawnPointsLeft;
			a = (pawnPointsLeft = Mathf.Max(a, source.Min((x) => x.combatPower)));
			while (pawnPointsLeft > 0f && source.Where((def) => def.combatPower <= pawnPointsLeft).TryRandomElement(out PawnKindDef result))
			{
				pawnPointsLeft -= result.combatPower;
				list.Add(new MechClusterSketch.Mech(result));
			}
			num -= a - pawnPointsLeft;

			Sketch buildingsSketch = SketchGen.Generate(FantasyBiotechDefOf.VR_ConstructCluster, new SketchResolveParams
			{
				points = num,
				totalPoints = points,
				mechClusterDormant = startDormant,
				sketch = new Sketch(),
				mechClusterForMap = map,
				forceNoConditionCauser = forceNoConditionCauser
			});
			if (list != null)
			{
				List<IntVec3> pawnUsedSpots = new List<IntVec3>();
				for (int i = 0; i < list.Count; i++)
				{
					MechClusterSketch.Mech pawn = list[i];
					if (!buildingsSketch.OccupiedRect.Where((c) => !buildingsSketch.ThingsAt(c).Any() && !pawnUsedSpots.Contains(c)).TryRandomElement(out IntVec3 result2))
					{
						CellRect cellRect = buildingsSketch.OccupiedRect;
						do
						{
							cellRect = cellRect.ExpandedBy(1);
						}
						while (!cellRect.Where((x) => !buildingsSketch.WouldCollide(pawn.kindDef.race, x, Rot4.North) && !pawnUsedSpots.Contains(x)).TryRandomElement(out result2));
					}
					pawnUsedSpots.Add(result2);
					pawn.position = result2;
					list[i] = pawn;
				}
			}
			return new MechClusterSketch(buildingsSketch, list, startDormant);
		}

		public static void ResolveSketch(SketchResolveParams parms)
		{
			if (!ModLister.CheckRoyalty("Mech cluster"))
			{
				return;
			}
			bool canBeDormant = !parms.mechClusterDormant.HasValue || parms.mechClusterDormant.Value;
			float num;
			if (parms.points.HasValue)
			{
				num = parms.points.Value;
			}
			else
			{
				num = 2000f;
				Log.Error("No points given for mech cluster generation. Default to " + num);
			}
			float value = parms.totalPoints ?? num;
			IntVec2 intVec;
			if (parms.mechClusterSize.HasValue)
			{
				intVec = parms.mechClusterSize.Value;
			}
			else
			{
				int num2 = GenMath.RoundRandom(PointsToSizeCurve.Evaluate(num) * SizeRandomFactorRange.RandomInRange);
				int num3 = GenMath.RoundRandom(PointsToSizeCurve.Evaluate(num) * SizeRandomFactorRange.RandomInRange);
				if (parms.mechClusterForMap != null)
				{
					CellRect cellRect = LargestAreaFinder.FindLargestRect(parms.mechClusterForMap, ( x) => !x.Impassable(parms.mechClusterForMap) && x.GetAffordances(parms.mechClusterForMap).Contains(TerrainAffordanceDefOf.Heavy), Mathf.Max(num2, num3));
					num2 = Mathf.Min(num2, cellRect.Width);
					num3 = Mathf.Min(num3, cellRect.Height);
				}
				intVec = new IntVec2(num2, num3);
			}
			Sketch sketch = new Sketch();
			if (Rand.Chance(WallsChanceCurve.Evaluate(num)))
			{
				SketchResolveParams parms2 = parms;
				parms2.sketch = sketch;
				parms2.mechClusterSize = intVec;
				SketchResolverDefOf.MechClusterWalls.Resolve(parms2);
			}
			List<ThingDef> buildingDefsForCluster = GetBuildingDefsForCluster(num, intVec, canBeDormant, value, parms.forceNoConditionCauser.GetValueOrDefault());
			AddBuildingsToSketch(sketch, intVec, buildingDefsForCluster);
			parms.sketch.MergeAt(sketch, default, Sketch.SpawnPosType.OccupiedCenter);
		}

		private static List<ThingDef> GetBuildingDefsForCluster(float points, IntVec2 size, bool canBeDormant, float? totalPoints, bool forceNoConditionCauser)
		{
			List<ThingDef> list = new List<ThingDef>();
			List<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) => def.building?.buildingTags != null && def.building.buildingTags.Contains(MechClusterMemberTag) && (!totalPoints.HasValue || (float)def.building.minMechClusterPoints <= totalPoints)).ToList();
			int liftCount = Mathf.FloorToInt(PointsToLiftCountCurve.Evaluate(points));
			Log.Message(liftCount);
			for (int i = 0; i < liftCount; i++)
			{
				list.Add(FantasyBiotechDefOf.VR_Construct_Lift);
			}

			// if (canBeDormant)
			// {
			// 	if (Rand.Chance(0.5f))
			// 	{
			// 		list.Add(ThingDefOf.ActivatorCountdown);
			// 	}
			// 	if (Rand.Chance(0.5f))
			// 	{
			// 		int num2 = GenMath.RoundRandom(ActivatorProximitysCountCurve.Evaluate(points));
			// 		for (int j = 0; j < num2; j++)
			// 		{
			// 			list.Add(ThingDefOf.ActivatorProximity);
			// 		}
			// 	}
			// }
			float pointsLeft = points;
			ThingDef thingDef = source.Where(( x) => x.building.buildingTags.Contains(MechClusterCombatThreatTag)).MinBy(( x) => x.building.combatPower);
			for (pointsLeft = Mathf.Max(pointsLeft, thingDef.building.combatPower); pointsLeft > 0f && source.Where(( x) => x.building.combatPower <= pointsLeft && x.building.buildingTags.Contains(MechClusterCombatThreatTag)).TryRandomElement(out ThingDef result4); pointsLeft -= result4.building.combatPower)
			{
				list.Add(result4);
			}
			return list;
		}

		private static bool TryRandomBuildingWithTag(string tag, List<ThingDef> allowedBuildings, List<ThingDef> generatedBuildings, IntVec2 size, out ThingDef result)
		{
			return allowedBuildings.Where((ThingDef x) => x.building.buildingTags.Contains(tag)).TryRandomElement(out result);
		}

		private static void AddBuildingsToSketch(Sketch sketch, IntVec2 size, List<ThingDef> buildings)
		{
			List<CellRect> edgeWallRects = new List<CellRect>
			{
				new CellRect(0, 0, size.x, 1),
				new CellRect(0, 0, 1, size.z),
				new CellRect(size.x - 1, 0, 1, size.z),
				new CellRect(0, size.z - 1, size.x, 1)
			};
			foreach (ThingDef item in buildings.OrderBy((ThingDef x) => x.building.IsTurret && !x.building.IsMortar))
			{
				bool flag = item.building.IsTurret && !item.building.IsMortar;
				if (!TryFindRandomPlaceFor(item, sketch, size, out var pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects) && !TryFindRandomPlaceFor(item, sketch, size + new IntVec2(6, 6), out pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects))
				{
					continue;
				}
				sketch.AddThing(item, pos, Rot4.North, GenStuff.RandomStuffByCommonalityFor(item));
				if (item != ThingDefOf.Turret_AutoMiniTurret)
				{
					continue;
				}
				if (pos.x < size.x / 2)
				{
					if (pos.z < size.z / 2)
					{
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					}
					else
					{
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					}
				}
				else if (pos.z < size.z / 2)
				{
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
				}
				else
				{
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
				}
			}
		}

		private static bool TryFindRandomPlaceFor(ThingDef thingDef, Sketch sketch, IntVec2 size, out IntVec3 pos, bool lowerLeftQuarterOnly, bool avoidCenter, bool requireLOSToEdge, bool avoidEdge, List<CellRect> edgeWallRects)
		{
			for (int i = 0; i < 200; i++)
			{
				CellRect cellRect = new CellRect(0, 0, size.x, size.z);
				if (lowerLeftQuarterOnly)
				{
					cellRect = new CellRect(cellRect.minX, cellRect.minZ, cellRect.Width / 2, cellRect.Height / 2);
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (avoidCenter)
				{
					CellRect cellRect2 = CellRect.CenteredOn(new CellRect(0, 0, size.x, size.z).CenterCell, size.x / 2, size.z / 2);
					for (int j = 0; j < 5; j++)
					{
						if (!cellRect2.Contains(randomCell))
						{
							break;
						}
						randomCell = cellRect.RandomCell;
					}
				}
				if (avoidEdge)
				{
					CellRect cellRect3 = CellRect.CenteredOn(new CellRect(0, 0, size.x, size.z).CenterCell, Mathf.RoundToInt((float)size.x * 0.75f), Mathf.RoundToInt((float)size.z * 0.75f));
					for (int k = 0; k < 5; k++)
					{
						if (cellRect3.Contains(randomCell))
						{
							break;
						}
						randomCell = cellRect.RandomCell;
					}
				}
				if (requireLOSToEdge)
				{
					IntVec3 end = randomCell;
					end.x += size.x + 1;
					IntVec3 end2 = randomCell;
					end2.x -= size.x + 1;
					IntVec3 end3 = randomCell;
					end3.z -= size.z + 1;
					IntVec3 end4 = randomCell;
					end4.z += size.z + 1;
					if (!sketch.LineOfSight(randomCell, end) && !sketch.LineOfSight(randomCell, end2) && !sketch.LineOfSight(randomCell, end3) && !sketch.LineOfSight(randomCell, end4))
					{
						continue;
					}
				}
				if (thingDef.building.minDistanceToSameTypeOfBuilding > 0)
				{
					bool flag = false;
					for (int l = 0; l < sketch.Things.Count; l++)
					{
						if (sketch.Things[l].def == thingDef && sketch.Things[l].pos.InHorDistOf(randomCell, thingDef.building.minDistanceToSameTypeOfBuilding))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				bool flag2 = false;
				CellRect cellRect4 = GenAdj.OccupiedRect(randomCell, Rot4.North, thingDef.Size);
				for (int m = 0; m < 4; m++)
				{
					if (cellRect4.Overlaps(edgeWallRects[m]))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2 && !sketch.WouldCollide(thingDef, randomCell, Rot4.North))
				{
					pos = randomCell;
					return true;
				}
			}
			pos = IntVec3.Invalid;
			return false;
		}

		[DebugOutput]
		public static void MechClusterBuildingSelection()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
			{
				float localPoints = item;
				list.Add(new DebugMenuOption(item.ToString("F0"), DebugMenuOptionMode.Action, delegate
				{
					string text = "";
					for (int i = 0; i < 50; i++)
					{
						int num = Rand.Range(10, 20);
						List<ThingDef> buildingDefsForCluster = GetBuildingDefsForCluster(localPoints, new IntVec2(num, num), canBeDormant: true, localPoints, forceNoConditionCauser: false);
						text = text + "points: " + localPoints + " , size: " + num;
						foreach (ThingDef item2 in buildingDefsForCluster)
						{
							text = text + "\n- " + item2.defName;
						}
						text += "\n\n";
					}
					Log.Message(text);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}
		public static List<Thing> SpawnCluster(IntVec3 center, Map map, MechClusterSketch sketch, bool dropInPods = true, bool canAssaultColony = false, string questTag = null)
		{
			List<Thing> spawnedThings = [];
			Faction constructFaction = MechUtility.ConstructFaction();
			if (constructFaction == null)
			{
				Log.Warning("Could not spawn mech cluster, no world mech faction found.");
				return spawnedThings;
			}
			foreach (IntVec3 item in sketch.buildingsSketch.OccupiedRect)
			{
				IntVec3 c = item + center;
				if (!c.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(map);
				Thing thing = null;
				foreach (Thing item2 in thingList)
				{
					if (!item2.def.IsBlueprint) continue;
					thing = item2;
					break;
				}
				thing?.Destroy();
			}
			Sketch.SpawnMode spawnMode = !dropInPods ? Sketch.SpawnMode.Normal : Sketch.SpawnMode.TransportPod;
			sketch.buildingsSketch.Spawn(map, center, constructFaction, Sketch.SpawnPosType.Unchanged, spawnMode, wipeIfCollides: false, forceTerrainAffordance: false, clearEdificeWhereFloor: false, spawnedThings, sketch.startDormant, buildRoofsInstantly: false, CanSpawnThing, delegate(IntVec3 spot, SketchEntity entity)
			{
				if (entity is SketchThing sketchThing && sketchThing.def != ThingDefOf.Wall && sketchThing.def != ThingDefOf.Barricade)
				{
					entity.SpawnNear(spot, map, 12f, constructFaction, spawnMode, wipeIfCollides: false, forceTerrainAffordance: false, spawnedThings, sketch.startDormant, CanSpawnThing);
				}
			});
			float defendRadius = Mathf.Sqrt(sketch.buildingsSketch.OccupiedSize.x * sketch.buildingsSketch.OccupiedSize.x + sketch.buildingsSketch.OccupiedSize.z * sketch.buildingsSketch.OccupiedSize.z) / 2f + 6f;
			LordJob_MechanoidDefendBase lordJob_MechanoidDefendBase  = !sketch.startDormant ? new LordJob_MechanoidsDefend(spawnedThings, constructFaction, defendRadius, center, canAssaultColony, isMechCluster: true) : new LordJob_SleepThenMechanoidsDefend(spawnedThings, constructFaction, defendRadius, center, canAssaultColony, isMechCluster: true);
			Lord lord = LordMaker.MakeNewLord(constructFaction, lordJob_MechanoidDefendBase, map);
			QuestUtility.AddQuestTag(lord, questTag);
			float randomInRange = MechClusterUtility.InitiationDelay.RandomInRange;
			int num = (int)(MechClusterUtility.MechAssemblerInitialDelayDays.RandomInRange * 60000f);
			for (int i = 0; i < spawnedThings.Count; i++)
			{
				Thing thing2 = spawnedThings[i];
				thing2.TryGetComp<CompSpawnerPawn>()?.CalculateNextPawnSpawnTick(num);
				if (thing2.TryGetComp<CompProjectileInterceptor>() != null)
				{
					lordJob_MechanoidDefendBase.AddThingToNotifyOnDefeat(thing2);
				}
				if (Rand.Chance(0.6f))
				{
					CompInitiatable compInitiatable = thing2.TryGetComp<CompInitiatable>();
					if (compInitiatable != null)
					{
						compInitiatable.initiationDelayTicksOverride = (int)(60000f * randomInRange);
					}
				}
				if (thing2 is Building b && MechClusterUtility.IsBuildingThreat(b))
				{
					lord.AddBuilding(b);
				}
				thing2.SetFaction(Faction.OfMechanoids);
			}
			if (!sketch.pawns.NullOrEmpty())
			{
				foreach (MechClusterSketch.Mech pawn2 in sketch.pawns)
				{
					IntVec3 result = pawn2.position + center;
					if (!result.Standable(map) && !CellFinder.TryFindRandomCellNear(result, map, 12, (IntVec3 x) => x.Standable(map), out result))
					{
						continue;
					}
					Pawn pawn = PawnGenerator.GeneratePawn(pawn2.kindDef, constructFaction);
					CompCanBeDormant compCanBeDormant = pawn.TryGetComp<CompCanBeDormant>();
					if (compCanBeDormant != null)
					{
						if (sketch.startDormant)
						{
							compCanBeDormant.ToSleep();
						}
						else
						{
							compCanBeDormant.WakeUp();
						}
					}
					lord.AddPawn(pawn);
					spawnedThings.Add(pawn);
					GenSpawn.Spawn(pawn, result, map);
				}
			}
			foreach (Thing item3 in spawnedThings)
			{
				if (!sketch.startDormant)
				{
					item3.TryGetComp<CompWakeUpDormant>()?.Activate(null, sendSignal: true, silent: true);
				}
			}
			return spawnedThings;
			bool CanSpawnThing(SketchEntity ent, IntVec3 cell)
			{
				foreach (IntVec3 item4 in ent.OccupiedRect.MovedBy(cell))
				{
					if (item4.InBounds(map))
					{
						foreach (Thing thing3 in item4.GetThingList(map))
						{
							if (thing3.def.preventSkyfallersLandingOn)
							{
								return false;
							}
						}
					}
				}
				return true;
			}
		}

	}
}