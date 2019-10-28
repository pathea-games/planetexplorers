using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem
{
	public class SkTriggerEvent
	{
		internal int _id;
		internal SkCond _cond;
		internal float _force;
		internal SkAttribsModifier _modsCaster = null;
		internal SkAttribsModifier _modsTarget = null;
		internal SkEffect _effOnHitCaster = null; //parameter
		internal SkEffect _effOnHitTarget = null; //parameter

		internal static Dictionary<int, SkTriggerEvent> s_SkTriggerEventTbl;
		public static void LoadData()
		{
			if (s_SkTriggerEventTbl != null)			return;
			
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("skTriggerEvent");
			//reader.Read(); // skip title if needed
			s_SkTriggerEventTbl = new Dictionary<int, SkTriggerEvent>();
			while (reader.Read())
			{
				SkTriggerEvent skEvent = new SkTriggerEvent();
				skEvent._id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_id")));

				skEvent._cond = SkCond.Create(reader.GetString(reader.GetOrdinal("_cond")));
				skEvent._force = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_force")));
				skEvent._modsCaster = SkAttribsModifier.Create(reader.GetString(reader.GetOrdinal("_modsCaster")));
				skEvent._modsTarget = SkAttribsModifier.Create(reader.GetString(reader.GetOrdinal("_modsTarget")));
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effOnHitCaster"))), out skEvent._effOnHitCaster);
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effOnHitTarget"))), out skEvent._effOnHitTarget);

				try{
					s_SkTriggerEventTbl.Add(skEvent._id, skEvent);
				}catch(Exception e)	{
					Debug.LogError("Exception on skTriggerEvent "+skEvent._id+" "+e);
				}
			}
		}

		public bool Exec(SkInst inst)
		{
			bool ret = false;
			SkRuntimeInfo.Current = inst;

			if(_cond.Tst(inst))
			{
				inst._forceMagnitude = _force;
				if(inst._target != null)
				{
					if(_modsCaster != null )	_modsCaster.Exec(inst._caster.attribs, inst._caster.attribs, inst._target.attribs, inst._para as ISkAttribsModPara);
					if(_modsTarget != null )	_modsTarget.Exec(inst._target.attribs, inst._caster.attribs, inst._target.attribs, inst._para as ISkAttribsModPara);
					if(_force > 0.0f) 			inst._forceDirection = inst.GetForceVec();
					if(_effOnHitCaster != null)	_effOnHitCaster.Apply(inst._caster, inst);
					if(_effOnHitTarget != null)	_effOnHitTarget.Apply(inst._target, inst);
				}
				else
				{
					inst._tmpTar = null;
					if(_effOnHitCaster != null)	_effOnHitCaster.Apply(inst._caster, inst);
					if(_cond._retTars != null && _cond._retTars.Count > 0)
					{
						foreach(SkEntity target in _cond._retTars)	
						{
							inst._tmpTar = target;
							if(_modsCaster != null )
							{
								_modsCaster.Exec(inst._caster.attribs, inst._caster.attribs, target.attribs, inst._para as ISkAttribsModPara);
							}
							if(_modsTarget != null )
							{
								_modsTarget.Exec(target.attribs, 		inst._caster.attribs, target.attribs, inst._para as ISkAttribsModPara);
							}
							if(_effOnHitTarget != null)
							{
								if(_force > 0.0f) inst._forceDirection = inst.GetForceVec();
								_effOnHitTarget.Apply(target, inst);
							}
						}
					}
					else
					{
						if(_modsCaster != null )	_modsCaster.Exec(inst._caster.attribs, inst._caster.attribs, null, inst._para as ISkAttribsModPara);
					}
				}
				ret = true;
			}
			SkRuntimeInfo.Current = null;
			return ret;
		}
	}
}
