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
            // if (launcher is not Pawn caster)
            // {
            //     Log.Error("Fantasy Biotech :: Chain Lighting : Launcher is not a Pawn.");
            //     return;
            // }
            IntVec3 strikePos = hitThing?.Position ?? Position;
            if (hitThing == null) return;
            if (!_props.doubleTarget)
            {
                _hitThings.Add(hitThing);
            }
            if (_numBounces >= _props.bounceCount) return;
            if (!TryFindNextTarget(strikePos, launcher, out Pawn nextTarget))
                return;
            FireAt(nextTarget, hitThing);
        }

        private bool TryFindNextTarget(IntVec3 originCell, Thing caster, out Pawn target)
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
            return !_hitThings.Contains(pawn) && pawn.Spawned && !pawn.Dead && !pawn.Downed && pawn.HostileTo(launcher);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 start = (_previousProjectile ?? launcher).TrueCenter();
            Vector3 end = DrawPos;
            if (end.magnitude > start.magnitude)
            {
                (end, start) = (start, end);
            }
            Vector3 center = (start + end) / 2f;
            float length = (start - end).magnitude;
            float angle = start.AngleToFlat(end) + 90f;
            Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(1f, 1f, length));
            Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic.MatSingle, 0);
        }

        private void FireAt(Thing newTarget, Thing hitThing)
        {
            IntVec3 launchPos = hitThing.Position;

            Projectile_Bouncing newProj = (Projectile_Bouncing)GenSpawn.Spawn(def, launchPos, newTarget.Map);
            newProj._numBounces = _numBounces + 1;
            if (!_props.doubleTarget)
            {
                newProj._hitThings = new List<Thing>(_hitThings);
            }
            newProj._previousProjectile = this;
            newProj._props = _props;

            newProj.Launch(launcher, newTarget, newTarget, ProjectileHitFlags.IntendedTarget);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _numBounces, "numBounces");
            if (!_props.doubleTarget)
            {
                Scribe_Collections.Look(ref _hitThings, "hitThings", LookMode.Reference);
            }
            Scribe_References.Look(ref _previousProjectile, "previousProjectile");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _props ??= def.GetModExtension<BounceProjectileExtension>();
            }
        }
    }
}
