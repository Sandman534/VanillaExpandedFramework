﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OPToxic
{
    public class DamageWorker_OPToxic : DamageWorker
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            if (def.explosionHeatEnergyPerCell > 1.401298E-45f)
            {
                GenTemperature.PushHeat(explosion.Position, explosion.Map, def.explosionHeatEnergyPerCell * cellsToAffect.Count);
            }
            FleckMaker.Static(explosion.Position, explosion.Map, FleckDefOf.ExplosionFlash, explosion.radius * 6f);
            FleckMaker.Static(explosion.Position, explosion.Map, FleckDefOf.ExplosionFlash, explosion.radius * 6f);
            ExplosionVisualEffectCenter(explosion);
        }

        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
            if (victim.def.category == ThingCategory.Pawn && victim.def.useHitPoints && dinfo.Def.harmsHealth)
            {
                float amount = dinfo.Amount;
                damageResult.totalDamageDealt = Mathf.Min(victim.HitPoints, GenMath.RoundRandom(amount));
                victim.HitPoints -= Mathf.RoundToInt(damageResult.totalDamageDealt);
                if (victim.HitPoints <= 0)
                {
                    victim.HitPoints = 0;
                    victim.Kill(new DamageInfo?(dinfo), null);
                }
            }
            return damageResult;
        }
    }
}