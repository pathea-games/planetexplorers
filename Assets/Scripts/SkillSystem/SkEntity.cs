using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkillSystem
{
	// base class for skill caster and skill target
	public partial class SkEntity : MonoBehaviour, ISkEntity
	{
		// Interface
		public ISkAttribs attribs{ get{ return _attribs; } }

        Dictionary<int, int> specialHatredData;
        public Dictionary<int, int> SpecialHatredData
        {
            get
            {
                if (specialHatredData == null)
                    specialHatredData = new Dictionary<int, int>();
                return specialHatredData;
            }
            set { specialHatredData = value; }
        }
#region init_update
		internal SkAttribs _attribs;
		internal void UpdateAttribs()
		{ 	
			_beModified.Clear ();
			_attribs.Update(_beModified);		
		}

		public void Init(Action<int, float, float> onAlterAttribs, Action<List<ItemToPack>> onPutinPackage, SkEntity parent, bool[] useParentMasks)
		{
			_attribs = new SkAttribs(this, parent._attribs, useParentMasks);

			SkEntityAttribsUpdater.Instance.Register (this);
			if(onAlterAttribs != null) _attribs._OnAlterNumAttribs += onAlterAttribs;
			if(onPutinPackage != null) _attribs._OnPutInPakAttribs += onPutinPackage;
			//for net 
			_attribs._OnAlterNumAttribs += OnNetAlterAttribs;
		}
		public void Init(Action<int, float, float> onAlterAttribs, Action<List<ItemToPack>> onPutinPackage, int nAttribs = SkAttribs.MinAttribsCnt)
		{
			_attribs = new SkAttribs(this, nAttribs);

			SkEntityAttribsUpdater.Instance.Register (this);
			if(onAlterAttribs != null) _attribs._OnAlterNumAttribs += onAlterAttribs;
			if(onPutinPackage != null) _attribs._OnPutInPakAttribs += onPutinPackage;
			//for net 
			_attribs._OnAlterNumAttribs += OnNetAlterAttribs;
		}
		public void Init(SkAttribs attribs, Action<int, float, float> onAlterAttribs, Action<List<ItemToPack>> onPutinPackage)
		{
			_attribs = attribs;

			SkEntityAttribsUpdater.Instance.Register (this);
			if(onAlterAttribs != null) _attribs._OnAlterNumAttribs += onAlterAttribs;
			if(onPutinPackage != null) _attribs._OnPutInPakAttribs += onPutinPackage;
			//for net 
			_attribs._OnAlterNumAttribs += OnNetAlterAttribs;
		}
#endregion

#region Skill_Query
		public void CancelBuffById(int id){					_attribs.pack -= id;					}
		public void CancelSkillById(int id)
		{		
			SkInst.StopSkill((inst)=>{ return inst.Caster==this && inst.SkillID==id; });		
			_skCanLoop.Reset();
			if( IsController() )
				_net.RPCServer(EPacketType.PT_InGame_SKStopSkill,id,PlayerNetwork.mainPlayerId);
		}
		public void CancelAllSkills()
		{
			SkInst.StopSkill((inst)=>{ return inst.Caster==this; });		
			_skCanLoop.Reset();
			if( IsController() )
				_net.RPCServer(EPacketType.PT_InGame_SKStopSkill,-1/*Tmp use -1, id*/,PlayerNetwork.mainPlayerId);
		}
		public void CancelSkill(SkInst inst)
		{
			inst.Stop ();
			_skCanLoop.Reset();
			if( IsController() )
				_net.RPCServer(EPacketType.PT_InGame_SKStopSkill, inst.SkillID, PlayerNetwork.mainPlayerId);
		}
		public virtual SkInst StartSkill(SkEntity target, int id, ISkPara para = null, bool bStartImm = true)
		{
			SkInst inst = SkInst.StartSkill(this, target, id, para, bStartImm);
			if(inst != null)
			{
				if(IsController())
				{
					if(para is ISkParaNet)
						SendStartSkill(target,id,((ISkParaNet)para).ToFloatArray());
					else if(para == null)
						SendStartSkill(target,id);
					else
						Debug.LogError("error skill para");
				}
			}
			return inst;
		}
		public SkInst GetSkInst(int id)
		{
			return SkInst.GetSkill((inst)=>{ return inst.Caster==this&&inst.SkillID==id; });
		}
		public SkBuffInst GetSkBuffInst(int id)
		{
			SkPackage pack = _attribs.pack as SkPackage;
			if(pack != null)
			{
				for(int i = 0; i < pack._buffs.Count; i++)
					if(pack._buffs[i]._buff._id == id)
						return pack._buffs[i];
//				return pack._buffs.Find((inst)=>{ return inst._buff._id==id; });
			}
			return null;
		}
		public bool IsSkillRunning(int id, bool cdInclude = true){ 	return null != SkInst.GetSkill((inst)=>{ return inst.Caster==this && inst.SkillID==id && (cdInclude||inst.IsActive); }); }
		public bool IsSkillRunning(bool cdInclude = true){ 			return null != SkInst.GetSkill((inst)=>{ return inst.Caster==this && (cdInclude||inst.IsActive); }); }
		public bool IsSkillRunnable(int id){						return SkInst.IsSkillRunnable(this, id);						}
		public PackBase BuffAttribs {								get { return _attribs.pack; } 	set { _attribs.pack = value;	}		}
		public PackBase Pack{										get { return _attribs.pack; } 	set { _attribs.pack = value;	}		}
		public float GetAttribute(int type, bool bSum = true){		return bSum ? _attribs.sums[type] : _attribs.raws[type];}		
		public void SetAttribute(int type, float value, bool eventOff = true, bool bBoth = true)
		{
			if(eventOff)	_attribs.DisableNumAttribsEvent();

			if(bBoth)	_attribs.raws[type] = value;
			_attribs.sums[type] = value;

			if(eventOff)	_attribs.EnableNumAttribsEvent();
		}

		public void SetAttribute(int type, float value, bool eventOff , bool bRaw,int caster)
		{
			if(eventOff)	_attribs.DisableNumAttribsEvent();

            _attribs.SetNetCaster(caster);
            if (bRaw)
				_attribs.raws[type] = value;
			else
				_attribs.sums[type] = value;
            _attribs.SetNetCaster(0);


            if (eventOff)	_attribs.EnableNumAttribsEvent();
		}

		public SkEntity GetCasterToModAttrib(int idx)
		{
			return _attribs.GetModCaster(idx);
		}
        public SkEntity GetNetCasterToModAttrib(int idx)
        {
            return _attribs.GetNetModCaster(idx);
        }
#endregion

        // Skill instances with immediate action
        internal List<SkInst> _instsActFromExt = new List<SkInst>();
		public void RegisterInstFromExt(SkInst inst)
		{
			_instsActFromExt.Add(inst);
		}
		public void UnRegisterInstFromExt(SkInst inst)
		{
			_instsActFromExt.Remove(inst);
		}
		// Collision test
		public void CollisionCheck(Collider selfCol, Collider otherCol)
		{
			Pathea.PECapsuleHitResult colInfo = new Pathea.PECapsuleHitResult ();
			colInfo.selfTrans = selfCol.transform;
			colInfo.hitTrans = otherCol.transform;
			// Other var evaluate
			int n = _instsActFromExt.Count;
			for(int i = 0; i < n; i++)
			{
				_instsActFromExt[i].ExecEventsFromCol(colInfo);
			}
		}
		public void CollisionCheck(Pathea.PECapsuleHitResult colInfo)
		{
			int n = _instsActFromExt.Count;
			for(int i = 0; i < n; i++)
			{
				_instsActFromExt[i].ExecEventsFromCol(colInfo);
			}
		}

		// static Utils
		public static explicit operator SkEntity(Collider col)
		{
			return PETools.PEUtil.GetComponent<SkEntity>(col.gameObject);
		}
		public static explicit operator SkEntity(Transform t)
		{
			return PETools.PEUtil.GetComponent<SkEntity>(t.gameObject);
		}
		// mount/unmount buff
		public static SkBuffInst MountBuff(SkEntity target, int buffId, List<int> idxList, List<float> valList)	// To save memory ,idxList should be increase step by step
		{
			int maxIdx = 0;
			int n = idxList != null ? idxList.Count : 0;
			for(int i = 0; i< n; i++)
			{
				if(idxList[i] > maxIdx)	maxIdx = idxList[i];
			}
			SkAttribs buffAttribs = new SkAttribs(null, maxIdx+1);
			for(int i = 0; i < n; i++)
			{
				int idx = idxList[i];
				buffAttribs.sums[idx] = buffAttribs.raws[idx] = valList[i];
			}

            //lz-2016.08.22 引导检测玩家进入buff
            Pathea.PeEntity entity = target.GetComponent<Pathea.PeEntity>();
            if (null != entity && entity.IsMainPlayer)
            {
                InGameAidData.CheckInBuff(buffId);
            }

            return SkBuffInst.Mount(target._attribs.pack as SkPackage, SkBuffInst.Create(buffId, buffAttribs, target._attribs));
		}
		public static void UnmountBuff(SkEntity target, int buffId)
		{
			//SkPackage pack = target._attribs.pack as SkPackage;
			SkBuffInst.Unmount(target._attribs.pack as SkPackage,  it=>it._buff._id == buffId);
		}
		public static void UnmountBuff(SkEntity target, SkBuffInst inst)
		{
			SkBuffInst.Unmount(target._attribs.pack as SkPackage, inst); 
		}
	}
}