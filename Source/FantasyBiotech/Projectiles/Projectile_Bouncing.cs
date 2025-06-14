﻿using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyBiotech
{
    public class Projectile_Bouncing : Bullet
    {
        private const int MaxBounces = 3;
        private const float SearchRadius = 10f;
        private int _numBounces;
        private List<Thing> _hitThings = [];
#nullable enable
        private Thing? _previousProjectile;
#nullable disable

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {

            base.Impact(hitThing);
            if (launcher is not Pawn caster)
            {
                Log.Warning("Chain Lightning: Launcher is not a Pawn or map is null.");
                return;
            }
            var strikePos = hitThing?.Position ?? Position;
            if (hitThing != null)
            {
                _hitThings.Add(hitThing);
                DamageInfo dinfo = new(
                    def.projectile.damageDef,
                    def.projectile.GetDamageAmount(launcher),
                    def.projectile.GetArmorPenetration(launcher),
                    ExactRotation.eulerAngles.y,
                    launcher,
                    null,
                    equipmentDef
                );
                hitThing.TakeDamage(dinfo);
                if (_numBounces < MaxBounces)
                {
                    Pawn nextTarget = FindNextTarget(strikePos, caster);
                    if (nextTarget != null)
                    {
                        Log.Message("Firing next target");
                        FireAt(nextTarget, hitThing);
                    }
                }
            }
        }
#nullable enable
        private Pawn? FindNextTarget(IntVec3 originCell, Pawn caster)
        {
            foreach (var pawn in RadialUtility.GetPawnsInRadius(originCell, caster.Map, SearchRadius))
            {
                if (IsValidTarget(pawn))
                {
                    return pawn;
                }
            }
            return null;
        }
#nullable disable

        private bool IsValidTarget(Pawn pawn)
        {
            return !_hitThings.Contains(pawn) &&
                   pawn.Spawned && !pawn.Dead && !pawn.Downed &&
                   pawn.HostileTo(launcher);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var vec1 = (_previousProjectile ?? launcher).TrueCenter();
            var vec2 = this.DrawPos;
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

            var newProj = (Projectile_Bouncing)GenSpawn.Spawn(def, launchPos, hitThing.Map);
            newProj._numBounces = this._numBounces + 1;
            newProj._hitThings = new List<Thing>(_hitThings);
            newProj.Launch(launcher, newTarget, newTarget, ProjectileHitFlags.IntendedTarget);
            newProj._previousProjectile = this;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _numBounces, "numBounces");
            Scribe_Collections.Look(ref _hitThings, "hitThings", LookMode.Reference);
            Scribe_References.Look(ref _previousProjectile, "previousProjectile");
        }
    }
}
