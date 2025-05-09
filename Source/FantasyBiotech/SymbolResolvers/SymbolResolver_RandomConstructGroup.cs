using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace FantasyBiotech
{
    public class SymbolResolver_RandomConstructGroup : SymbolResolver
    {
        private static readonly IntRange DefaultMechanoidCountRange = new IntRange(1, 5);

        public override void Resolve(ResolveParams rp)
        {
            int num = rp.mechanoidsCount ?? DefaultMechanoidCountRange.RandomInRange;
            Lord lord = rp.singlePawnLord;
            if (lord == null && num > 0)
            {
                Map map = BaseGen.globalSettings.map;
                lord = LordMaker.MakeNewLord(lordJob: (!Rand.Bool || !rp.rect.Cells.Where((IntVec3 x) => !x.Impassable(map)).TryRandomElement(out var result)) ? ((LordJob)new LordJob_AssaultColony(MechUtility.ConstructFaction(), canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false)) : ((LordJob)new LordJob_DefendPoint(result)), faction: MechUtility.ConstructFaction(), map: map);
            }
            for (int i = 0; i < num; i++)
            {
                var pawnKindDef = rp.singlePawnKindDef;
                pawnKindDef ??= DefDatabase<PawnKindDef>.AllDefsListForReading.Where(MechUtility.ConstructSuitableForCluster).RandomElementByWeight((PawnKindDef kind) => 1f / kind.combatPower);
                ResolveParams resolveParams = rp;
                resolveParams.singlePawnKindDef = pawnKindDef;
                resolveParams.singlePawnLord = lord;
                resolveParams.faction = MechUtility.ConstructFaction();
                BaseGen.symbolStack.Push("pawn", resolveParams);
            }
        }
    }
}