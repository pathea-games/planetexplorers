//#define ATTACK_OLD
using UnityEngine;
using SkillSystem;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using Pathea;
using Pathea.Projectile;
using Behave.Runtime;
using Behave.Runtime.Action;
using Pathfinding;

namespace Pathea
{
	public class TargetCmpt : PeCmpt, IPeMsg
	{
		const float SwitchValue = 1.1f;
		const float Multiple = 3.0f;
		const float ThreatIntervalDistance = 0.5f;
		const float ThreatIntervalTime = 0.5f;

		public Action<PeEntity, PeEntity, float> HatredEvent;

		//float m_VisionRadius = 50.0f;
		//float m_VisionAngle = 75.0f;
		//float m_HearingRadius = 10.0f;

		bool m_FirstDamage;
        bool m_CanSearch;
        bool m_CanAttack;

		PeEntity m_Entity;
        SkEntity m_SkEntity;
		PeTrans m_Trans;
		AnimatorCmpt m_Animator;
		RequestCmpt m_Request;
		Motion_Equip m_Equipment;
		//IKAimCtrl m_IKAim;
		CommonCmpt m_Common;
        NpcCmpt m_Npc;
        //Motion_Move_Motor m_Motor;

        PEMonster m_Monster;

		List<Enemy> m_Enemies = new List<Enemy> ();
        List<PeEntity> _tmpEntities = new List<PeEntity>();
        List<PeEntity> _tmpReputationEntities = new List<PeEntity>();
        List<PeEntity> _corpses = new List<PeEntity>();
        List<PeEntity> _treats = new List<PeEntity>();
        List<PeEntity> _Melees = new List<PeEntity>();
        List<BTAction> _actions = new List<BTAction>();
        List<IAttack> _Attacks = new List<IAttack>();
        List<IWeapon> _Weapons = new List<IWeapon>();

        Enemy m_Enemy;
		Enemy m_Escape;
		Enemy m_Specified;

        IAttack m_Attack = null;

		PEVision[] m_Visions;
		PEHearing[] m_Hears;

		float m_EscapeBase;
		PeTrans m_mainPlayerTran;

        long m_FrameCount;

		public  static float Combatpercent = 1.5f;
		public  static float HPpercent = 0.4f;
		public  static float Atkpercent = 0.4f;
		public  static float Defpercent = 0.2f;

        public Enemy enemy {
            get
            {
                if (!Enemy.IsNullOrInvalid(m_Enemy))
                    return m_Enemy;
                else
                    return null;
            }
        }

        PeEntity m_Afraid;
        public PeEntity Afraid { get { return m_Afraid; } set { m_Afraid = value; } }

        PeEntity m_Doubt;
        public PeEntity Doubt { get { return m_Doubt; } set { m_Doubt = value; } }

        PeEntity m_Chat;
        public PeEntity Chat { get { return m_Chat; } set { m_Chat = value; } }

        PeEntity m_Food;
        public PeEntity Food { get { return m_Food; } set { m_Food = value; } }

        PeEntity m_Treat;
        public PeEntity Treat { get { return m_Treat; } set { m_Treat = value; } }

		Vector3 m_HidePistion;
		public Vector3 HidePistion { get { return m_HidePistion; } }

		public float EscapeBase { set { m_EscapeBase = value; } get { return m_EscapeBase; } }

		float m_EscapeProp;
		public float EscapeProp { set { m_EscapeProp = value; } get { return m_EscapeProp; } }

		bool m_Scan = true;
		public bool Scan { set { m_Scan = value; } get{return m_Scan;}}

		bool m_IsAddHatred = true;
		public bool IsAddHatred { set { m_IsAddHatred = value; } get{return m_IsAddHatred;}}

		bool m_CanTransferHatred = true;
		public bool CanTransferHatred{ set{ m_CanTransferHatred = value;} get{return m_CanTransferHatred;} }

		bool m_CanActiveAttack = true;
		public bool CanActiveAttck { get { return m_CanActiveAttack;} set{ m_CanActiveAttack = value;}}

		bool m_UseTool = false;
		public bool UseTool {get {return m_UseTool;} set{m_UseTool = value;}}

		bool m_beSkillTarget = false;
		public bool beSkillTarget {get { return  m_beSkillTarget;}}

        public List<IAttack> Attacks { get { return _Attacks; } }

        public void SetActions(List<BTAction> actions)
        {
            _actions = actions;

            for (int i = 0; i < _actions.Count; i++)
            {
                if (_actions[i] != null && _actions[i] is BTAttackBase)
                {
                    foreach (KeyValuePair<string, object> kvp in _actions[i].GetDatas())
                    {
                        if (kvp.Value != null && kvp.Value is IAttack)
                        {
                            _Attacks.Add((IAttack)kvp.Value);
                        }
                    }
                }
            }
        }


        public bool ContainEnemy(Enemy enemy)
        {
            return m_Enemies.Contains(enemy);
        }

        public Enemy GetEnemy(PeEntity targetEntity)
        {
            return m_Enemies.Find(ret => ret != null && ret.isValid && ret.entityTarget == targetEntity);
        }

        public List<Enemy> GetEnemies ()
		{
			return m_Enemies;
		}

		public bool HasAnyEnemy ()
		{
			return m_Enemies.Count > 0;
		}

		public bool HasEnemy ()
		{
			return GetAttackEnemy () != null;
		}

        public bool ContainsMelee(PeEntity peEntity)
        {
            return _Melees.Contains(peEntity);
        }

        public void AddMelee(PeEntity peEntity,int n= 3)
        {
            if (!_Melees.Contains(peEntity) && _Melees.Count < n)
                _Melees.Add(peEntity);
        }

        public void RemoveMelee(PeEntity peEntity)
        {
            if (_Melees.Contains(peEntity))
                _Melees.Remove(peEntity);
        }

        public List<PeEntity> GetMelees()
        {
            return _Melees;
        }

        public int GetMeleeCount()
        {
            return _Melees.Count;
        }

        public bool HasHatred(PeEntity entity)
        {
            Enemy e = m_Enemies.Find(ret => ret != null && ret.entityTarget == entity);
            if (!Enemy.IsNullOrInvalid(e) && e.Hatred > PETools.PEMath.Epsilon)
                return true;

            return false;
        }

		public void ClearEscapeEnemy ()
		{
			m_Enemies.Remove (m_Escape);
			m_Escape = null;
		}

		public Enemy GetEscapeEnemy ()
		{
            if (IsDeath() || m_Entity.IsDarkInDaytime)
                return null;

			if (m_Common != null && m_Common.TDObj != null || m_Entity.IsSeriousInjury)
				return null;

            if (!Enemy.IsNullOrInvalid(m_Escape))
                return m_Escape;

			return null;
		}

        public Enemy GetEscapeEnemyUnit()
        {
            if (IsDeath())
                return null;

			if (m_Common != null && m_Common.TDObj != null)
				return null;

            if (!Enemy.IsNullOrInvalid(m_Escape))
                return m_Escape;

            return null;
        }

        public Enemy GetFollowEnemy ()
		{
            if (!Enemy.IsNullOrInvalid(m_Enemy))
                return null;
            else
                return m_Enemies.Find (ret => !Enemy.IsNullOrInvalid(ret) && ret.canFollowed);
		}

		public Enemy GetAfraidEnemy ()
		{
			return m_Enemies.Find (ret => ret != null && ret.canAfraid);
		}

		public Enemy GetThreatEnemy ()
		{
			return m_Enemies.Find (ret => ret != null && ret.canThreat);
		}

		public Enemy GetAttackEnemy ()
		{
            if(!Enemy.IsNullOrInvalid(m_Enemy))
			    return m_Enemy;

            return null;
		}

        public PeEntity GetAfraidTarget()
        {
            int playerID = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);

            for (int i = 0; i < _tmpReputationEntities.Count; i++)
            {
                if(_tmpReputationEntities[i] != null)
                {
                    int tmpPlayerID = (int)_tmpReputationEntities[i].GetAttribute(AttribType.DefaultPlayerID);
                    if(ReputationSystem.Instance.HasReputation(tmpPlayerID, playerID))
                    {
                        return _tmpEntities[i].GetComponent<PeEntity>();
                    }
                }
            }

            return null;
        }

        public PeEntity GetDoubtTarget()
        {
            int playerID = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);

            for (int i = 0; i < _tmpReputationEntities.Count; i++)
            {
                if (_tmpReputationEntities[i] != null)
                {
                    int tmpPlayerID = (int)_tmpReputationEntities[i].GetAttribute(AttribType.DefaultPlayerID);
                    if (ReputationSystem.Instance.HasReputation(tmpPlayerID, playerID))
                    {
                        return _tmpEntities[i].GetComponent<PeEntity>();
                    }
                }
            }

            return null;
        }

        public void ClearEnemy ()
		{
			int n = m_Enemies.Count;
			for(int i = 0; i < n; i++) {
				if (m_Enemies[i] != null) {
						m_Enemies[i].Dispose ();
				}
			}

            if(m_Enemy != null)
			    OnEnemyLost (m_Enemy);

			if(m_Npc != null)
				m_Npc.ClearLockedEnemies();

			m_Enemy = null;
			m_Enemies.Clear ();
		}

        public void AddDamageHatred(PeEntity argEntity, float hatred)
        {
            if (m_Entity == null || argEntity == null || argEntity == m_Entity || IsDeath())
                return;

            Enemy enemy = m_Enemies.Find(ret => ret != null && ret.entityTarget != null && ret.entityTarget == argEntity);
            if (enemy == null)
            {
                enemy = new Enemy(m_Entity, argEntity);
                AddEnemy(enemy);
            }

            enemy.OnDamage(hatred);

            if (HatredEvent != null)
                HatredEvent(m_Entity, argEntity, hatred);

            PeNpcGroup.Instance.OnCSAddDamageHaterd(argEntity, m_Entity, hatred);
        }

        public void AddSharedHatred (PeEntity argEntity, float hatred)
		{
            if (m_Entity == null || argEntity == null || argEntity == m_Entity || IsDeath())
                return;

            if (!CanAddDamageHatred(argEntity))
                return;

            if (m_Entity.NpcCmpt != null && !m_CanTransferHatred)
                return;

            Enemy enemy = m_Enemies.Find(ret => ret != null && ret.entityTarget != null && ret.entityTarget == argEntity);
            if (enemy != null)
                enemy.AddHatred(hatred);
            else
                AddEnemy(new Enemy(m_Entity, argEntity, hatred));

            if (HatredEvent != null)
                HatredEvent (m_Entity, argEntity, hatred);

			PeNpcGroup.Instance.OnCSAddDamageHaterd(argEntity,m_Entity,hatred);
		}

        public void CallHelp(float radius)
        {
            if (m_Entity == null)
                return;

            int playerID = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);

            List<PeEntity> entities = EntityMgr.Instance.GetEntitiesFriendly(m_Trans.position, radius, playerID, m_Entity.ProtoID, false, m_Entity);
            for (int i = 0; i < entities.Count; i++)
            {
                TargetCmpt target = entities[i].GetComponent<TargetCmpt>();
                if(target != null) target.CopyEnemies(this);
            }
        }

		public void CopyEnemies (TargetCmpt target)
		{
			List<Enemy> enemys = target.GetEnemies ();
			int n = enemys.Count;
			for(int i = 0; i < n; i++){
				Enemy enemy = enemys[i];
				float scale = (GetAttackEnemy () != null) ? 0.032f : (enemy.Equals (target.GetAttackEnemy ()) ? 0.25f : 0.125f);
				float value = Mathf.Clamp (enemy.Hatred * scale, 1.0f, 100.0f);

				AddSharedHatred (enemy.entityTarget, value);
			}
		}

        public void OnDamageMember(PeEntity entity, float damage)
        {
            AddSharedHatred(entity, damage * 0.5f);

            if(m_Entity != null && m_Entity.Group != null && m_Entity.Group.AlivePercent <= 0.5f)
            {
                if(UnityEngine.Random.value < 0.5f)
                    SetEscape(entity);
            }
        }

        public void OnTargetDiscover(PeEntity entity)
        {
            if (ContainsEnemy(entity))
                return;

            SelectEntity(entity);
        }

		public void TransferHatred (PeEntity argEntity, float hatred)
		{
			//传递加仇恨
			if(!m_CanTransferHatred || argEntity == null || m_Entity == null || argEntity == m_Entity || IsDeath())
				return ;

            if ((m_Npc != null && m_Npc.Battle == ENpcBattle.Passive))//|| m_Npc.Battle == ENpcBattle.Evasion
                return;

			if (PeGameMgr.IsMulti)
			{
				int argPlayerId = (int)argEntity.GetAttribute(AttribType.DefaultPlayerID);
				int selfPlayerId = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);

				if (ForceSetting.Instance.AllyPlayer(argPlayerId, selfPlayerId))
					return;
			}

            SkEntity skCaster = PEUtil.GetCaster(argEntity.skEntity);

            if (skCaster == null)
                return;

			PeEntity peCaster = skCaster.GetComponent<PeEntity>();

			float scale = (GetAttackEnemy () != null) ? 0.032f : 0.125f;

            AddSharedHatred(peCaster, hatred * scale);
		}

        public PeEntity GetReputation(ReputationSystem.ReputationLevel minLevel, ReputationSystem.ReputationLevel maxLevel)
        {
            for (int i = 0; i < _tmpEntities.Count; i++)
            {
                int p1 = (int)_tmpEntities[i].GetAttribute(AttribType.DefaultPlayerID);
                int p2 = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);

                if (ReputationSystem.Instance.HasReputation(p1, p2))
                {
                    ReputationSystem.ReputationLevel level = ReputationSystem.Instance.GetReputationLevel(p1, p2);
                    if(level <= maxLevel && level >= minLevel)
                        return _tmpEntities[i];
                }
            }

            return null;
        }

		public override void Start ()
		{
			base.Start ();

			m_FirstDamage = true;
            m_CanSearch = true;
            m_CanAttack = true;
			m_Entity = GetComponent<PeEntity> ();
			m_SkEntity = GetComponent<SkEntity> ();
            m_Trans = GetComponent<PeTrans> ();
			m_Animator = GetComponent<AnimatorCmpt> ();
			m_Request = GetComponent<RequestCmpt> ();
			m_Equipment = GetComponent<Motion_Equip> ();
			m_Common = GetComponent<CommonCmpt> ();
            m_Npc = GetComponent<NpcCmpt>();
            //m_Motor = GetComponent<Motion_Move_Motor>();

            PESkEntity peEntity = m_Entity.skEntity as PESkEntity;
			if (peEntity != null)
            {
				peEntity.deathEvent += OnDeath;
            }

            //StartCoroutine (SwitchAttack());
			StartCoroutine (ClearEnemyEnumerator ());
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            m_FrameCount++;

            if (m_FrameCount % 15 != 0)
                return;

            _tmpEntities.Clear();
            _tmpEntities .AddRange(GetMelees());
            for (int i=0;i<_tmpEntities.Count;i++)
            {
                if (_tmpEntities[i] == null || _tmpEntities[i].IsDeath() || !_tmpEntities[i].hasView)
                    RemoveMelee(_tmpEntities[i]);
            }
            //_Melees = _Melees.FindAll(ret => ret != null && !ret.IsDeath() && ret.hasView);

            if (!Entity.hasView || (Entity.netCmpt != null && !Entity.netCmpt.IsController))
                return;

            if (!Entity.Equals(PeCreature.Instance.mainPlayer))
            {
                CalculateEnemy();

                CalculateAllys();

                CalculateSectorField();
            }
            else
            {
                UpdateEnemies();
            }
        }

        void CalculateEnemy()
        {
			CollectEnemies ();

			UpdateEnemies ();

            SelectEnemy ();
        }

        void CalculateAllys()
		{
            Profiler.BeginSample("CollectAllys");
			if(m_Npc != null && m_Npc.IsFollower && !m_Npc.IsOnVCCarrier && !m_Npc.IsOnRail)
			{
				mAllys= m_Npc.Allys;

				if(mAllys != null)
				{
					//有队友已经在载具上或者轻轨上不放技能
					for(int i=0;i<mAllys.Count;i++)
					{
						if(mAllys[i].IsOnRail || mAllys[i].IsOnVCCarrier)
							return ;
					}
	
					for (int i = 0; i < mAllys.Count; i++)
					{
						NpcCmpt npc = mAllys [i];
						if (!npc.IsOnVCCarrier && !npc.IsOnRail && ConditionDecide (m_Npc, npc)) {
							m_Npc.Req_UseSkill ();
							m_Npc.NpcSkillTarget = npc;
							m_Npc.InAllys = true;
						}
					}
				}
			}
            Profiler.EndSample();
		}

		bool IsDeath ()
		{
			return m_Entity == null || m_Entity.IsDeath();
		}

        public  bool CanAttack()
        {
            return m_Entity != null && m_Entity.hasView && m_CanAttack && !m_Entity.IsSeriousInjury && !m_Entity.IsDarkInDaytime && GetEscapeEnemy() == null;
        }

		public void SetEnityCanAttack( bool canAttackOrNot)
		{
			m_CanAttack = canAttackOrNot;
		}

		public void SetCanAtiveWeapon(bool value)
		{
			m_CanActiveAttack = value;
		}

		bool ContainsEnemy (PeEntity entity)
		{
            for (int i = 0; i < m_Enemies.Count; i++)
            {
                if (m_Enemies[i] != null && m_Enemies[i].entityTarget == entity)
                    return true;
            }

            return false;
		}

		void AddEnemy (Enemy enemy)
		{
			if (IsDeath ())
				return;

			if (!m_Enemies.Contains (enemy)) {
				m_Enemies.Add (enemy);
			}
		}

        bool ContainsAction(Type type)
        {
            for (int i = 0; i < _actions.Count; i++)
            {
                if(type.IsInstanceOfType(_actions[i]))
                {
                    return true;
                }
            }

            return false;
        }

        void SetEscape(PeEntity escape)
        {
            if (escape != null && ContainsAction(typeof(BTEscape)) && !RandomDunGenUtil.IsInDungeon(Entity))
            {
                m_Escape = m_Enemies.Find(ret => !Enemy.IsNullOrInvalid(ret) && ret.entityTarget.Equals(escape));

                if (Entity.Group != null && !Enemy.IsNullOrInvalid(m_Escape))
                    Entity.Group.SetEscape(m_Entity, escape);
            }
        }

        public void SetEscapeEntity(PeEntity escape)
        {
            if (escape != null && ContainsAction(typeof(BTEscape)))
            {
                m_Escape = m_Enemies.Find(ret => !Enemy.IsNullOrInvalid(ret) && ret.entityTarget.Equals(escape));
            }
        }

        bool CanSelectEntity (PeEntity entity)
		{
            if (ContainsEnemy(entity))
                return false;

			int pid1 = (int)m_Entity.GetAttribute (AttribType.DefaultPlayerID);
            int pid2 = (int)entity.GetAttribute (AttribType.DefaultPlayerID);

            int cid1 = (int)m_Entity.GetAttribute (AttribType.CampID);
            int cid2 = (int)entity.GetAttribute (AttribType.CampID);

            return PEUtil.CanAttackReputation(pid1, pid2) 
                && ForceSetting.Instance.Conflict(pid1, pid2) 
                && Mathf.Abs(ThreatData.GetInitData(cid1, cid2)) > PETools.PEMath.Epsilon;
		}

		bool CanAddDamageHatred (PeEntity target)
		{
			if (!m_IsAddHatred)
				return false;

            if (!CanAttackCarrier(target))
                return false;

			int d1 = System.Convert.ToInt32 (m_Entity.GetAttribute (Pathea.AttribType.DamageID));
			int d2 = System.Convert.ToInt32 (target.GetAttribute (Pathea.AttribType.DamageID));

            return PEUtil.CanDamageReputation(m_Entity, target) && Pathea.DamageData.GetValue (d1, d2) > 0;
		}

        bool CanAttackCarrier(PeEntity entity)
        {
            if (entity == null)
                return false;

            if (m_Monster == null || !m_Monster.isAttackCarrier)
                return true;

            if (entity.carrier != null && entity.carrier is WhiteCat.HelicopterController)
                return true;

            return false;
        }

        bool CanAttackEntity(PeEntity entity)
        {
            //if (m_Entity.Group != null)
            //    return true;

            //if (entity.Group == null)
            //    return true;

            //if (entity.target != null && entity.target.EscapeBase >= 1.0f && entity.target.EscapeProp >= 1.0f)
            //    return true;

            //return false;

            return true;
        }

        /// <summary>
        /// 用于判断敌人阵营、声望、或者初始仇恨发生变化时，是否应该继续攻击
        /// </summary>
        /// <returns></returns>
        bool CanAttackEnemy(PeEntity entity)
        {
            //if (PeGameMgr.IsMulti)
            //    return true;

            int pid1 = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
            int pid2 = (int)entity.GetAttribute(AttribType.DefaultPlayerID);

            //int cid1 = (int)m_Entity.GetAttribute(AttribType.CampID);
            //int cid2 = (int)entity.GetAttribute(AttribType.CampID);

            int d1 = System.Convert.ToInt32(m_Entity.GetAttribute(Pathea.AttribType.DamageID));
            int d2 = System.Convert.ToInt32(entity.GetAttribute(Pathea.AttribType.DamageID));

            return PEUtil.CanDamageReputation(pid1, pid2)
                && ForceSetting.Instance.Conflict(pid1, pid2)
                && Pathea.DamageData.GetValue(d1, d2) != 0;//Mathf.Abs(ThreatData.GetInitData(cid1, cid2)) > PETools.PEMath.Epsilon;
        }

		List<NpcCmpt> mAllys = new List<NpcCmpt>();
		bool GetHpJudge(NpcCmpt Slef,NpcCmpt Target,int SkillId)
		{
			if(Slef != null)
			{
				float per = Slef.GetNpcChange_Hp(SkillId);
				if(per == 0)
					return true;
				
				float perTarg  = Target.NpcHppercent;
				return perTarg <= per;
			}
			return false;
		}

		bool  ConditionDecide(NpcCmpt SelfNpc,NpcCmpt targetNpc)
		{
			if(SelfNpc.GetReadySkill() == -1 )
				return false;

			int SkillId = SelfNpc.GetReadySkill();
			float Dis = Vector3.Distance(SelfNpc.NpcPostion,targetNpc.NpcPostion);
			float Range = SelfNpc.GetNpcSkillRange(SkillId);
			if(Range == -1)
				return false;
			return Dis<Range && GetHpJudge(SelfNpc,targetNpc,SkillId);
		}

		//float hideDistance = 5.0f;
		Vector3 direction = Vector3.back;
		Enemy mHideEnemy;
		List<Vector3> mdirs= new List<Vector3>();

		void hideDirction(Enemy enemie,Vector3 player)
		{
			mdirs.Add((player -enemie.position).normalized);
			return;
		}

		Vector3 betterHideDirtion(List<Vector3> dirs)
		{
			if(dirs.Count <=0)
				return Vector3.back;

			Vector3 betterdir = dirs[0];
			for(int i=1;i<dirs.Count;i++)
			{
				betterdir =(betterdir + dirs[i]).normalized;
			}
			return betterdir;
		}

		Vector3 calculateSteering(PeTrans selfTran,Vector3 targetPos,float max_velocity = 0.6f)
		{
			Vector3 cur_velocity = selfTran.forward;
//			Vector3 selfpos = selfTran.position;
			Vector3 desired_velocity = (targetPos - selfTran.position).normalized;
			Vector3 steering = (desired_velocity - cur_velocity).normalized * max_velocity;
			return  steering;
		}

		public float Steering_max_velocity(Vector3 PlayerPostion,Vector3 NpcPostion,float maxRadius = 6.0f,float minRadius = 4.0f)
		{
			
			float distance = PEUtil.SqrMagnitudeH(PlayerPostion, NpcPostion);
			if( distance> maxRadius * maxRadius)
				return 0.0f; 
			
			if(distance >minRadius *minRadius && distance < maxRadius * maxRadius)
				return (maxRadius -Mathf.Sqrt(distance))/2;
			
			if(distance <minRadius *minRadius)
				return 1.0f;
			
			return 0.0f;
		}

		Vector3 flee_steeringPos(Vector3 dirPos,Vector3 plyer,PeTrans npcTran,float max_velocity = 0.5f)
		{
			float distance = PEUtil.SqrMagnitudeH(dirPos, npcTran.position);
			if(distance <= 2.0f *2.0f)
				return dirPos;

			Vector3 slefPos = npcTran.position;
			//seek steering
			Vector3 cur_velocity = npcTran.forward;
//			Vector3 selfpos = npcTran.position;
			Vector3 desired_velocity = (dirPos - npcTran.position).normalized;
			Vector3 seek_steering = (desired_velocity - cur_velocity).normalized * max_velocity;

			//Vector3 seek_steering = calculateSteering(npcTran,dirPos,0.6f);

			//flee palyer
		//	Vector3 flee_player_steering = -calculateSteering(npcTran,plyer,Steering_max_velocity(plyer,npcTran.position));
//			Vector3 fee_desired_velocity = (plyer - npcTran.position).normalized;
//			Vector3 flee_player_steering = -(fee_desired_velocity - npcTran.forward).normalized * Steering_max_velocity(plyer,npcTran.position);
			//flee enemies
			Vector3 flee_enemy_steering = Vector3.zero;
			Vector3 flee_enemy_steering2 = Vector3.zero;
			float Max_fleeEneny_velocity = 0;
			for(int i=0;i<m_Enemies.Count;i++)
			{
				if(!m_Enemies[i].canAttacked)
					continue;

				distance = PEUtil.SqrMagnitudeH(m_Enemies[i].position, m_Trans.position);
				if(distance >= 10.0f*10.0f)
					continue;

				if(distance <=4.0f *4.0f && distance>2.0f*2.0f)
				{
					Max_fleeEneny_velocity =0.6f;
				}

				if(distance <= 2.0f*2.0f)
				{
					Max_fleeEneny_velocity = 4.0f;
				}
				flee_enemy_steering2 = -calculateSteering(npcTran,m_Enemies[i].position,Max_fleeEneny_velocity);
				flee_enemy_steering = flee_enemy_steering + flee_enemy_steering2;

			}
			return slefPos +(npcTran.forward +seek_steering + flee_enemy_steering).normalized *4.0f;
		}

		void CalculateSectorField()
		{
            Profiler.BeginSample("CalculateSectorField");
			if(PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null && m_mainPlayerTran == null)
				m_mainPlayerTran =PeCreature.Instance.mainPlayer.peTrans;

			if(m_mainPlayerTran != null)
			{
				//hideDistance = 5.0f;
				if(m_Enemies != null && m_Enemies.Count >0)
				{
					mdirs.Clear();
					for(int n=0;n<m_Enemies.Count;n++)
					{
						float distance = PEUtil.SqrMagnitudeH(m_Enemies[n].position, m_Trans.position);
						if(distance >=10.0f*10.0f)
							continue;

						if(m_Enemies[n].canAttacked)
							hideDirction(m_Enemies[n],m_mainPlayerTran.position);
					}

					direction = betterHideDirtion(mdirs);

					if(direction != Vector3.back && direction != Vector3.zero)
						m_HidePistion = (m_mainPlayerTran.position  + direction * 8.0f);
					else
						m_HidePistion = Vector3.zero;

					if(m_HidePistion != Vector3.zero)
						m_HidePistion = flee_steeringPos(m_HidePistion,m_mainPlayerTran.position,m_Trans);

					//Debug.DrawLine(m_mainPlayerTran.position,m_HidePistion,Color.yellow);
				}
				else
					m_HidePistion = Vector3.zero;
			}
            Profiler.EndSample();
		}

        bool MatchAttack(IAttack attack, Enemy enemy)
        {
            return attack.ReadyAttack(enemy);
        }

        bool MatchAttackPositive(IAttack attack)
        {
            return attack is IAttackPositive;
        }

        IEnumerator SwitchAttack()
        {
            while (true)
            {
                if(!Enemy.IsNullOrInvalid(m_Enemy) && (m_Enemy.Attack == null || !m_Enemy.Attack.IsRunning(m_Enemy)))
                {
                    IAttack tmp = null;

                    for (int i = 0; i < _Attacks.Count; i++)
                    {
                        if (_Attacks[i].IsReadyCD(m_Enemy) && _Attacks[i].ReadyAttack(m_Enemy))
                        {
                            tmp = _Attacks[i];
                            break;
                        }
                    }

                    if(tmp == null)
                    {
                        for (int i = 0; i < _Attacks.Count; i++)
                        {
                            if (_Attacks[i].IsReadyCD(m_Enemy) && (_Attacks[i] is IAttackPositive))
                            {
                                tmp = _Attacks[i];
                                break;
                            }
                        }
                    }

                    m_Enemy.Attack = tmp;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

		IEnumerator SwitchWeapon ()
		{
			while (m_Equipment != null) 
			{
                bool canSwitchWeapon = true;

                if (Entity.motionMgr != null && Entity.motionMgr.IsActionRunning(PEActionType.SwordAttack))
                    canSwitchWeapon = false;

                if (Entity.motionMgr != null && Entity.motionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
                    canSwitchWeapon = false;

                if (m_Equipment.IsSwitchWeapon())
                    canSwitchWeapon = false;

                if(Entity.isRagdoll)
                    canSwitchWeapon = false;

                if(Entity.netCmpt != null && !Entity.netCmpt.IsController)
                    canSwitchWeapon = false;

                

                if (canSwitchWeapon)
                {
                    if (!Enemy.IsNullOrInvalid(m_Enemy))
                    {
                        Vector3 dir1 = Vector3.ProjectOnPlane(Entity.tr.forward, Vector3.up);
                        Vector3 dir2 = m_Enemy.DirectionXZ;
                        bool canSwitchNow = (Entity.Race != ERace.Paja && Entity.Race != ERace.Puja) || Vector3.Angle(dir1, dir2) < 90f;

                        IWeapon weapon = SwitchWeapon(m_Enemy);
                        if (weapon != null && !weapon.Equals(null) && canSwitchNow)
                        {
                            if (m_Equipment.Weapon == null || m_Equipment.Weapon.Equals(null))
                            {
                                //Vector3 forward = Vector3.ProjectOnPlane(Entity.peTrans.trans.forward, Vector3.up);
                                //Vector3 direction = Vector3.ProjectOnPlane(m_Enemy.Direction, Vector3.up);
                                //float angle = Vector3.Angle(forward, direction);
                                //bool canHold = (Entity.Race != ERace.Puja && Entity.Race != ERace.Paja) || angle < 45f;

                                if (!weapon.HoldReady /*&& canHold*/)
                                {
                                    weapon.HoldWeapon(true);

                                    //float startTime = Time.time;
                                    while (weapon != null && !weapon.Equals(null) && !weapon.HoldReady && Time.time < 5.0f)
                                        yield return new WaitForSeconds(1.0f);
                                }
                            }
                            else
                            {
                                if (!m_Equipment.Weapon.Equals(weapon))
                                {
                                    m_Equipment.SwitchHoldWeapon(m_Equipment.Weapon, weapon);
                                    yield return new WaitForSeconds(5f);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (m_Equipment.Weapon != null && !m_Equipment.Weapon.Equals(null) && !m_UseTool)
                        {
                            m_Equipment.Weapon.HoldWeapon(false);
                            yield return new WaitForSeconds(2f);
                        }
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }
		}

        IEnumerator ClearEnemyEnumerator()
        {
            while (true)
            {
                m_CanSearch = true;
                if (m_Enemy != null && m_Npc != null)
                {
                    if (m_Npc.IsFollower && m_Npc.FollowDistance > 64.0f * 64.0f)
                    {
                        m_CanSearch = false;
                        ClearEnemy();
                    }
                }

                yield return new WaitForSeconds(5.0f);
            }
        }

        void CollectEnemies ()
		{
			if (!m_Scan || !m_CanSearch || IsDeath ())
				return;

			_tmpEntities.Clear ();
            _tmpReputationEntities.Clear();

            int playerID = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);

			int n = 0;
            if (m_Hears != null && m_Hears.Length > 0) {
				n = m_Hears.Length;
				for(int i = 0; i < n; i++){
					PEHearing hear = m_Hears[i];
					if (hear != null) {
                        //hear.Hearing();
                        if (hear.Entities != null && hear.Entities.Count > 0){
                            int m = hear.Entities.Count;
                            for (int j = 0; j < m; j++)
                            {
                                PeEntity skEntity = hear.Entities[j];
                                if (skEntity != null && !_tmpEntities.Contains(skEntity))
                                    _tmpEntities.Add(skEntity);
                            }
                        }
					}
				}
			}

			if (m_Visions != null && m_Visions.Length > 0)
            {
                n = m_Visions.Length;
				for(int i = 0; i < n; i++){
					PEVision vision = m_Visions[i];
                    if (vision != null) { 
                        //vision.Vision();
                        if (vision.Entities != null && vision.Entities.Count > 0){
                            int m = vision.Entities.Count;
                            for (int j = 0; j < m; j++)
                            {
                                PeEntity skEntity = vision.Entities[j];
                                if (skEntity != null && !_tmpEntities.Contains(skEntity))
                                    _tmpEntities.Add(skEntity);
                            }
                        }
					}
				}
			}

			n = _tmpEntities.Count;
            for (int i = 0; i < n; i++){
				PeEntity entity = _tmpEntities[i];
				if (entity == null || m_Entity.Equals (entity))
					continue;

                if(entity.IsDeath())
                {
                    if(m_Entity.Race == ERace.Monster && entity.Race == ERace.Monster)
                    {
                        if(!_corpses.Contains(entity))
                        {
                            //if (m_Food == null && m_Entity.ProtoID != entity.ProtoID && UnityEngine.Random.value < 1.0f)
                            //    m_Food = entity;

                            _corpses.Add(entity);
                        }
                    }

                    continue;
                }

                int playerid1 = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
                int playerid2 = (int)entity.GetAttribute(AttribType.DefaultPlayerID);

                if (entity.IsSeriousInjury && !_treats.Contains(entity) && !entity.IsDeath() && entity.hasView && playerid1 == playerid2)
                {
                    if(UnityEngine.Random.value < 1.0f)
                        m_Treat = entity;

                    _treats.Add(entity);
                }

                int id = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
                if (!_tmpReputationEntities.Contains(entity) && ReputationSystem.Instance.HasReputation(id, playerID))
                    _tmpReputationEntities.Add(entity);

				if (CanSelectEntity (entity) && CanAttackEntity(entity) && CanAttackCarrier(entity)) {

                    SelectEntity(entity);

                    if (m_Entity.Group != null)
                    {
                        m_Entity.Group.OnTargetDiscover(m_Entity, entity);
                    }
				}
			}

            _tmpEntities.Clear();
            SpecialHatred.IsHaveEnnemy(m_Entity, ref _tmpEntities);
            n = _tmpEntities.Count;
            for (int i = 0; i < n; i++)
            {
                if (_tmpEntities[i] != null && !ContainsEnemy(_tmpEntities[i]))
                {
                    SelectEntity(_tmpEntities[i], 100.0f);

                    if (m_Entity.Group != null)
                    {
                        m_Entity.Group.OnTargetDiscover(m_Entity, _tmpEntities[i]);
                    }
                }
            }
        }

		void UpdateEnemies ()
		{
			int n = m_Enemies.Count;
			for(int i = n-1; i >=0; i--){
				Enemy enemy = m_Enemies[i];
				if (enemy != null)
                {
					if (enemy.CanDelete() || !CanAttackCarrier(enemy.entityTarget) || !CanAttackEnemy(enemy.entityTarget))
                    {
                        if (!Entity.IsAttacking || !enemy.Equals(m_Enemy))
                        {
                            m_Enemies[i].Dispose();
                            m_Enemies.Remove(enemy);
                        }
                    }
                    else
                    {
                        if(enemy.entityTarget != null 
                            && enemy.entityTarget.vehicle != null 
                            && enemy.entityTarget.vehicle.creationPeEntity != null)
                        {
                            if(!ContainsEnemy(enemy.entityTarget.vehicle.creationPeEntity))
                            {
                                AddEnemy(new Enemy(m_Entity, enemy.entityTarget.vehicle.creationPeEntity));
                            }
                        }

                        if (enemy.entityTarget != null
                            && enemy.skTarget != null
                            && (enemy.skTarget is WhiteCat.CreationSkEntity))
                        {
                            enemy.ThreatShared = 0.0f;

                            WhiteCat.CarrierController carrier = enemy.entityTarget.carrier;
                            if (carrier != null)
                            {
                                carrier.ForeachPassenger(
                                    (PassengerCmpt passenger, bool isDriver)=>
                                        {
                                            Enemy e = m_Enemies.Find(ret => ret.entityTarget != null && ret.entityTarget.Equals(passenger.Entity));
                                            if (e != null)
                                            {
                                                enemy.ThreatShared += e.Threat * 10.0f;
                                            }
                                        }
                                    );
                            }
                        }

                        enemy.Update();
                    }
                }
			}

			m_Enemies.Sort ((a,b) => b.Hatred.CompareTo (a.Hatred));
		}

		void SelectEnemy ()
		{
#if ATTACK_OLD
            int idxAtk = m_Enemies.FindIndex (ret => ret.canAttacked /*&& CanAttack(ret)*/);	// Find first(which Threat is the largest)
#else
            int idxAtk = m_Enemies.FindIndex (ret => ret.canAttacked);	// Find first(which Threat is the largest)
#endif

			if (!CanAttack() || idxAtk < 0) {
				if (m_Enemy != null) {
					OnEnemyLost (m_Enemy);
					m_Enemy = null;
                }
				if (m_Equipment != null && m_Equipment.Weapon != null)
					m_Equipment.Weapon.HoldWeapon(false);
			} else {
				if (m_Enemy == null || !m_Enemy.isValid) {
					m_Enemy = m_Enemies [idxAtk];
					OnEnemyAchieve (m_Enemy);
				} else {
					if (!m_Enemy.canAttacked || !m_Enemies.Contains (m_Enemy) || m_Enemies [idxAtk].Hatred > m_Enemy.Hatred * SwitchValue) {
						OnEnemyChange (m_Enemy, m_Enemies [idxAtk]);
						m_Enemy = m_Enemies [idxAtk];
					}
				}
			}

            if(!Enemy.IsNullOrInvalid(m_Enemy) && m_Enemy.entityTarget.target != null)
            {
                if (m_Enemy.GroupAttack == EAttackGroup.Threat)
                {
                    List<PeEntity> entities = m_Enemy.entityTarget.target.GetMelees();
                    for (int i = 0; i < entities.Count; i++)
                    {
                        Enemy targetEnemy = m_Enemy.entityTarget.target.GetEnemy(entities[i]);
                        if (!Enemy.IsNullOrInvalid(targetEnemy))
                        {
                            if (m_Enemy.Distance < targetEnemy.Distance * 0.8f)
                            {
                                int num =  m_Enemy.entityTarget.monsterProtoDb != null &&  m_Enemy.entityTarget.monsterProtoDb.AtkDb != null ? m_Enemy.entityTarget.monsterProtoDb.AtkDb.mNumber : 3;
                                m_Enemy.entityTarget.target.RemoveMelee(targetEnemy.entityTarget);
                                m_Enemy.entityTarget.target.AddMelee(m_Entity, num);
                            }
                        }
                    }
                }
                //else
                //{
                //    if (m_Enemy.entityTarget != null && m_Enemy.entityTarget.target != null && !HasAttackRanged())
                //        m_Enemy.entityTarget.target.AddMelee(Entity);
                //}
            }
        }

        void SelectEntity(PeEntity entity, float hatred = 0.0f)
        {
            if (m_Entity == null)
                return;

            Enemy enemy = new Enemy (m_Entity, entity, hatred);

            AddEnemy(enemy);

            if (enemy.ThreatInit < -PETools.PEMath.Epsilon && UnityEngine.Random.value < Mathf.Abs(enemy.ThreatInit) / 100.0f)
                SetEscape(entity);
        }

		bool Match (IWeapon weapon, Enemy e)
		{
			if (e == null)
				return false;

			AttackMode[] modes = weapon.GetAttackMode ();
			for (int i = 0; i < modes.Length; i++)
            {
                if (modes[i].type == AttackType.Melee)
                {
                    if(m_Npc != null)
                    {
                        if (e.IsInAir)
                            continue;
                    }
                    else
                    {
                        if (!e.IsOnLand)
                            continue;
                    }
                }

                return true;
			}

			return false;
		}

        public bool CanAttackWeapon(IWeapon weapon, Enemy enemy)
        {
            if (m_Equipment.WeaponCanUse(weapon))
            {
                AttackMode[] modes = weapon.GetAttackMode();
                for (int i = 0; i < modes.Length; i++)
                {
                    if (modes[i].type == AttackType.Melee)
                    {
                        if (m_Npc != null && enemy.IsInAir)
                            continue;

                        if (m_Npc == null && Entity.Race != ERace.Mankind && !enemy.IsOnLand)
                            continue;
                    }
                    //else
                    //{
                    //    if (m_Npc == null && !enemy.IsOnLand)
                    //        continue;
                    //}

                    return true;
                }
            }

            return false;
        }

        public List<IWeapon> GetCanUseWeaponList(Enemy enemy)
        {
            _Weapons.Clear();

            if (m_CanActiveAttack)
            {
                if (m_Npc == null)
                    _Weapons = m_Equipment.GetWeaponList();
                else
                    _Weapons = m_Equipment.GetCanUseWeaponList(m_Entity);

                _Weapons = _Weapons.FindAll(ret => CanAttackWeapon(ret, enemy));
            }

            return _Weapons;
        }

        public  IWeapon SwitchWeapon (Enemy e)
		{
            IWeapon tmpWeapon = null;

            if(m_CanActiveAttack && !Enemy.IsNullOrInvalid(e))
            {
                float minDis = Mathf.Infinity;

				List<IWeapon> weapons;
                if (m_Npc != null)//&& (m_Npc.IsServant || m_Npc.Creater != null)
					weapons = m_Equipment.GetCanUseWeaponList(m_Entity);
				else
					weapons = m_Equipment.GetWeaponList();
               
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (!m_Equipment.WeaponCanUse(weapons[i]) || !(Match(weapons[i], e)))
                        continue;

                    float tmpMinDis = Mathf.Infinity;

                    AttackMode[] modes = weapons[i].GetAttackMode();

                    bool isBreak = false;

                    for (int j = 0; j < modes.Length; j++)
                    {
                        //目标没有攻击目标或者攻击目标不是自己的时候用远程攻击！
                        if (modes[j].type == AttackType.Ranged)
                        {
                            TargetCmpt targetCmpt = enemy.entityTarget.target;
                            if (targetCmpt != null)
                            {
                                PeEntity targetEntity = targetCmpt.enemy != null ? targetCmpt.enemy.entityTarget : null;
                                if (targetEntity == null || !targetEntity.Equals(Entity))
                                {
                                    tmpWeapon = weapons[i];
                                    isBreak = true;
                                    break;
                                }
                            }
                        }

                        tmpMinDis = Mathf.Min(Mathf.Abs(e.DistanceXZ - modes[j].minRange), Mathf.Abs(e.DistanceXZ - modes[j].maxRange));
                    }

                    if (isBreak && tmpWeapon != null)
                        break;

                    if (tmpMinDis < minDis)
                    {
                        minDis = tmpMinDis;
                        tmpWeapon = weapons[i];
                    }
                }
            }

			return tmpWeapon;
		}

        public bool HasAttackRanged()
        {
            if (m_Entity.Tower != null)
                return true;
            else if (m_Equipment != null && m_Equipment.GetWeaponList().Count > 0)
            {
                List<IWeapon> weaponList = m_Equipment.GetWeaponList();
                for (int i = 0; i < weaponList.Count; i++)
                {
                    AttackMode[] attackModes = weaponList[i].GetAttackMode();
                    for (int j = 0; j < attackModes.Length; j++)
                    {
                        if (attackModes[j].type == AttackType.Ranged)
                            return true;
                    }
                }
            }
            else if (Attacks != null && Attacks.Count > 0)
            {
                List<IAttack> attacks = Attacks;
                for (int i = 0; i < attacks.Count; i++)
                {
                    if (!(attacks[i] is BTMelee || attacks[i] is BTMeleeAttack))
                        return true;
                }
            }
            else
                return true;

            return false;
        }

        void OnEnemyEnter(Enemy enemy)
        {
            if (m_Entity.peSkEntity != null)
                m_Entity.peSkEntity.DispatchEnemyEnterEvent(enemy.entityTarget);

            if (enemy.entityTarget != null && enemy.entityTarget.peSkEntity != null)
                enemy.entityTarget.peSkEntity.DispatchBeEnemyEnterEvent(m_Entity);

            //if (enemy.entityTarget != null && enemy.entityTarget.target != null && !HasAttackRanged())
            //    enemy.entityTarget.target.AddMelee(Entity);

            //if (enemy != null && enemy.entityTarget != null && enemy.entityTarget.NpcCmpt != null)
            //    enemy.entityTarget.NpcCmpt.AddEnemyLocked(enemy.entity);
        }

        void OnEnemyExit(Enemy enemy)
        {
            if (m_Entity.peSkEntity != null)
                m_Entity.peSkEntity.DispatchEnemyExitEvent(enemy.entityTarget);

            if (enemy.entityTarget != null && enemy.entityTarget.peSkEntity != null)
                enemy.entityTarget.peSkEntity.DispatchBeEnemyExitEvent(m_Entity);

            if (enemy.entityTarget != null && enemy.entityTarget.target != null)
                enemy.entityTarget.target.RemoveMelee(Entity);

            //if (enemy != null && enemy.entityTarget != null && enemy.entityTarget.NpcCmpt != null)
            //    enemy.entityTarget.NpcCmpt.RemoveEnemyLocked(enemy.entity);
        }

		void OnEnemyAchieve (Enemy enemy)
		{
			if (m_Animator != null)
				m_Animator.SetBool ("Combat", true);

			if (m_Request != null) 
			{
				if((m_Npc != null && !m_Npc.hasAnyRequest))// && m_Npc.Campsite != null && !m_Npc.IsFlollowTarget) || (m_Npc != null && m_Npc.IsServant))
				{
					//no Reqest Attack
				}
				else
				{
					m_Request.Register (EReqType.Attack);
				}
			   
			}

            if (m_Entity.peSkEntity != null)
                m_Entity.peSkEntity.DispatchEnemyAchieveEvent(enemy.entityTarget);

            OnEnemyEnter(enemy);

//            if(m_Npc != null)
//                StartCoroutine(SwitchWeapon());
        }

        void OnEnemyChange (Enemy oldEnemy, Enemy newEnemy)
		{
            OnEnemyExit(oldEnemy);
            OnEnemyEnter(newEnemy);
		}

		void OnEnemyLost (Enemy enemy)
		{
			m_FirstDamage = true;

			if (m_Animator != null)
				m_Animator.SetBool ("Combat", false);

            if (m_Entity.Tower != null)
                m_Entity.Tower.Target = null;

            if (m_Entity.animCmpt != null && m_Entity.animCmpt.GetBool("Squat"))
                m_Entity.animCmpt.SetBool("Squat", false);

            if (m_Request != null)
			    m_Request.RemoveRequest (EReqType.Attack);

            if (m_Entity.peSkEntity != null)
                m_Entity.peSkEntity.DispatchEnemyLostEvent(enemy.entityTarget);

            OnEnemyExit(enemy);

			PeNpcGroup.Instance.OnEnemyLost(enemy);
            NpcHatreTargets.Instance.OnEnemyLost(enemy.entityTarget);

			if (PeGameMgr.IsMulti && m_SkEntity.IsController() && Entity.Tower != null)
			{
				if (m_SkEntity._net is AiTowerNetwork)
					(m_SkEntity._net as AiTowerNetwork).RequestEnemyLost();
				else if (m_SkEntity._net is MapObjNetwork)
					(m_SkEntity._net as MapObjNetwork).RequestEnemyLost();
			}

//            if(m_Npc != null)
//                StopCoroutine(SwitchWeapon());
		}
		

		void OnDeath (SkEntity self, SkEntity killer)
		{
			ClearEnemy ();
		}

		void OnDamage (PeEntity entity, float damage)
		{
			//攻击加仇恨
			if (!m_IsAddHatred || IsDeath ()) 
				return;

            if (CanAddDamageHatred(entity))
            {
                AddDamageHatred(entity, damage * (m_FirstDamage ? Multiple : 1.0f));

                m_FirstDamage = false;

                if (m_Entity.HPPercent <= m_EscapeBase)
                {
                    if (Entity.Race == ERace.Puja || Entity.Race == ERace.Paja)
                    {
                        if (GetEscapeEnemy() != null && UnityEngine.Random.value <= 0.3f)
                        {
                            m_Escape = null;
                            m_Entity.IsSeriousInjury = true;
                        }
                    }

                    if (!m_Entity.IsSeriousInjury && UnityEngine.Random.value <= m_EscapeProp)
                        SetEscape(entity);
                }

                if (Enemy.IsNullOrInvalid(m_Escape))
                {
                    Enemy enemy = m_Enemies.Find(ret => ret.entityTarget != null && ret.entityTarget == entity);
                    if (!Enemy.IsNullOrInvalid(enemy))
                    {
                        if( enemy.ThreatInit < -PETools.PEMath.Epsilon && Enemy.IsNullOrInvalid(m_Enemy))
                            SetEscape(entity);
                    }
                }
            }

            m_Food = null;
			m_beSkillTarget = false;
			CancelInvoke("ReflashSelf");
		}

		public void OnTargetSkill(SkEntity entity)
		{
			if(m_beSkillTarget)
				return ;

			float SKILL_HATER = 5.0f;
			m_beSkillTarget = true;
			SkEntity caster = PEUtil.GetCaster (entity);
			if (caster != null)
			{
				PeEntity e = caster.GetComponent<PeEntity>();
				if(e == m_Entity)
					return ;

				if (!HasHatred(e))
				{
				    AddSharedHatred(e, SKILL_HATER);
				}
				Invoke("ReflashSelf",2.0f);
			}
		}

		void ReflashSelf()
		{
			m_beSkillTarget = false;
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();

            if (m_Entity != null && m_Entity.skEntity != null)
            {
                PESkEntity peEntity = m_Entity.skEntity as PESkEntity;
                if (peEntity != null)
                {
                    peEntity.deathEvent -= OnDeath;
                }
            }

			ClearEnemy ();
		}

		public void OnMsg (EMsg msg, params object[] args)
		{
			switch (msg) {
			case EMsg.View_Model_Build:
				BiologyViewRoot viewRoot = args[1] as BiologyViewRoot;
				m_Visions = viewRoot.visions;
				m_Hears = viewRoot.hears;
                m_Monster = viewRoot.monster;
    
                break;
            case EMsg.View_Prefab_Build:
				//BiologyViewCmpt obj1 = args [0] as BiologyViewCmpt;
				//m_IKAim = obj1.monoIKAimCtrl;
				break;
			case EMsg.Battle_HPChange:
				SkEntity skEntity = (SkEntity)args [0];
				float damage = (float)args [1];
				if (skEntity != null && damage < PETools.PEMath.Epsilon) {
                    SkEntity caster = PEUtil.GetCaster(skEntity);
                    if (caster != null)
                    {
                        PeEntity e = caster.GetComponent<PeEntity>();
                        if (e != null)
                        {
                            if (m_Entity.Group != null)
                                m_Entity.Group.OnDamageMember(m_Entity, e, Mathf.Abs(damage));

                            OnDamage(e, Mathf.Abs(damage));
                        }
                    }
                    
				}
				break;
			case EMsg.Battle_TargetSkill:
				SkEntity _skEntity = (SkEntity)args [0];
				if(_skEntity != null)
				   OnTargetSkill (_skEntity);
				break;
            case EMsg.Skill_Interrupt:
                if(!Enemy.IsNullOrInvalid(m_Enemy) && 
                    m_Attack != null && 
                    m_Attack.IsRunning(m_Enemy) && 
                    m_Attack.CanInterrupt())
                {
                    
                }
                break;
			default:
				break;
			}
		}
	}

	public class ThreatData
	{
		int m_ID;
		//string m_Name;
		int[] m_Init;
		//int[] m_Amount;
		int[] m_InitChange;
		Dictionary<int, int> m_InitCover;

		static Dictionary<int, ThreatData> s_ThreatData;

		public static void LoadData ()
		{
			s_ThreatData = new Dictionary<int, ThreatData> ();

			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable ("initthreat");

			int threatCount = reader.FieldCount - 3;

			while (reader.Read()) {
				ThreatData data = new ThreatData ();
				data.m_Init = new int[threatCount];
				//data.m_Amount = new int[threatCount];
				data.m_InitCover = new Dictionary<int, int> ();
				data.m_ID = Convert.ToInt32 (reader.GetString (0));
				reader.GetString (1);

				for (int i = 0; i < threatCount; i++) {
					data.m_Init[i] = Convert.ToInt32(reader.GetString(i + 3));
				}

				s_ThreatData.Add (data.m_ID, data);
			}
		}

		public static int GetInitData (int src, int dst)
		{
			if (PeGameMgr.IsMulti)
			{
				if ((src >= GroupNetwork.minTeamID && src <= GroupNetwork.maxTeamID) &&
					(dst >= GroupNetwork.minTeamID && dst <= GroupNetwork.maxTeamID))
				{
					if (PeGameMgr.IsVS)
						return 5;
					else
						return 0;
				}
				else if (src >= GroupNetwork.minTeamID && src <= GroupNetwork.maxTeamID)
					src = 1;
				else if (dst >= GroupNetwork.minTeamID && dst <= GroupNetwork.maxTeamID)
					dst = 1;
			}

			if (s_ThreatData.ContainsKey (src)) {
				if (s_ThreatData [src].m_InitCover.ContainsKey (dst))
					return s_ThreatData [src].m_InitCover [dst];
				else
                {
                    if(dst >= 0 && dst < s_ThreatData [src].m_Init.Length)
					    return s_ThreatData [src].m_Init [dst];
                    else
                    {
                        Debug.LogError("Error camp id : " + dst);
                        return 0;
                    }
                }
			}

            Debug.LogError("Error camp id : " + src);
			return 0;
		}

		public static void SetThreatData (int src, int dst, int value)
		{
			if (s_ThreatData.ContainsKey (src)) {
				if (s_ThreatData [src].m_InitCover.ContainsKey (dst))
					s_ThreatData [src].m_InitCover [dst] = value;
				else
					s_ThreatData [src].m_InitCover.Add (dst, value);
			}
		}

		public static void Clear (int src)
		{
			if (s_ThreatData.ContainsKey (src)) {
				s_ThreatData [src].m_InitCover.Clear ();
			}
		}

		public static void Clear (int src, int dst)
		{
			if (s_ThreatData.ContainsKey (src)) {
				if (s_ThreatData [src].m_InitCover.ContainsKey (dst))
					s_ThreatData [src].m_InitCover.Remove (dst);
			}
		}
	}

	public class CampData
	{
		int m_ID;
		string m_Name;
		int[] m_Data = null;

		static Dictionary<int, CampData> s_CampData;

		public static void LoadData ()
		{
			s_CampData = new Dictionary<int, CampData> ();

			//SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable ("inithatred");

			//int campCount = reader.FieldCount - 3;

			//while (reader.Read()) {
			//	CampData data = new CampData ();
			//	data.m_Data = new int[campCount];
			//	data.m_ID = Convert.ToInt32 (reader.GetString (0));
			//	data.m_Name = reader.GetString (1);

			//	for (int i = 0; i < campCount; i++) {
			//		data.m_Data [i] = Convert.ToInt32 (reader.GetString (i + 3));
			//	}

			//	s_CampData.Add (data.m_ID, data);
			//}
		}

		public static int GetValue (int src, int dst)
		{
			if (s_CampData.ContainsKey (src))
				return s_CampData [src].m_Data [dst];

			return 0;
		}
	}

	public class DamageData
	{
		int m_ID;
		//string m_Name;
		int[] m_Data;

		static Dictionary<int, DamageData> s_CampData;

		public static void LoadData ()
		{
			s_CampData = new Dictionary<int, DamageData> ();

			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable ("isaddhatred");

			int campCount = reader.FieldCount - 3;

			while (reader.Read()) {
				DamageData data = new DamageData ();
				data.m_Data = new int[campCount];
				data.m_ID = Convert.ToInt32 (reader.GetString (0));
				reader.GetString (1);

				for (int i = 0; i < campCount; i++) {
					data.m_Data [i] = Convert.ToInt32 (reader.GetString (i + 3));
				}

				s_CampData.Add (data.m_ID, data);
			}
		}

		public static int GetValue (int src, int dst)
		{
            try
            {
				if (PeGameMgr.IsMulti)
				{
					if ((src >= GroupNetwork.minTeamID && src <= GroupNetwork.maxTeamID) &&
						(dst >= GroupNetwork.minTeamID && dst <= GroupNetwork.maxTeamID))
						return 5;
					else if (src >= GroupNetwork.minTeamID && src <= GroupNetwork.maxTeamID)
						src = 1;
					else if (dst >= GroupNetwork.minTeamID && dst <= GroupNetwork.maxTeamID)
						dst = 1;
				}

				if (s_CampData.ContainsKey(src))
                    return s_CampData[src].m_Data[dst];
            }
            catch
            {
                Debug.LogError("src id = " + src + " --> " + "dst id = " + dst);
            }

            return 0;
        }
    }

	public enum EEnemyState
	{
		None,
		Ignore,
		Threat,
		Attack,
		Afraid,
		Escape
	}

    public enum EEnemySpace
    {
        None,
        Land,
        Sky,
        Water,
        WaterSurface,
        Max
    }

    public enum EEnemyMoveDir
    {
        Stop,
        Close,
        Away
    }

    public enum EAttackGroup
    {
        Threat,
        Attack
    }

	public enum EEnemyMsg
	{
		None,
		Melee,
		Shoot
	}

	public class EnemyMessage
	{
		EEnemyMsg m_Type;
		float m_Time;

		public EnemyMessage (EEnemyMsg msg, object data = null)
		{
			m_Type = msg;
			m_Data = data;

			m_Time = Time.time;
		}

		public EEnemyMsg Type {
			get { return m_Type; }
		}

		bool m_Dirty;

		public bool Dirty {
			get {
				if (Time.time - m_Time > 0.3f)
					m_Dirty = false;

				return m_Dirty; 
			}
			set { m_Dirty = value; }
		}

		object m_Data;

		public object Data {
			get { return m_Data; }
		}
	}

	public class Enemy : IDisposable
	{
		public const float ThreatIntervalDistance = 0.5f;
        public const float ThreatIntervalTime = 0.5f;
        public const float AttackBorderValue = 200.0f;
        public const float DamageHatredScale = 0.6f;
        public const float FollowUpDistance = 80.0f;
        public const float FollowUpDistanceInterval = 2.0f;

		PeEntity m_Entity;
		PeEntity m_EntityTarget;

        float m_ThreatExtra;
        float m_ThreatDistance;
        float m_StartTime;
        float m_HoldWeaponDelayTime;
        float m_LastFollowTime;
        float m_LastAttacktime;
        float m_LastCanMoveTime;
        float m_LastCanAttacktime;
        Vector3 m_LastAttackPos;

        public bool m_Ignore;

        EEnemySpace m_Space = EEnemySpace.None;

        EEnemyMoveDir m_MoveDir = EEnemyMoveDir.Stop;
        public EEnemyMoveDir MoveDir {
            get { return m_MoveDir; }
        }

        public EAttackGroup m_GroupAttack = EAttackGroup.Attack;
        public EAttackGroup GroupAttack {
            get { return m_GroupAttack; }
            set { m_GroupAttack = value; }
        }

        public bool IsOnLand {
            get { return !IsInWater && !IsInAir; }
        }

        public bool IsInWater {
            get {
                return IsDeepWater || IsShallowWater;
            }
        }

        public bool IsDeepWater
        {
            get
            {
                return m_Space == EEnemySpace.Water;
            }
        }

        public bool IsShallowWater{
            get
            {
                return m_Space == EEnemySpace.WaterSurface
                    && (m_EntityTarget == null
                    || m_EntityTarget.motionMove == null
                    || !m_EntityTarget.motionMove.grounded);
            }
        }

        public bool IsInAir {
            get { return m_Space == EEnemySpace.Sky; }
        }

		float m_Hatred;
		public float Hatred
        {
			get { return m_Hatred; }
		}

        float m_LastDamagetime;
        public float LastDamagetime
        {
            get { return m_LastDamagetime; }
        }

        float m_Threat;
		public float Threat {
			get { return m_Threat; }
		}

		int m_ThreatInit;
		public int ThreatInit {
			get { return m_ThreatInit; }
		}

        float m_ThreatShared;
        public float ThreatShared
        {
            set { m_ThreatShared = value; }
            get { return m_ThreatShared; }
        }

        float m_ThreatDamage;
        public float ThreatDamage
        {
            get { return m_ThreatDamage; }
        }

        float m_ThreatSharedDamage;
        public float ThreatSharedDamage
        {
            get { return m_ThreatSharedDamage; }
        }

        float m_SqrDistanceXZ;
        public float SqrDistanceXZ
        {
            get { return m_SqrDistanceXZ; }
        }

        float m_SqrDistance;
        public float SqrDistance
        {
            get { return m_SqrDistance; }
        }

        float m_DistanceXZ;
        public float DistanceXZ
        {
            get { return m_DistanceXZ; }
        }

        float m_Distance;
        public float Distance
        {
            get { return m_Distance; }
        }

		float m_AttackDistance;
		public float AttackDistance
		{
			get { return m_Distance; }
		}

        Vector3 m_Direction;
        public Vector3 Direction
        {
            get { return m_Direction; }
        }

        Vector3 m_DirectionXZ;
        public Vector3 DirectionXZ
        {
            get { return m_DirectionXZ; }
        }

        Vector3 m_ClosetPoint;
        public Vector3 closetPoint { get { return m_ClosetPoint; } }

        Vector3 m_FarthestPoint;
        public Vector3 farthestPoint { get { return m_FarthestPoint; } }

        bool m_Inside;
        public bool Inside
        {
            get { return m_Inside && m_DirectionXZ.sqrMagnitude < 0.5f * 0.5f; }
        }

        bool m_InHeight;

        Path m_Path;

        public Vector3 velocity
        {
            get
            {
                return m_EntityTarget != null ? m_EntityTarget.velocity : Vector3.zero;
            }
        }

        public float SqrDistanceLogic
        {
            get
            {
                if (m_Entity.Field == MovementField.Land && m_InHeight)
                    return m_SqrDistanceXZ;
                else
                    return m_SqrDistance;
            }
        }

        IAttack m_Attack;
        public IAttack Attack
        {
            get { return m_Attack; }
            set { m_Attack = value; }
        }

		public static bool IsNullOrInvalid (Enemy enemy)
		{
			return enemy == null || !enemy.isValid;
		}

		public Enemy (PeEntity argSelf, PeEntity argTarget, float argHatred = 0.0f)
		{
            Profiler.BeginSample("Enemy");

            m_Entity = argSelf;
            m_EntityTarget = argTarget;

            m_StartTime = Time.time;
            m_HoldWeaponDelayTime = UnityEngine.Random.Range(0.0f, 1.0f);

            m_LastFollowTime = -1000.0f;
			m_LastAttackPos = m_Entity.position;
			m_ThreatExtra = 0.0f;

            m_LastDamagetime = Time.time;
            m_LastAttacktime = Time.time;
            m_LastCanAttacktime = Time.time;

			m_ThreatInit = GetThreatInit (m_Entity, m_EntityTarget);
			AddHatred (argHatred);

            if (m_EntityTarget != null && m_EntityTarget.peSkEntity != null)
                m_EntityTarget.peSkEntity.onHpReduce += OnDamageTarget;

//            if (m_Entity != null)
//                m_Entity.StartCoroutine(AssessPath());

            Profiler.EndSample();
		}

        void OnPathComplete(Path _p)
        {
            ABPath path = _p as ABPath;

            path.Claim(this);

            if (path.error)
            {
                path.Release(this);
                Debug.LogError("error!!");
            }

            if (m_Path != null)
                m_Path.Release(this);

            m_Path = path;
        }

        IEnumerator AssessPath()
        {
            while (true)
            {
                if (isValid)
                {
                    ABPath.Construct(m_Entity.position, m_EntityTarget.position, OnPathComplete);
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        public void Update()
        {
            if (!isValid) return;

            UpdateMoveDir();
            UpdateHatred();
            UpdateFollow();
            UpdateCanMove();
            UpdateSpace();
            UpdateCanAttack();

            m_ClosetPoint = m_Entity.tr.TransformPoint(m_Entity.bounds.ClosestPoint(m_Entity.tr.InverseTransformPoint(m_EntityTarget.position)));
            m_FarthestPoint = m_EntityTarget.tr.TransformPoint(m_EntityTarget.bounds.ClosestPoint(m_EntityTarget.tr.InverseTransformPoint(m_Entity.position)));


            m_Direction = m_EntityTarget.position - m_Entity.position;
            m_DirectionXZ = Vector3.ProjectOnPlane(m_Direction, Vector3.up);

            m_Distance = PEUtil.Magnitude(m_ClosetPoint, m_FarthestPoint);
            m_SqrDistance = PEUtil.SqrMagnitude(m_ClosetPoint, m_FarthestPoint);

            Vector3 dir = Vector3.ProjectOnPlane(m_FarthestPoint - m_ClosetPoint, Vector3.up);
            m_DistanceXZ = PEUtil.Magnitude(m_ClosetPoint, m_FarthestPoint, false);
            m_SqrDistanceXZ = PEUtil.SqrMagnitude(m_ClosetPoint, m_FarthestPoint, false);

            float tarHeightDown = m_EntityTarget.position.y - 0.5f;
            float tarHeightTop = m_EntityTarget.position.y + m_EntityTarget.maxHeight + 0.5f;
            float selHeightDown = m_Entity.position.y;
            float selHeightTop = m_Entity.position.y + m_Entity.maxHeight;

            m_InHeight =   (selHeightDown <= tarHeightTop && selHeightDown >= tarHeightDown)
                        || (selHeightTop <= tarHeightTop && selHeightTop >= tarHeightDown);

            if (Vector3.Dot(m_DirectionXZ.normalized, dir.normalized) >= 0.0f)
                m_Inside = false;
            else
            {
                //m_Distance = 0.0f;
                //m_SqrDistance = 0.0f;
                m_SqrDistanceXZ = 0.0f;

                if (Mathf.Abs(m_DirectionXZ.x) < m_Entity.bounds.extents.x
                    || Mathf.Abs(m_DirectionXZ.z) < m_Entity.bounds.extents.z)
                    m_Inside = true;
                else
                    m_Inside = false;
            }
        }

        void UpdateCanMove()
        {
            if (m_LastCanMoveTime < PETools.PEMath.Epsilon && 
                m_Entity.attackEnemy != null      && 
                m_Entity.attackEnemy.Equals(this) && 
                m_Entity.Stucking(5.0f))
                m_LastCanMoveTime = Time.time;

            if (m_LastCanMoveTime > PETools.PEMath.Epsilon && m_Distance < 2f)
                m_LastCanMoveTime = 0.0f;
        }

        void UpdateSpace()
        {
            if (PEUtil.CheckPositionUnderWater(m_EntityTarget.peTrans.centerUp))
                m_Space = EEnemySpace.Water;
            else if (PEUtil.CheckPositionUnderWater(m_EntityTarget.peTrans.position))
                m_Space = EEnemySpace.WaterSurface;
            else if (!Physics.Raycast(m_EntityTarget.position + Vector3.up*0.1f, Vector3.down, m_Entity.maxHeight, PEConfig.GroundedLayer)
                  && !Physics.Raycast(m_EntityTarget.position + m_EntityTarget.tr.up* m_EntityTarget.maxHeight, -m_EntityTarget.tr.up, m_EntityTarget.maxHeight, PEConfig.GroundedLayer))
                m_Space = EEnemySpace.Sky;
            else
                m_Space = EEnemySpace.Land;
        }

        void UpdateMoveDir()
        {
            Vector3 vel = Vector3.ProjectOnPlane(m_EntityTarget.velocity, Vector3.up);

            if (vel.sqrMagnitude < 0.15f * 0.15f)
                m_MoveDir = EEnemyMoveDir.Stop;
            else
            {
                if (Vector3.Dot(m_DirectionXZ, vel) > 0.0f)
                    m_MoveDir = EEnemyMoveDir.Away;
                else
                    m_MoveDir = EEnemyMoveDir.Close;
            }
        }

        void UpdateThreat()
        {
            float threatDamage = m_ThreatDamage + m_ThreatSharedDamage;

            //if (m_MoveDir != EEnemyMoveDir.Away)
            //    m_LastAttacktime = Time.time;

            //if (Time.time - m_LastDamagetime > 10.0f)
            //    threatDamage = Mathf.Lerp(m_ThreatDamage, 0.0f, (Time.time - m_LastDamagetime - 10.0f) * 0.01f);
            //else
            //    threatDamage = m_ThreatDamage;

            if (HasAttackRanged () && isReadyAttack ()) {
				threatDamage += 1000;

				if(m_EntityTarget.Tower != null)
					threatDamage += 1000;
			}

            m_Threat = m_ThreatInit + threatDamage + m_ThreatExtra + m_ThreatShared;
        }

        public void UpdateExtra()
        {
            if (m_Entity == null || m_EntityTarget == null)
                return;

            int hatred;
            if (SpecialHatred.IsHaveSpecialHatred (m_Entity, m_EntityTarget, out hatred)) 
			{
				if (hatred == 1) {
					m_Ignore = true;
					m_ThreatExtra = 0.0f;
				} else if (hatred == 2) {
					m_Ignore = false;
					m_ThreatExtra = 10000.0f;
				}
			} 
			else 
			{
				m_Ignore = false;
				m_ThreatExtra = 0.0f;
			}
        }

        public void UpdateFollow()
        {
            if(isValid)
            {
                float d = m_Entity.Field == MovementField.Sky ? 256.0f : 128.0f;

                if (m_Entity.ProtoID == 71)
                    d = 35;

                if(m_LastFollowTime < PETools.PEMath.Epsilon 
                    && PEUtil.SqrMagnitudeH(m_Entity.position, m_LastAttackPos) > d * d
                    && Time.time - m_LastDamagetime > 15.0f)
                    m_LastFollowTime = Time.time;

                if(m_LastFollowTime < PETools.PEMath.Epsilon 
                    && m_EntityTarget.target != null 
                    && m_EntityTarget.target.GetEscapeEnemy() != null 
                    && Time.time - m_LastDamagetime > 15.0f
                    && Time.time - m_LastAttacktime > 15.0f)
                    m_LastFollowTime = Time.time;

                //if (m_LastFollowTime < PETools.PEMath.Epsilon && m_Entity.Stucking(5.0f))
                //    m_LastFollowTime = Time.time;

                if (m_LastFollowTime > PETools.PEMath.Epsilon 
                    && Time.time - m_LastFollowTime > 5.0f 
                    && SqrDistanceXZ < 5.0f * 5.0f)
                {
                    m_LastFollowTime = -1000.0f;
                    m_LastAttackPos = m_Entity.position;
                }
            }
        }

        public bool CheckCanAttack()
        {
            if (!isValid)
                return false;

            if (!IsFollowPolarShield())
                return false;

            if (!m_Entity.target.CanAttack())
                return false;

            if (m_EntityTarget != null && m_Entity.target != null && m_EntityTarget != null)
            {
                if (m_Entity.Tower != null)
                {
                    return m_Entity.Tower.EstimatedAttack(m_EntityTarget.centerPos, m_EntityTarget.transform);
                }
                else if (m_Entity.motionEquipment != null && m_Entity.motionEquipment.GetWeaponList().Count > 0)
                {
                    return m_Entity.target.GetCanUseWeaponList(this).Count > 0;
                }
                else if (m_Entity.target.Attacks != null && m_Entity.target.Attacks.Count > 0)
                {
                    return m_Entity.target.Attacks.Find(ret => ret.CanAttack(this)) != null;
                }
                else
                {
                    if (m_Entity.Field == MovementField.water && m_Space != EEnemySpace.Water && m_Space != EEnemySpace.WaterSurface)
                        return false;

                    if (m_Entity.Field == MovementField.Sky && m_Space == EEnemySpace.Water)
                        return false;

                    if (m_Entity.Field == MovementField.Land && m_Space != EEnemySpace.Land)
                        return false;

                    return true;
                }
            }

            return true;
        }

        public bool HasAttackRanged()
        {
            if (m_Entity.Tower != null)
                return true;
            else if (m_Entity.motionEquipment != null && m_Entity.motionEquipment.GetWeaponList().Count > 0)
            {
                List<IWeapon> weaponList = m_Entity.motionEquipment.GetWeaponList();
                for (int i = 0; i < weaponList.Count; i++)
                {
                    AttackMode[] attackModes = weaponList[i].GetAttackMode();
                    for (int j = 0; j < attackModes.Length; j++)
                    {
                        if (attackModes[j].type == AttackType.Ranged)
                            return true;
                    }
                }
            }
            else if (m_Entity.target.Attacks != null && m_Entity.target.Attacks.Count > 0)
            {
                List<IAttack> attacks = m_Entity.target.Attacks;
                for (int i = 0; i < attacks.Count; i++)
                {
                    if (!(attacks[i] is BTMelee || attacks[i] is BTMeleeAttack))
                        return true;
                }
            }
            else
                return true;

            return false;
        }

        bool CanAttackMode(AttackMode mode)
        {
            if (Distance < mode.minRange || Distance > mode.maxRange)
                return false;

            return mode.ignoreTerrain || !PEUtil.IsBlocked(m_Entity, m_EntityTarget);
        }

        bool isReadyAttack()
        {
            if(m_Entity.Tower != null)
                return m_Entity.Tower.EstimatedAttack(m_EntityTarget.centerPos, m_EntityTarget.transform);
            else if(m_Entity.motionEquipment != null && m_Entity.motionEquipment.Weapon != null)
            {
                AttackMode[] modes = m_Entity.motionEquipment.Weapon.GetAttackMode();
                for (int i = 0; i < modes.Length; i++)
                {
                    if (modes[i].type == AttackType.Ranged && CanAttackMode(modes[i]))
                        return true;
                }
            }

            return false;
        }

        public void UpdateCanAttack()
        {
            if(CheckCanAttack())
                m_LastCanAttacktime = Time.time;
        }

        public void UpdateHatredDistance()
        {

            if (isValid && m_Threat > PETools.PEMath.Epsilon)
            {
                //水中怪物，空中怪物在攻击时不更新距离仇恨！
                if ((m_Entity.Field == MovementField.water || m_Entity.Field == MovementField.Sky) && m_Entity.IsAttacking)
                    return;

                float disHatred = Mathf.Max(0.0f, 35f*35f - m_SqrDistanceXZ);

                if (entityTarget.target != null && !entityTarget.target.ContainsMelee(entity) && !HasAttackRanged())
                    disHatred = Mathf.Lerp(disHatred, 0.0f, entityTarget.target.GetMeleeCount() * 0.333f);

                m_ThreatDistance = disHatred;
            }
            else
            {
                m_ThreatDistance = 0.0f;
            }
        }

        void UpdateHatred ()
		{
            m_ThreatInit = GetThreatInit (m_Entity, m_EntityTarget);

            UpdateThreat();
            UpdateExtra();
            UpdateHatredDistance();

            //&& ItemAsset.SelectItem.MatchEnemyAttack(entity,entityTarget)
            if (CanBeThreat())
                m_GroupAttack = EAttackGroup.Threat;
            else
                m_GroupAttack = EAttackGroup.Attack;

            if (m_ThreatInit < -PETools.PEMath.Epsilon)
                m_Hatred = 0.0f;
            else
			    m_Hatred = m_Threat * 0.5f + m_ThreatDistance * 0.5f;
		}

        public bool CanBeThreat()
        {
            if(entity.NpcCmpt == null)
              return !HasAttackRanged() 
                  && entityTarget.target != null
                  && entityTarget.target.GetMeleeCount() > 2
                  && !entityTarget.target.ContainsMelee(entity)
                  && entity.target != null
                  && this.Equals(entity.target.GetAttackEnemy());
            else
            {
                int n =entityTarget != null && entityTarget.monsterProtoDb != null && entityTarget.monsterProtoDb.AtkDb != null ? entityTarget.monsterProtoDb.AtkDb.mNumber : 3;
                if(!HasAttackRanged() 
                    && entityTarget.target != null
                    && entityTarget.target.GetMeleeCount() > n-1
                    && !entityTarget.target.ContainsMelee(entity)
                    && entity.target != null 
                    && this.Equals(entity.target.GetAttackEnemy())
                    || !ItemAsset.SelectItem.MatchEnemyAttack(entity, entityTarget)
                    )
                {
                    return true;
                }
                return false;
            }
        }

        public bool CanHoldWeapon()
        {
            return Time.time - m_StartTime > m_HoldWeaponDelayTime;
        }

        public void AddHatred(float damage)
        {
            if (!isValid) return;

            m_LastFollowTime = -1000.0f;
            m_LastAttackPos = m_Entity.position;
            m_ThreatSharedDamage += damage;
        }

        public void OnDamage(float damage)
        {
            if (!isValid) return;

            m_LastDamagetime = Time.time;
            m_LastFollowTime = -1000.0f;
            m_LastCanMoveTime = 0.0f;
            m_LastAttackPos = m_Entity.position;
            m_ThreatDamage += damage;
			m_AttackDistance = Mathf.Max(m_AttackDistance, Vector3.Distance(m_Entity.position, m_EntityTarget.position));
        }

        void OnDamageTarget(SkEntity caster, float damage)
        {
            SkEntity skCaster = PEUtil.GetCaster(caster);
            if (skCaster != null && m_Entity != null && m_Entity.skEntity == skCaster)
            {
                m_LastAttacktime = Time.time;
            }
        }

        MovementField GetMovementLimiter (Motion_Move mover)
		{
			if (mover == null)
				return MovementField.None;

			if (mover is Motion_Move_Motor)
				return (mover as Motion_Move_Motor).Field;
			else if (mover is Motion_Move_Human)
				return MovementField.Land;
			else
				return MovementField.None;
		}

		bool CanAddHatred (PeEntity e1, PeEntity e2)
		{
			int d1 = System.Convert.ToInt32 (e1.GetAttribute (Pathea.AttribType.DamageID));
			int d2 = System.Convert.ToInt32 (e2.GetAttribute (Pathea.AttribType.DamageID));

			return Pathea.DamageData.GetValue (d1, d2) > 0;
		}

		public bool matchAttackType(PeEntity self, PeEntity target)
		{
			if(self == null || target == null)
				return false;

			if(self.NpcCmpt == null || target.entityProto == null)
				return true;
			
			MonsterProtoDb.Item monsterPDB = MonsterProtoDb.Get(target.entityProto.protoId);
			if(null == monsterPDB)
				return true;
			
			switch(monsterPDB.attackType)
			{
			case 0:return false;
			case 1:return true;
			case -1:return GetAttackValue(target) >= GetAttackValue(self) * TargetCmpt.Combatpercent;
			default:break;
			}
			return true;
		}

		float GetAttackValue(PeEntity entity)
		{
			float value = TargetCmpt.HPpercent*entity.GetAttribute(AttribType.Hp) +  TargetCmpt.Atkpercent*entity.GetAttribute(AttribType.Atk) +  TargetCmpt.Defpercent*entity.GetAttribute(AttribType.Def);
			return value;
		}

		int GetThreatInit (PeEntity e1, PeEntity e2)
		{
			if(matchAttackType(e1,e2))
			{
				int cid1 = (int)e1.GetAttribute (AttribType.CampID);
				int cid2 = (int)e2.GetAttribute (AttribType.CampID);
				
				return  ThreatData.GetInitData (cid1, cid2);
			}
			return 0;

		}

		public override bool Equals (object obj)
		{
			Enemy enemy = obj as Enemy;

			if (enemy != null && enemy.skTarget != null)
				return enemy.skTarget.Equals (this.skTarget);

			return false;
		}

		// clear warnings
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public SkEntity skTarget { get { return m_EntityTarget.skEntity; } }
		public PeEntity entity { get { return m_Entity; } }
		public PeEntity entityTarget { get { return m_EntityTarget; } }

        public bool isValid {
			get {
                if (m_Entity == null || m_EntityTarget == null)
                    return false;

                if (!m_Entity.gameObject.activeSelf || !m_EntityTarget.gameObject.activeSelf)
                    return false;

				if (m_EntityTarget.IsDeath() || !m_EntityTarget.hasView)
					return false;

				return true;
			}
		}

        public bool isCarrierAndSky
        {
            get
            {
                if(m_EntityTarget != null && (m_EntityTarget.skEntity is WhiteCat.CreationSkEntity))
                {
                    RaycastHit hitInfo;
                    bool hit = Physics.Raycast(m_EntityTarget.position, Vector3.down, out hitInfo, 128.0f, PEConfig.GroundedLayer);
                    if (!hit || Mathf.Abs(m_EntityTarget.position.y - hitInfo.point.y) > 10.0f)
                        return true;
                }

                return false;
            }
        }

        public bool CanAttack()
        {
            if (m_EntityTarget.vehicle != null)
                return false;

            return Time.time - m_LastCanAttacktime < 10.0f;
        }

        public bool CanFollow()
        {
            return m_LastFollowTime < PETools.PEMath.Epsilon;
        }

        bool CanSelectForReputation()
        {
            if (m_EntityTarget == null || m_Entity == null)
                return false;

            if (m_ThreatDamage > PETools.PEMath.Epsilon)
                return PEUtil.CanDamageReputation(m_Entity, m_EntityTarget);
            else
                return PEUtil.CanAttackReputation(m_Entity, m_EntityTarget);
        }

        bool Conflict ()
		{
            if (m_EntityTarget == null || m_Entity == null)
                return false;

			int pid1 = (int)m_Entity.GetAttribute (AttribType.DefaultPlayerID);
            int pid2 = (int)m_EntityTarget.GetAttribute (AttribType.DefaultPlayerID);

            return CanSelectForReputation() && ForceSetting.Instance.Conflict(pid1, pid2);
		}

        public bool CanDelete()
        {
            if (m_Entity == null || m_EntityTarget == null || m_EntityTarget.IsDeath())
                return true;

            if (!Conflict())
                return true;

            if (m_LastFollowTime > PETools.PEMath.Epsilon && Time.time - m_LastFollowTime > 20.0f)
                return true;

            if (Time.time - m_LastCanAttacktime > 20.0f)
                return true;

            if (Time.time - m_LastDamagetime > 15.0f && Time.time - m_LastAttacktime > 15.0f && m_Entity.NpcCmpt != null)
                return true;

            return false;
        }

        public bool IsFollowPolarShield()
        {
            if (m_Entity.monster == null)
                return true;

            if (m_Entity.commonCmpt != null && m_Entity.commonCmpt.TDObj != null)
                return true;

            return !PolarShield.IsInsidePolarShield(m_EntityTarget.position, m_Entity.monster.InjuredLevel);
        }

        //public bool CanAddAttcker(PeEntity attacker)
        //{
        //    if (ContainsAttacker(attacker))
        //        return false;

        //    if(attacker.monsterProtoDb == null)
        //        return false;

        //    int atkNum = attacker.monsterProtoDb.AtkDb.mNumber;
        //    if (m_EntityAttackers.Count >= atkNum)
        //        return false;

        //    return true;
        //}

        //public bool ContainsAttacker(PeEntity attacker)
        //{
        //    return m_EntityAttackers != null && m_EntityAttackers.Contains(attacker);
        //}

        //public bool AddAttacker(PeEntity attacker)
        //{
        //    if (ContainsAttacker(attacker))
        //    {
        //        m_EntityAttackers.Add(attacker);
        //        return true;
        //    }
        //    return false;
        //}

        //public bool RemoveAttacker(PeEntity attacker)
        //{
        //    return m_EntityAttackers.Remove(attacker);
        //}

#if ATTACK_OLD
        public bool canAttacked         { get { return isValid && !m_Ignore && Hatred > PETools.PEMath.Epsilon && CanAttack() && CanFollow(); } }
        public bool canFollowed         { get { return isValid && !m_Ignore && Hatred > PETools.PEMath.Epsilon && !CanAttack(); } }
#else
        public bool canAttacked         { get { return isValid && !m_Ignore && Hatred > PETools.PEMath.Epsilon && CanFollow() && CanAttack(); } }
        public bool canFollowed         { get { return isValid && !m_Ignore && Hatred > PETools.PEMath.Epsilon && !CanAttack();} }
#endif
        public bool canEscaped          { get { return isValid && ThreatInit < -PETools.PEMath.Epsilon; } }
		public bool canAfraid           { get { return isValid && false; } }
		public bool canThreat           { get { return isValid && false; } }

		public Transform trans          { get { return m_EntityTarget != null ? m_EntityTarget.transform        : null; } }
		public Transform modelTrans     { get { return m_EntityTarget != null ? m_EntityTarget.tr               : null; } }
        public Vector3 position         { get { return m_EntityTarget != null ? m_EntityTarget.position         : Vector3.zero; } }
		public Vector3 centerPos        { get { return m_EntityTarget != null ? m_EntityTarget.centerPos        : Vector3.zero; } }
		public Quaternion rotation      { get { return m_EntityTarget != null ? m_EntityTarget.rotation         : Quaternion.identity; } }
		public Transform CenterBone     { get { return m_EntityTarget != null ? m_EntityTarget.centerBone       : null; } }
		public float radius             { get { return m_EntityTarget != null ? m_EntityTarget.maxRadius        : 0.0f; } }
		public float height             { get { return m_EntityTarget != null ? m_EntityTarget.maxHeight        : 0.0f; } }
		public bool isRagdoll           { get { return m_EntityTarget != null ? m_EntityTarget.isRagdoll        : false; } }
        

        
		public void Dispose ()
		{
            if (m_EntityTarget != null && m_EntityTarget.peSkEntity != null)
                m_EntityTarget.peSkEntity.onHpReduce -= OnDamageTarget;

//            if (m_Entity != null)
//                m_Entity.StopCoroutine(AssessPath());
        }
	}

	public enum ETargetType
	{
		Attack,
		Defence,
		Passive
	}
}

