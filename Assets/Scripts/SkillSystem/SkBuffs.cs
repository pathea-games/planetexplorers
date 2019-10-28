using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem
{
	public class SkBuff
	{
		internal int _id;
		internal int _desc;
		internal string _icon;
		internal string _name;
		internal int _type;
		internal int _priority;
		internal int _stackLimit;
		internal float _lifeTime;
		internal float _interval;
		internal SkAttribsModifier _mods;
		internal SkEffect _eff = null;
		internal SkEffect _effBeg = null;
		internal SkEffect _effEnd = null;

		internal static Dictionary<int, SkBuff> s_SkBuffTbl;
		internal static int s_maxId = 0;
		public static void LoadData()
		{
			if(s_SkBuffTbl != null)			return;

			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("skBuff");
			//reader.Read(); // skip title if needed
			s_SkBuffTbl = new Dictionary<int, SkBuff>();
			while (reader.Read())
			{
				SkBuff buff = new SkBuff();
				buff._id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_id")));
				buff._desc = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_desc")));
				buff._icon = reader.GetString(reader.GetOrdinal("_icon"));
				buff._name = reader.GetString(reader.GetOrdinal("_name"));
				buff._type = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_type")));
				buff._priority = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_priority")));
				buff._stackLimit = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_stackLimit")));
				buff._lifeTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeActive")));
				buff._interval = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeInterval")));
				buff._mods = SkAttribsModifier.Create(reader.GetString(reader.GetOrdinal("_mods")));
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_eff"))), out buff._eff);
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effBeg"))), out buff._effBeg);
				SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effEnd"))), out buff._effEnd);

				try{
					s_SkBuffTbl.Add(buff._id, buff);
				}catch(Exception e)	{
					Debug.LogError("Exception on skBuff "+buff._id+" "+e);
				}
				if(s_maxId < buff._id)	s_maxId = buff._id;
			}
		}
		public static int CreateBuff(int tmplBuffId, string modDesc)	//A*(x+B)+C
		{
			// TODO : code to catch exception while tmplBuff == null
			SkBuff tmplBuff = s_SkBuffTbl[tmplBuffId];
			SkBuff buff = new SkBuff();
			buff._id = ++s_maxId;
			buff._icon = tmplBuff._icon;
			buff._desc = tmplBuff._desc;
			buff._name = tmplBuff._name;
			buff._type = tmplBuff._type;
			buff._priority = tmplBuff._priority;
			buff._stackLimit = tmplBuff._stackLimit;
			buff._lifeTime = tmplBuff._lifeTime;
			buff._interval = tmplBuff._interval;
			buff._mods = SkAttribsModifier.Create(modDesc);
			buff._eff = tmplBuff._eff;
			buff._effBeg = tmplBuff._effBeg;
			buff._effEnd = tmplBuff._effEnd;
			s_SkBuffTbl.Add(buff._id, buff);
			if(s_maxId < buff._id)	s_maxId = buff._id;
			return s_maxId;
		}
	}
	public class SkBuffInst : SkRuntimeInfo
	{
		internal SkBuff _buff;
		internal ISkPara _para;
		internal ISkAttribs _paraCaster;
		internal ISkAttribs _paraTarget;
		internal int _n = 0;
		internal float _startTime;
		// SkRuntimeInfo
		public override ISkPara Para{ get{ return _para;		}	}
		public override SkEntity Caster{ 	get{ return _paraCaster != null ? (_paraCaster.entity as SkEntity) : null;	}	}
		public override SkEntity Target{ 	get{ return _paraTarget != null ? (_paraTarget.entity as SkEntity) : null;	}	}

		public static SkBuffInst Create(int buffId, ISkAttribs paraCaster = null, ISkAttribs paraTarget = null)
		{
			SkBuff buff;
			if(SkBuff.s_SkBuffTbl.TryGetValue(buffId, out buff))
			{
				SkBuffInst inst = new SkBuffInst();
				inst._buff = buff;
				SkEntity caster = SkRuntimeInfo.Current != null ? SkRuntimeInfo.Current.Caster : null;
				inst._paraCaster = (paraCaster == null && caster != null) ? caster.attribs : paraCaster;
				inst._paraTarget = paraTarget;
				inst._startTime = Time.time;
				return inst;
			}
			return null;
		}
		public static SkBuffInst Mount(SkPackage buffPak, SkBuffInst inst)
		{
			if(inst == null)	return null;

			int id = inst._buff._id;
			int type = inst._buff._type;
			int prior = inst._buff._priority;
			int maxStack = inst._buff._stackLimit;
			
			List<SkBuffInst> stackBuffs = buffPak._buffs.FindAll((SkBuffInst it)=>it._buff._id == id);
			if(stackBuffs.Count == 0)
			{
				if(buffPak._buffs.Exists((SkBuffInst it)=>(it._buff._type == type && it._buff._priority > prior)))
					return null;	// Failed to add this buff because there are hi-prior buff(s)

				// Unmount lo-prior buffs
				Unmount(buffPak, (SkBuffInst it)=>(it._buff._type == type && it._buff._priority < prior));
			}
			else if(stackBuffs.Count >= maxStack)
			{
				// Unmount same id buffs to fit stack limit
				Unmount(buffPak, stackBuffs[0]);
			}

			buffPak._buffs.Add(inst);
			return inst;
		}
		public static void Unmount(SkPackage buffPak, SkBuffInst inst, bool force = true)
		{
			bool ret = inst.OnDiscard(buffPak._parentAttribs);
			if (ret || force) {
				buffPak._buffs.Remove (inst);
			}
		}
		public static void Unmount(SkPackage buffPak, System.Predicate<SkBuffInst> match)
		{
			for(int i = buffPak._buffs.Count - 1; i >= 0; i--)
			{
				if(match(buffPak._buffs[i])){
					Unmount(buffPak, buffPak._buffs[i]); 
				}
			}
		}
		// Get first matched buff
		public static SkBuffInst GetBuff(SkPackage buffPak, Func<SkBuffInst, bool> match)
		{
			return buffPak._buffs.Find((SkBuffInst inst)=>match(inst));
		}

		public bool Exec(ISkAttribs dst)
		{
			bool ret = true;
			SkRuntimeInfo.Current = this;

			if(_n == 0)
			{
				if(_buff._effBeg != null)				_buff._effBeg.Apply(Target, this);
				if(_buff._eff != null) 					_buff._eff.Apply(Target, this);
				if(_buff._mods!= null)					_buff._mods.Exec(dst, _paraCaster, _paraTarget, _para as ISkAttribsModPara);
				_n++;
			}

			float elapse = Time.time-_startTime;
			if(_buff._interval > PETools.PEMath.Epsilon && elapse > _n*_buff._interval)
			{
				if(_buff._eff != null) 				_buff._eff.Apply(Target, this);
				if(_buff._mods!= null)				_buff._mods.Exec(dst, _paraCaster, _paraTarget, _para as ISkAttribsModPara);
				_n++;
			}

			if(_buff._lifeTime > -PETools.PEMath.Epsilon && elapse > _buff._lifeTime)
			{
				ret = false;	//going to be discarded. so req to reset tmp mods
			}
			SkRuntimeInfo.Current = null;
			return ret;
		}
		public void TryExecTmp(ISkAttribs dst, int idxToMod)
		{
			if(_buff._mods!= null)						_buff._mods.TryExecTmp(dst, _paraCaster, _paraTarget, _para as ISkAttribsModPara, idxToMod, _n);
		}
		public bool OnDiscard(ISkAttribs dst)
		{
			if(_buff._mods!= null)						_buff._mods.ReqExecTmp(dst);
			if(_buff._effEnd != null)					_buff._effEnd.Apply(Target, this);
			return true;		// for facination of predicate expression
		}
		// Helper funx
		public bool MatchID(int id)
		{
			return _buff._id == id;
		}
	}
}

