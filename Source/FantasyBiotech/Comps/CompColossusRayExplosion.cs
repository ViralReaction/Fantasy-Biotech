using System.Collections;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FantasyBiotech
{
    public class CompColossusRayExplosion : ThingComp
    {
        private CompProperties_ColossusRayExplosion Props => (CompProperties_ColossusRayExplosion)props;
        private List<QueuedExplosion> queuedExplosions = [];

        private class QueuedExplosion : IExposable
        {
            public IntVec3 cell;
            public int ticksLeft;
            public Thing instigator;

            public void ExposeData()
            {
                Scribe_Values.Look(ref cell, "cell");
                Scribe_Values.Look(ref ticksLeft, "ticksLeft");
                Scribe_References.Look(ref instigator, "instigator");
            }
        }
        public override void PostExposeData()
        {
	        base.PostExposeData();
	        Scribe_Collections.Look(ref queuedExplosions, "queuedExplosions", LookMode.Deep);
        }

        public override void CompTick()
        {
	        if (queuedExplosions.Count == 0) return;

	        queuedExplosions[0].ticksLeft--;
	        if (queuedExplosions[0].ticksLeft > 0) return;

	        for (int i = 0; i < queuedExplosions.Count; i++)
	        {
		        QueuedExplosion explosion = queuedExplosions[i];
		        if (parent?.Map != null && explosion.cell.InBounds(parent.Map))
		        {
			        GenExplosion.DoExplosion(
				        explosion.cell,
				        parent.Map,
				        Props.radius,
				        Props.damageDef ?? DamageDefOf.Bomb,
				        explosion.instigator,
				        Props.damageAmount,
				        Props.armorPenetration
			        );
		        }
	        }
	        queuedExplosions.Clear();
        }

        public void QueueExplosion(IntVec3 cell, Thing instigator)
        {
	        queuedExplosions.Add(new QueuedExplosion
	        {
		        cell = cell,
		        ticksLeft = Props.defaultDelayTicks,
		        instigator = instigator
	        });
        }

    }
}
