using Verse;

namespace FantasyBiotech
{
    public class Projectile_ColossusCannon : Projectile
    {
        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            base.Impact(hitThing, blockedByShield);
            IntVec3 position = base.Position;
            DamageDef damageDef = base.DamageDef;
            Thing instigator = launcher;
            int damageAmount = DamageAmount;
            float armorPenetration = ArmorPenetration;
            ThingDef weapon = equipmentDef;
            ThingDef projectile = def;
            Thing thing = intendedTarget.Thing;
            float explosionChanceToStartFire = def.projectile.explosionChanceToStartFire;
            float expolosionPropagationSpeed = base.DamageDef.expolosionPropagationSpeed;
            float explosionRadius = def.projectile.explosionRadius;
            GenExplosion.DoExplosion(position, map, explosionRadius, damageDef, instigator, damageAmount, armorPenetration, null, weapon, projectile, thing, null, 0f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, explosionChanceToStartFire, damageFalloff: false, null, null, null, doVisualEffects: false, expolosionPropagationSpeed);
        }
    }
}
