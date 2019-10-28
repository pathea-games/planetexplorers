using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkillSystem
{
	public class ExpFuncSet : IExpFuncSet
	{
		const float ShieldToHP = 10f;
		ISkAttribs _parent;
		public ExpFuncSet(ISkAttribs parent){			_parent = parent;		}

		// Interface
		public bool RandIn(float prob)
		{
			return UnityEngine.Random.value < prob;
		}

        public void GetHurt(float dmg)
        {
            SkInst skInst = SkRuntimeInfo.Current as SkInst;
            if (null != skInst)
            {
                if (skInst._colInfo != null && skInst._colInfo.damageScale > PETools.PEMath.Epsilon)
                    dmg *= skInst._colInfo.damageScale;
            }

            SkEntity caster = SkRuntimeInfo.Current.Caster;
            SkEntity target = SkRuntimeInfo.Current.Target;

            _parent.raws[(int)Pathea.AttribType.Hp] -= dmg;
            _parent.sums[(int)Pathea.AttribType.Hp] -= dmg;
            _parent.modflags[(int)Pathea.AttribType.Hp] = false;

            float curTime = Time.time;
            if (caster != null) caster._lastestTimeOfHurtingSb = curTime;
            if (target != null) target._lastestTimeOfGettingHurt = curTime;

            SkInst inst = SkRuntimeInfo.Current as SkInst;
            if (inst != null)
            {
                inst.Caster.OnHurtSb(inst, dmg);
                inst.Target.OnGetHurt(inst, dmg);
            }
        }

        public void TryGetHurt(float dmg, float exp = 0)
		{ 
			SkInst skInst = SkRuntimeInfo.Current as SkInst;
			if(null != skInst)
			{
				if(skInst._colInfo != null && skInst._colInfo.damageScale > PETools.PEMath.Epsilon)
					dmg *= skInst._colInfo.damageScale;
			}

			SkEntity caster = SkRuntimeInfo.Current.Caster;
			SkEntity target = SkRuntimeInfo.Current.Target;
			if(CanDamage(caster, target))
			{
				if(caster is Pathea.Projectile.SkProjectile)
				{
					Pathea.Projectile.SkProjectileDamageScale damageScale = caster.GetComponent<Pathea.Projectile.SkProjectileDamageScale>();
					if(null != damageScale)
						dmg *= damageScale.damageScale;
				}

				float shield = _parent.sums[(int)Pathea.AttribType.Shield];
				if(shield > 0 && caster is Pathea.Projectile.SkProjectile)
				{
					if(shield * ShieldToHP  < dmg)
					{
						_parent.sums[(int)Pathea.AttribType.Shield] = 0;
						_parent.raws[(int)Pathea.AttribType.Hp] -= dmg - shield * ShieldToHP;
						_parent.sums[(int)Pathea.AttribType.Hp] -= dmg - shield * ShieldToHP; 
						_parent.modflags[(int)Pathea.AttribType.Hp] = false;
					}
					else
						_parent.sums[(int)Pathea.AttribType.Shield] -= dmg / ShieldToHP;
				}
				else{
					_parent.raws[(int)Pathea.AttribType.Hp] -= dmg;
					_parent.sums[(int)Pathea.AttribType.Hp] -= dmg;
					_parent.modflags[(int)Pathea.AttribType.Hp] = false;
				}

				float curTime = Time.time;
				if(caster != null) caster._lastestTimeOfHurtingSb = curTime;
				if(target != null) target._lastestTimeOfGettingHurt = curTime;

				SkInst inst = SkRuntimeInfo.Current as SkInst;
				if(inst != null)
				{
					inst.Caster.OnHurtSb(inst, dmg);
					inst.Target.OnGetHurt(inst, dmg);
				}
			}

			if(exp > PETools.PEMath.Epsilon)
			{
				SkEntity parentCaster = caster;
				if(caster is Pathea.Projectile.SkProjectile)
					parentCaster = (parentCaster as Pathea.Projectile.SkProjectile).parentSkEntity;
				parentCaster.SetAttribute((int)Pathea.AttribType.Exp, parentCaster.GetAttribute((int)Pathea.AttribType.Exp) + exp, false);
			}
		}

		public bool InCoolingForGettingHurt(float cooltime)
		{
			return (Time.time >= SkRuntimeInfo.Current.Target._lastestTimeOfGettingHurt + cooltime);
		}
		public bool InCoolingForConsumingStamina(float cooltime)
		{
			return (Time.time > SkRuntimeInfo.Current.Target._lastestTimeOfConsumingStamina + Mathf.Max(cooltime, 0.1f));
		}

        static bool HasReputation(int p1, int p2)
        {
            if (!ReputationSystem.Instance.GetActiveState(p1))
                return false;

            return p2 == 4 || p2 == 5;
        }

        // Helper
        static bool CanDamage(SkEntity e1, SkEntity e2)
        {
			Pathea.Projectile.SkProjectile p = e1 as Pathea.Projectile.SkProjectile;
			if(p != null)
			{
                SkEntity caster = p.GetSkEntityCaster();
                if (caster == null)
                    return false;
                else
				    e1 = p.GetSkEntityCaster();
			}

            int p1 = System.Convert.ToInt32(e1.GetAttribute((int)Pathea.AttribType.DefaultPlayerID));
            int p2 = System.Convert.ToInt32(e2.GetAttribute((int)Pathea.AttribType.DefaultPlayerID));

            int d1 = System.Convert.ToInt32(e1.GetAttribute((int)Pathea.AttribType.DamageID));
            int d2 = System.Convert.ToInt32(e2.GetAttribute((int)Pathea.AttribType.DamageID));


            return PETools.PEUtil.CanDamageReputation(p1, p2) && ForceSetting.Instance.Conflict(p1, p2) && Pathea.DamageData.GetValue(d1, d2) != 0;
        }
	}
}