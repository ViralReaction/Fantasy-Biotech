using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class CompAbilityEffect_SteamBurst : CompAbilityEffect
    {
        private new CompProperties_AbilitySteamBurst Props => (CompProperties_AbilitySteamBurst)props;

        private Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            GenExplosion.DoExplosion(Pawn.Position, Pawn.MapHeld, Props.radius, DamageDefOf.Burn, Pawn, Props.damage, Props.armorPen, null, null, null, null, null, 1f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, null, null, doVisualEffects: false, 0.6f);
            base.Apply(target, dest);
        }

        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            yield return new PreCastAction
            {
                action = delegate
                {
                    parent.AddEffecterToMaintain(FantasyBiotechDefOf.VR_Steam_Burst.Spawn(parent.pawn.Position, parent.pawn.Map), parent.pawn.Position, 17, parent.pawn.Map);
                },
                ticksAwayFromCast = 17
            };
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            if (target is { HasThing: true, Thing: Pawn pawn })
            {
                return pawn.TargetCurrentlyAimingAt == Pawn;
            }
            return false;
        }

    }
}
