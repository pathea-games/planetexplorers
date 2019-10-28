using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using System.Linq;

namespace SkillSystem
{
	public class ItemToPack
	{
		public int _id;
		public int _cnt;
		public ItemToPack(int id, int cnt = 1)
		{
			_id = id;
			_cnt = cnt;
		}
	}
	public class SkPackage : PackBase
	{
		internal static List<SkBuffInst> _tmpBuffs = new List<SkBuffInst>();

		internal SkAttribs _parentAttribs;
		internal List<SkBuffInst> _buffs = new List<SkBuffInst>(); // Spcial code for buffs

		public SkPackage(SkAttribs parentAttribs){	_parentAttribs = parentAttribs;		}
		public void ExecBuffs()
		{
			int n = _buffs.Count;
			if (n <= 0)
				return;

			_tmpBuffs.Clear ();
			_tmpBuffs.AddRange (_buffs);// use tmp array to avoid from error occuring on removing buff
			int idx;
			for(int i = n-1; i >= 0; i--)
			{
				idx = _buffs[i] == _tmpBuffs[i] ? i : _buffs.IndexOf(_tmpBuffs[i]);
				if(idx >= 0)	// Exist check
				{
					if(!_tmpBuffs[i].Exec(_parentAttribs) && _tmpBuffs[i].OnDiscard(_parentAttribs))
					{
						_buffs.RemoveAt(idx);
					}
				}
			}
		}
		public void ExecTmpBuffs(int idxToExec)
		{
			int n = _buffs.Count;
			if (n <= 0)
				return;

			_tmpBuffs.Clear ();
			_tmpBuffs.AddRange (_buffs);// use tmp array to avoid from error occuring on removing buff
			int idx;
			for(int i = n-1; i >= 0; i--)
			{
				idx = _buffs[i] == _tmpBuffs[i] ? i : _buffs.IndexOf(_tmpBuffs[i]);
				if(idx >= 0)	// Exist check
				{
					_tmpBuffs[i].TryExecTmp(_parentAttribs, idxToExec);
				}
			}
		}	
		// Override
		protected override void PushIn(params int[] ids)
		{
			List<ItemToPack> lstItems = new List<ItemToPack>();
			int n = ids.Length;
			for(int i = 0; i < n;)
			{
				if(IsBuff(ids[i]))	{		SkBuffInst.Mount(this, SkBuffInst.Create(ids[i], null, _parentAttribs)); i += 2;} //Ignore cnt
				else				{		lstItems.Add(new ItemToPack(ids[i], ids[i+1]));	i += 2;							}
			}
			if(lstItems.Count > 0)
			{
				_parentAttribs.OnPutInPakAttribs(lstItems);
			}
		}
		protected override void PopOut(params int[] ids)
		{
			int n = ids.Length;
			for(int i = 0; i < n;)
			{
				if(IsBuff(ids[i]))	{		SkBuffInst.Unmount(this, (SkBuffInst buffInst)=>buffInst.MatchID(ids[i])); i++;	}
				else				{		Debug.LogError("[SkPackablAttribs]Unsupport id to minus:"+ids[i]);	i += 2;		}
			}
		}

		internal static bool IsBuff(int id)	{	return id > 10000000;	}
	}
	// All properties, items to get, and skills
	public partial class SkAttribs : ISkAttribs
	{
		public const int MinAttribsCnt = 1;

		ISkEntity _entity;
		IExpFuncSet _expFunc;
		IList<float>_raws;
		IList<float>_sums;
		ModFlags	_dirties;
		SkPackage	_pack;

		int _nAttribs;

		public ISkEntity	entity{get{  return _entity;  }}
		public IExpFuncSet	expFunc{get{ return _expFunc; }}
		public IList<float> raws{get{ return _raws;	}}
		public IList<float> sums{get{ return _sums;	}}
		public IList<bool>	modflags{ get{ return (IList<bool>)_dirties;	}}
		public PackBase   	pack{get{ return (PackBase)_pack;				} 
								 set{ _pack = (SkPackage)value;				}}
		// paras for buff mods
		public float		buffMul{get;set;}
		public float		buffPreAdd{get;set;}
		public float		buffPostAdd{get;set;}
		public event Action<int,float, float> 	_OnAlterNumAttribs;		// para:(index, oldValue, newValue) clamping and other calculation such as damage to hp
		public event Action<List<ItemToPack>> 	_OnPutInPakAttribs;		// handler for putting somethings(item/buff) into this attribs pak... 
		bool _numAttribsEventEnable = true;
		bool _pakAttribsEventEnable = true;
		public void OnAlterNumAttribs(int idx, float oldValue, float newValue)
		{		
			if(_OnAlterNumAttribs!=null && _numAttribsEventEnable)	
				_OnAlterNumAttribs(idx, oldValue, newValue);
		}	// event can not be invoked outside
		public void OnPutInPakAttribs(List<ItemToPack> ids)
		{
			if(_OnPutInPakAttribs!=null && _pakAttribsEventEnable)
				_OnPutInPakAttribs(ids);	
		}

		public SkAttribs(ISkEntity ent, int nAttribs)
		{
			_entity = ent;
			_expFunc = new ExpFuncSet(this);

			_nAttribs = nAttribs;
			_dirties = new ModFlags(nAttribs);
			_raws = new NumList(nAttribs, (n,i,v)=>RawSetter(i, v));
			_sums = new NumList(nAttribs, (n,i,v)=>SumSetter(i, v));
			_pack = new SkPackage(this);
			AddToSendDValue(Pathea.AttribType.Hp);
		}
		public SkAttribs(ISkEntity ent, SkAttribs baseAttribs, bool[] masks)
		{
			_entity = ent;
			_expFunc = new ExpFuncSet(this);

			int nAttribs = baseAttribs._raws.Count;
			_nAttribs = nAttribs;
			_dirties = new ModFlags(nAttribs);
			_raws = new NumListWithParent(baseAttribs._raws as NumList, masks, nAttribs, (n,i,v)=>RawSetter(i, v));
			_sums = new NumListWithParent(baseAttribs._sums as NumList, masks, nAttribs, (n,i,v)=>SumSetter(i, v));
			_pack = new SkPackage(this);
			AddToSendDValue(Pathea.AttribType.Hp);
		}
		public SkAttribs(ISkEntity ent, List<int> idxList, List<float> valList)
		{
			_entity = ent;
			_expFunc = new ExpFuncSet(this);

			int maxIdx = 0;
			int cnt = idxList.Count;
			for(int j = 0; j< cnt; j++)
			{
				if(idxList[j] > maxIdx)	maxIdx = idxList[j];
			}
			_nAttribs = maxIdx+1;
			_dirties = new ModFlags(_nAttribs);
			_raws = new NumList(_nAttribs, (n,i,v)=>RawSetter(i, v));
			_sums = new NumList(_nAttribs, (n,i,v)=>SumSetter(i, v));
			_pack = new SkPackage(this);

			for(int j = 0; j < cnt; j++)
			{
				int idx = idxList[j];
				_sums[idx] = _raws[idx] = valList[j];
			}
			AddToSendDValue(Pathea.AttribType.Hp);
		}

		// Callbacks and event handers
		private void RawSetter(int idx, float v)
		{
			NumList r = (NumList)_raws;
			r.Set(idx, v);
			_dirties[idx] = true;
		}
		private void SumSetter(int idx, float v)
		{
			NumList s = (NumList)_sums;
			float oldValue = s.Get(idx);
			s.Set(idx, v);
			OnAlterNumAttribs(idx, oldValue, v);
		}

		// Methods
		public void EnableNumAttribsEvent(){	_numAttribsEventEnable = true;	}
		public void DisableNumAttribsEvent(){	_numAttribsEventEnable = false;	}
		public SkEntity GetModCaster(int idx){	return _dirties.GetCaster(idx);	}
        public SkEntity GetNetModCaster(int idx)
        {
            if(Pathea.EntityMgr.Instance.Get(_netCaster) != null)
                return Pathea.EntityMgr.Instance.Get(_netCaster).skEntity;
            return null;
        }
        public void Update(SkBeModified beModified)
		{
			_pack.ExecBuffs();
			int nAttribs = _raws.Count;
			for(int i = 0; i < nAttribs; i++)
			{
				if(_dirties[i])
				{
					buffMul = 1.0f;
					buffPreAdd = 0.0f; 
					buffPostAdd = 0.0f;
					_pack.ExecTmpBuffs(i);
					_sums[i] = buffMul*(_raws[i]+buffPreAdd)+buffPostAdd;
					SkEntity entity = GetModCaster(i);
					int casterId = 0;
					if(entity != null)
					{
						casterId = entity.GetId();
					}
					_dirties[i] = false;
					beModified.indexList.Add(i);
					beModified.valueList.Add(_sums[i]);
					beModified.casterIdList.Add(casterId);
				}
			}
		}
		public void Serialize(BinaryWriter w)
		{
			int nAttribs = _raws.Count;
			for(int i = 0; i < nAttribs; i++)
			{
				w.Write(_raws[i]);
			}
		}
		public void Deserialize(BinaryReader r)
		{
			DisableNumAttribsEvent();
			// vars for buff mods and buffs would not be serialized			
			int nAttribs = _raws.Count;
			for(int i = 0; i < nAttribs; i++)
			{
				_sums[i] = _raws[i] = r.ReadSingle();
			}
			_dirties.Clear();
			EnableNumAttribsEvent();
		}
	}
}