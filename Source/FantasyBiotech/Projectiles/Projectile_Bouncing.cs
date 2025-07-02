using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyBiotech
{
    public class Projectile_Bouncing : Bullet
    {
        private int _numBounces;
        private List<Thing> _hitThings = [];
        private Thing _previousProjectile;
        private BounceProjectileExtension _props;

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);
            _props ??= def.GetModExtension<BounceProjectileExtension>();
            if (launcher is not Pawn caster)
            {
                Log.Error("Chain Lightning: Launcher is not a Pawn.");
                return;
            }
            IntVec3 strikePos = hitThing?.Position ?? Position;
            if (hitThing == null) return;
            _hitThings.Add(hitThing);
            DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, def.projectile.GetDamageAmount(launcher), def.projectile.GetArmorPenetration(launcher), ExactRotation.eulerAngles.y, launcher, null, equipmentDef);
            hitThing.TakeDamage(dinfo);
            if (_numBounces >= _props.bounceCount) return;
            if (!TryFindNextTarget(strikePos, caster, out Pawn nextTarget))
                return;
            FireAt(nextTarget, hitThing);
        }

        private bool TryFindNextTarget(IntVec3 originCell, Pawn caster, out Pawn target)
        {
            target = null;
            foreach (Pawn pawn in RadialUtility.GetPawnsInRadius(originCell, caster.Map, _props.bounceRadius))
            {
                if (!IsValidTarget(pawn)) continue;
                target = pawn;
                return true;
            }
            return false;
        }

        private bool IsValidTarget(Pawn pawn)
        {
            return !_hitThings.Contains(pawn) &&
                   pawn.Spawned && !pawn.Dead && !pawn.Downed &&
                   pawn.HostileTo(launcher);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 vec1 = (_previousProjectile ?? launcher).TrueCenter();
            Vector3 vec2 = this.DrawPos;
            if (vec2.magnitude > vec1.magnitude)
            {
                (vec2, vec1) = (vec1, vec2);
            }

            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(vec2 + (vec1 - vec2) / 2, Quaternion.AngleAxis(vec1.AngleToFlat(vec2) + 90f, Vector3.up), new Vector3(1f, 1f, (vec1 - vec2).magnitude)),
                Graphic.MatSingle, 0);
        }

        private void FireAt(Thing newTarget, Thing hitThing)
        {
            IntVec3 launchPos = hitThing.Position;

            Projectile_Bouncing newProj = (Projectile_Bouncing)GenSpawn.Spawn(def, launchPos, newTarget.Map);
            newProj._numBounces = this._numBounces + 1;
            newProj._hitThings = new List<Thing>(_hitThings);
            newProj._previousProjectile = this;
            newProj._props = _props;

            newProj.Launch(launcher, newTarget, newTarget, ProjectileHitFlags.IntendedTarget);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _numBounces, "numBounces");
            Scribe_Collections.Look(ref _hitThings, "hitThings", LookMode.Reference);
            Scribe_References.Look(ref _previousProjectile, "previousProjectile");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _props ??= def.GetModExtension<BounceProjectileExtension>();
            }
        }
    }
}
