//#define USE_TRIGGER_FOR_SKILL	// now use defense trigger system
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using SkillSystem;
using Pathea.Projectile;

namespace Pathea
{
    public class HPChangeEventDataMan : Pathea.PeSingleton<HPChangeEventDataMan>, IHPEventData, IPesingleton
	{
        Stack<HPChangeEventData> _datas;

        void IPesingleton.Init()
        {
			_datas = new Stack<HPChangeEventData>();
		}

		public HPChangeEventData Pop()
		{
            //lz-2016.11.07 ±ÜÃâ¿Õ¶ÔÏó±¨´í
			if(null!=_datas&&_datas.Count > 0)
				return _datas.Pop();
			return null;
		}
		
		public void OnHpChange(SkEntity self, SkEntity caster, float hpChange)
		{
			if(null == self)
				return;
			SkAliveEntity skAlive = self as SkAliveEntity;
			if(null != skAlive)
			{
				PeTrans trans = skAlive.Entity.peTrans;
				CommonCmpt common = skAlive.Entity.commonCmpt;
				if(null != trans)
				{
					HPChangeEventData data = new HPChangeEventData();
					data.m_Self = self;
					data.m_HPChange = hpChange;
					data.m_Transfrom = trans.trans;
					data.m_Proto = (null != common && null != common.entityProto)?common.entityProto.proto:EEntityProto.Doodad;
                    data.m_AddTime = Time.realtimeSinceStartup;
                    _datas.Push(data);
				}
			}
		}
	}

	public class SkAliveEntity : PESkEntity , IPeCmpt, IPeMsg, IDigTerrain, ISkSubTerrain
	{
		const int 		Version = 3;

		ViewCmpt		m_View;
		PeTrans         m_Trans;
		AnimatorCmpt 	m_Animator;
		Motion_Beat		m_Beat;
		Motion_Equip 	m_MotionEquip;
		Motion_Live		m_Live;
		MotionMgrCmpt	m_MotionMgr;


		//Auto Recover
		bool m_Init = false;

        Dictionary<Collider, List<Collider>> m_CollisionEntities;

		public event Action<int> evtOnBuffAdd;
		public event Action<int> evtOnBuffRemove;
		
		public override void ApplyRepelEff (SkRuntimeInfo info)
		{
			base.ApplyRepelEff(info);
			SkInst inst = info as SkInst;
			if (inst != null) {
				ApplyForce (inst);
			} else {
				Debug.LogError("[ApplySpEff] Unsupported SkRuntimeInfo type");
			}
		}

		public override void OnBuffAdd (int buffId)
		{
			if(null != evtOnBuffAdd)
				evtOnBuffAdd(buffId);
		}

		public override void OnBuffRemove (int buffId)
		{
			if(null != evtOnBuffRemove)
				evtOnBuffRemove(buffId);
		}
		
		public override void CondTstFunc (SkFuncInOutPara funcInOut)
		{
			base.CondTstFunc (funcInOut);

			Entity.SendMsg(EMsg.Skill_CheckLoop, funcInOut);
			if(funcInOut._ret && IsController())
			{
				if(funcInOut._inst._target != null)
					SendBLoop(funcInOut._inst.SkillID,funcInOut._inst._target.GetId(),funcInOut._ret);
				else
					SendBLoop(funcInOut._inst.SkillID,0,funcInOut._ret);
			}
			else
			{
				SetCondRet(funcInOut);
			}
		}
		
		public override void ApplyEmission(int emitId, SkRuntimeInfo info)
		{
			base.ApplyEmission(emitId, info);

			ProjectileBuilder.Instance.Register(emitId, transform, info, 0, false);
		}
		
		public override void ApplyEff(int effId, SkRuntimeInfo info)
		{
			base.ApplyEff(effId, info);
			if(null != m_Trans)
				Pathea.Effect.EffectBuilder.Instance.RegisterEffectFromSkill(effId, info, m_Trans.existent);
		}

		public override void ApplySe(int seId, SkRuntimeInfo info)
        {
			base.ApplySe(seId, info);
			if (m_Trans != null && !m_Trans.Equals (null)) {
				AudioManager.instance.Create (m_Trans.position, seId);
			}
        }

		public override void ApplyCamEff(int camEffId, SkRuntimeInfo info)
		{
			base.ApplyCamEff(camEffId, info);
			PeCamera.ApplySkCameraEffect(camEffId, this);
		}

        public override Collider GetCollider(string name)
        {
			if (m_View is BiologyViewCmpt)
			{
				return (m_View as BiologyViewCmpt).GetModelCollider(name);
			}
            return null;
        }
		
		//public override void TstColWithName(SkExternalFuncPara para)
		//{
		//    //base.TstColWithName(para);
		//}

		protected override void OnPutInPak (List<ItemToPack> itemsToPack)
		{
			base.OnPutInPak (itemsToPack);
			if (Entity.packageCmpt != null)
			{
				foreach(ItemToPack item in itemsToPack)
				{
					if(PeGameMgr.IsMulti)
					{
						if(_net != null)
							_net.RPCServer(EPacketType.PT_Test_AddItem,item._id,item._cnt);
					}
					else
					{
                        NpcPackageCmpt npcpk = Entity.packageCmpt as NpcPackageCmpt;
                        if (npcpk != null)
                        {
                            npcpk.AddToHandin(item._id, item._cnt);
                        }
                        else
						  Entity.packageCmpt.Add(item._id, item._cnt);
					}
				}
			}
		}

		#region public interface
		private SkEntity m_SkParent = null;
		public SkEntity SkParent
		{
			get
			{
				return m_SkParent;
			}
			set
			{
				m_SkParent = value;
				m_UseParentMasks = new bool[(int)AttribType.Max];
				for (int i=0;i<m_UseParentMasks.Length;i++)
					m_UseParentMasks[i] = true;
			}
		}
		
		private bool[] m_UseParentMasks = null;
		public bool[] UseParentMasks{ get{ return m_UseParentMasks;}}
		public void SetUseParentMasks(AttribType type,bool value)
		{
			if (m_UseParentMasks == null)
				return;
			if ((int)type < m_UseParentMasks.Length)
				m_UseParentMasks[(int)type] = value;
		}
		#endregion
		
#if USE_TRIGGER_FOR_SKILL
        void AddCollisionSkEntity(Collider c1, Collider c2)
        {
            if (!m_CollisionEntities.ContainsKey(c1))
                m_CollisionEntities.Add(c1, new List<Collider>());

            if (!m_CollisionEntities[c1].Contains(c2))
                m_CollisionEntities[c1].Add(c2);
        }

        void RemoveCollisionSkEntity(Collider c1, Collider c2)
        {
            if (m_CollisionEntities.ContainsKey(c1)
                && m_CollisionEntities[c1].Contains(c2))
            {
                m_CollisionEntities[c1].Remove(c2);
            }
        }

		void OnColliderEnter(Collider self, Collider other)
		{
            if (other.gameObject.layer != Pathea.Layer.Damage
                && other.gameObject.layer != Pathea.Layer.GIEProductLayer)
                return;

			if (!other.transform.IsChildOf(transform) && self.transform.IsChildOf(transform))
			{
                SkEntity entity = other.transform.GetComponentInParent<SkEntity>();
                if(entity != null)
                {
                    PESkEntity skEntity = entity as PESkEntity;
                    if (skEntity == null || !skEntity.isDead)
                    {
//                        CollisionCheck(self, other);
                        //AddCollisionSkEntity(self, other);
                    }
                }
			}
		}
		
		void OnColliderExit(Collider self, Collider other)
		{
            if (other.gameObject.layer != Pathea.Layer.Damage
                && other.gameObject.layer != Pathea.Layer.GIEProductLayer)
                return;

            if (!other.transform.IsChildOf(transform) && self.transform.IsChildOf(transform))
            {
                SkEntity entity = other.transform.GetComponentInParent<SkEntity>();
                if (entity != null)
                {
                    //RemoveCollisionSkEntity(self, other);
                }
            }
		}		
#endif
		
        PeEntity mEntity;
        public PeEntity Entity
        {
            get
            {
                if (mEntity == null)
                {
                    mEntity = GetComponent<PeEntity>();
                }
                return mEntity;
            }
        }
		void Start()
		{
			if(!m_Init)
				InitSkEntity();

			Entity.AddMsgListener(this);

            m_CollisionEntities = new Dictionary<Collider, List<Collider>>();

			m_View = Entity.viewCmpt;
			m_Trans = Entity.peTrans;
			m_Animator = Entity.animCmpt;
			m_Beat = Entity.GetCmpt<Motion_Beat>();
			m_MotionMgr = Entity.motionMgr;
			m_MotionEquip = Entity.GetCmpt<Motion_Equip>();
			m_Live = Entity.GetCmpt<Motion_Live>();
			onHpChange += OnHpChange;
			onSkillEvent += OnTargetSkill;
			onTranslate += OnTranslate;
			Invoke("CheckInitAttr", 0.5f);
		}
        
        public virtual void Serialize(BinaryWriter w)
		{
#if UNITY_EDITOR
            if (_attribs == null)
            {
                Debug.Log("<color=yellow> entity:" + Entity + ", id:" + Entity.Id + ", _attribs is null</color>");
            }
#endif
			w.Write(Version);
			_attribs.Serialize(w);
//			if(null != m_InitBuffList)
//			{
//				w.Write(m_InitBuffList.Length);
//				foreach(int buffID in m_InitBuffList)
//					w.Write(buffID);
//			}
//			else
//				w.Write((int)0);
		}

		public virtual void Deserialize(BinaryReader r)
		{
			if(!m_Init)
				InitSkEntity();
			int readVersion = r.ReadInt32();

			if(readVersion == 2)
			{
				_attribs.DisableNumAttribsEvent();
				for(int i = 0; i < (int)AttribType.HPRecover; i++)
					_attribs.sums[i] = _attribs.raws[i] = r.ReadSingle();
				_attribs.EnableNumAttribsEvent();

				SetAttribute(AttribType.HPRecover, 0.01f);
			}
			else
				_attribs.Deserialize(r);

			if(1 == readVersion)
			{
				int buffLength = r.ReadInt32();
				m_InitBuffList = new int[buffLength];
				for(int i = 0; i < buffLength; i++)
					m_InitBuffList[i] = r.ReadInt32();
			}

			EntityProto entityProto = Entity.entityProto;
			if(null != entityProto)
			{
				switch(entityProto.proto)
				{
				case EEntityProto.Monster:
					MonsterProtoDb.Item monsterPDB = MonsterProtoDb.Get(entityProto.protoId);
					if(null != monsterPDB)
						m_InitBuffList = monsterPDB.initBuff;
					break;
				case EEntityProto.Npc:
					NpcProtoDb.Item npcPDB = NpcProtoDb.Get(entityProto.protoId);
					if(null != npcPDB)
						m_InitBuffList = npcPDB.InFeildBuff;
					break;
				case EEntityProto.Player:
					PlayerProtoDb.Item playerPDB = PlayerProtoDb.Get();
					if(null != playerPDB)
						m_InitBuffList = playerPDB.initBuff;
					break;
				case EEntityProto.RandomNpc:
					PlayerProtoDb.Item RnpcPDB = PlayerProtoDb.GetRandomNpc();
					if(null != RnpcPDB)
						m_InitBuffList = RnpcPDB.InFeildBuff;
					break;
				}
			}
		}

        #region implement IPeCmpt
        void IPeCmpt.Serialize(BinaryWriter w)
        {
            Serialize(w);
        }

        void IPeCmpt.Deserialize(BinaryReader r)
        {
            Deserialize(r);
        }

        string IPeCmpt.GetTypeName()
        {
            return GetType().Name;
        }
		#endregion
		
		public void InitSkEntity()
		{
			m_Init = true;
			InitEntity(m_SkParent,m_UseParentMasks);
		}
		
		public void OnMsg(EMsg msg, params object[] args)
		{
			switch (msg)
			{
#if USE_TRIGGER_FOR_SKILL
            case EMsg.View_Model_AttachJoint:
                GameObject injuredBuild = args[0] as GameObject;
                PETrigger.AttachTriggerEvent(injuredBuild, OnColliderEnter, null, null);
                break;
            case EMsg.View_Model_DeatchJoint:
                GameObject injuredDestroy = args[0] as GameObject;
                PETrigger.DetachTriggerEvent(injuredDestroy, OnColliderEnter, null, null);
                break;
            case EMsg.View_Model_Build:
				Profiler.BeginSample("View_Model_Build_SkAlive");
				GameObject modelBuild = args[0] as GameObject;
                PETrigger.AttachTriggerEvent(modelBuild, OnColliderEnter, null, null);
				Profiler.EndSample();
                break;
            case EMsg.View_Model_Destroy:
                GameObject modelDestroy = args[0] as GameObject;
                PETrigger.DetachTriggerEvent(modelDestroy, OnColliderEnter, null, null);
                break;
#endif
			case EMsg.State_Revive:
				DispatchReviveEvent();
				SetAutoBuffActive(true);
				isDead = false;
				break;
			}
		}
		
		#region SkEntityPE.ISkEntityHandler
		public override void ApplyAnim (string anim, SkRuntimeInfo info)
		{
			base.ApplyAnim (anim, info);
			if (anim == "Knock")
			{
				SkInst inst = info as SkInst;
				if(inst != null){
					if (m_View != null && inst._colInfo != null && inst._colInfo.hitTrans != null && inst._colInfo.hitTrans.GetComponent<Rigidbody>() != null)
					{
						//Vector3 direction = inst._colTarget.transform.position - inst._colCaster.transform.position;
						RagdollHitInfo hitInfo = PETools.PE.CapsuleHitToRagdollHit(inst._colInfo);
						if (m_View is BiologyViewCmpt) (m_View as BiologyViewCmpt).ActivateRagdoll(hitInfo, true);
					}
				}else{
					Debug.LogError("[ApplyAnim] Unsupported SkRuntimeInfo type");
				}
			}
			else
			{
				if (m_Animator != null)
				{
					m_Animator.SetTrigger(anim);
				}
			}
		}
		
		void ApplyForce(SkInst inst)
		{
			//TODO:llj
			if (null != m_Beat)
			{
				if(PETools.PEUtil.CanDamage (inst.Caster, inst.Target))
					m_Beat.Beat(inst.Target, (null != inst._colInfo) ? inst._colInfo.hitTrans : null, inst._forceDirection, inst._forceMagnitude);
			}
		}

		public override void OnHurtSb (SkInst inst, float dmg)
		{
			base.OnHurtSb (inst, dmg);
			if(m_MotionEquip!= null && null != m_MotionEquip.Weapon && null != m_MotionEquip.Weapon.ItemObj)
				Entity.SendMsg(EMsg.Battle_EquipAttack, m_MotionEquip.Weapon.ItemObj);
		}

		public override void OnGetHurt (SkInst inst, float dmg)
		{
			base.OnGetHurt (inst, dmg);
			Entity.SendMsg(EMsg.Battle_BeAttacked, dmg, inst.Caster);
		}
		
		public override void GetCollisionInfo(out List<KeyValuePair<Collider,Collider>> colPairs)
		{
			//TODO : coding
            colPairs = new List<KeyValuePair<Collider, Collider>>();
            foreach (KeyValuePair<Collider, List<Collider>> kvp in m_CollisionEntities)
            {
                foreach (Collider collider in kvp.Value)
                {
                    if(kvp.Key != null && collider != null)
                    {
                        colPairs.Add(new KeyValuePair<Collider, Collider>(kvp.Key, collider));
                    }
                }
            }
		}

		void EventFunc (string para)
		{
			Entity.SendMsg(EMsg.Skill_Event, para);
		}

		public override Transform GetTransform ()
		{
			return m_Trans != null ? m_Trans.existent : null;
		}
		#endregion

		#region IDigTerrain implementation

		public IntVector4 digPosType
		{
			get 
			{
				if(null != m_MotionMgr)
				{
					if(m_MotionMgr.IsActionRunning(PEActionType.Dig) && null != m_MotionEquip)
					{
						Action_DigTerrain actionDigTerrain = m_MotionMgr.GetAction<Action_DigTerrain>();
						if(null != actionDigTerrain)
							return new IntVector4(actionDigTerrain.digPos, 1);
					}
					else if(m_MotionMgr.IsActionRunning(PEActionType.SwordAttack))
					{
						Action_SwordAttack actionSwordAttack = m_MotionMgr.GetAction<Action_SwordAttack>();
						if(null != actionSwordAttack)
							return new IntVector4(actionSwordAttack.GetHitPos(), 0);
					}
					else if(m_MotionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
					{
						Action_TwoHandWeaponAttack actionAttack = m_MotionMgr.GetAction<Action_TwoHandWeaponAttack>();
						if(null != actionAttack)
							return new IntVector4(actionAttack.GetHitPos(), 0);
					}
                    else
                    {
                        if (Entity.peTrans != null)
                            return new IntVector4(Entity.peTrans.forwardCenter, 0);
                        else
                            return new IntVector4(Entity.position, 0);
                    }
				}

				return IntVector4.Zero;
			}
		}

		#endregion

		#region ISkSubTerrain implementation

		public GlobalTreeInfo treeInfo 
		{
			get 
			{
				if(null != m_MotionMgr)
				{
					if(m_MotionMgr.IsActionRunning(PEActionType.Fell))
					{
						Action_Fell fell = m_MotionMgr.GetAction<Action_Fell>();
						if(null != fell)
							return fell.treeInfo;
					}

					if(null != m_Live && m_MotionMgr.IsActionRunning(PEActionType.Gather))
					{

						return m_Live.gather.treeInfo;
					}
				}

				return null;
			}
		}

		#endregion


		void OnHpChange(SkEntity caster, float hpChange)
		{
			if(null != Entity)
				Entity.SendMsg(EMsg.Battle_HPChange, caster, hpChange);
		}

		void OnTargetSkill(SkEntity caster)
		{
			if(null != Entity)
				Entity.SendMsg(EMsg.Battle_TargetSkill, caster);
		}

		void OnTranslate(Vector3 pos)
		{
			if(null != Entity)
				Entity.SendMsg(EMsg.Trans_Pos_set,pos);
		}

		public void OnDeathProcessBuff(SkEntity cur, SkEntity caster)
		{
			if(caster == null || cur == null )
				return;
			SkAliveEntity killer = caster as SkAliveEntity;
			if(killer == null)
            {
                if(caster is SkProjectile)
                {
                    killer = (caster as SkProjectile).GetSkEntityCaster() as SkAliveEntity;
                }
                if(killer == null)
                    return;
            }
				
			SkAliveEntity victem = cur as SkAliveEntity;
			if(victem == null)
				return;
			MonsterProtoDb.Item protoItem = MonsterProtoDb.Get(victem.Entity.entityProto.protoId);
			if(protoItem == null || protoItem.deathBuff.Length == 0)
				return;
            if(killer.Entity.NpcCmpt != null )
            {
                if (killer.Entity.NpcCmpt.Master != null && killer.Entity.NpcCmpt.Master.Entity != null && killer.Entity.NpcCmpt.Master.Entity.aliveEntity != null)
                    killer = killer.Entity.NpcCmpt.Master.Entity.aliveEntity;
            }
			if(PeGameMgr.IsMulti)
			{
				if(killer.IsController())
				{
					string[] args1 = protoItem.deathBuff.Split(',');
					int buffid =Convert.ToInt32(args1[0]);
					List<int> atrtype = new List<int>();
					List<float> atrvalue = new List<float>();
					for(int i = 1; i < args1.Length; i = i + 2)
					{
						atrtype.Add(Convert.ToInt32(args1[i]));
						atrvalue.Add(Convert.ToSingle(args1[i+1]));
					}
					if(atrtype.Count > 0 && atrvalue.Count > 0)
					{
						//remove old buff
						SkEntity.UnmountBuff(killer, buffid);
						//add buff
						SkEntity.MountBuff(killer, buffid,atrtype,atrvalue);                           
					}
				}
			}
			else
			{
				string[] args1 = protoItem.deathBuff.Split(',');
				int buffid =Convert.ToInt32(args1[0]);
				List<int> atrtype = new List<int>();
				List<float> atrvalue = new List<float>();
				for(int i = 1; i < args1.Length; i = i + 2)
				{
					atrtype.Add(Convert.ToInt32(args1[i]));
					atrvalue.Add(Convert.ToSingle(args1[i+1]));
				}
				if(atrtype.Count > 0 && atrvalue.Count > 0)
				{
					//remove old buff
					SkEntity.UnmountBuff(killer, buffid);
					//add buff
					SkEntity.MountBuff(killer, buffid,atrtype,atrvalue);                           
				}
			}
			victem.deathEvent -= OnDeathProcessBuff;
		}
    }
}
