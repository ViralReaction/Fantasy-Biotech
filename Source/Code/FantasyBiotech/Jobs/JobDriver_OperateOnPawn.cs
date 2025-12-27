using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FantasyBiotech
{

    public class JobDriver_OperateOnPawn : JobDriver
    {
        public Building_GeneRemover GeneExtractor => TargetA.Thing as Building_GeneRemover;
        public Pawn PawnOperated => GeneExtractor.ContainedPawn;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.ReserveSittableOrSpot(GeneExtractor.InteractionCell, job, errorOnFailed))
            {
                return false;
            }
            return pawn.Reserve(TargetA, job);
        }

        private static float OperateSpeedMultiplier = 3;
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => CanWork(pawn, PawnOperated, GeneExtractor) is false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil operateToil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
                initAction = delegate
                {
                    if (PawnOperated.Dead is false)
                    {
                        HealthUtility.TryAnesthetize(PawnOperated);
                    }
                },
                tickAction = delegate
                {
                    float speed = pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * OperateSpeedMultiplier;
                    GeneExtractor.ticksRemaining -= Mathf.Max(1, (int)speed);
                    if (GeneExtractor.ticksRemaining <= 0)
                    {
                        GeneExtractor.Finish(pawn);
                    }
                }
            };
            operateToil.WithEffect(FantasyBiotechDefOf.ButcherFlesh, TargetIndex.A);
            operateToil.PlaySustainerOrSound(() => FantasyBiotechDefOf.Recipe_ButcherCorpseFlesh);
            yield return operateToil;
        }

        public static bool CanWork(Pawn worker, Pawn pawnOperated, Building_GeneRemover geneExtractor)
        {
            if (PawnUtility.WillSoonHaveBasicNeed(worker, -0.05f))
            {
                return false;
            }
            if (pawnOperated is null)
            {
                return false;
            }
            if (pawnOperated.Corpse != null && pawnOperated.Corpse.GetRotStage() != RotStage.Fresh)
            {
                return false;
            }
            if (geneExtractor.ticksRemaining <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
