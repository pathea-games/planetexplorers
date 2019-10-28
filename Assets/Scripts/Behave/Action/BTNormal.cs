using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using PETools;
using Pathea.Operate;
using Steer3D;

namespace Behave.Runtime
{
    public class BTNormal : BTAction
    {
        const float HungerFloor     = 0.1f;
        const float ComfortFloor    = 0.1f;

        internal static int WanderLayer = 1 << Pathea.Layer.VFVoxelTerrain
                | 1 << Pathea.Layer.SceneStatic;

        internal static int IgnoreWanderLayer = 1 << Pathea.Layer.Unwalkable;

        IAgent m_Agent;

		PeEntity m_Entity;
        BehaveCmpt m_Behave;
        SkAliveEntity m_Alive;
		PeTrans m_Transform;
		BiologyViewCmpt m_View;
        Motion_Move m_Motor;
        AnimatorCmpt m_Animator;
        RequestCmpt m_Request;
        TargetCmpt m_Target;
        IKAimCtrl m_IKAim;
        SkEntity m_SkEntity;
        Motion_Equip m_Equipment;
		NpcCmpt m_Npc;
        OperateCmpt m_Operator;
        MotionMgrCmpt m_Motion;
        UseItemCmpt m_UseItem;
        CommonCmpt m_Common;
        PassengerCmpt m_Passenger;
        MonsterCmpt m_Monster;
		//AbnormalConditionCmpt m_Abnormal;

        internal override void InitAgent(IAgent argAgent)
        {
            m_Agent = argAgent;

            if (m_Agent != null && !m_Agent.Equals(null))
            {
                m_Behave = m_Agent as BehaveCmpt;
                m_Entity = m_Behave.Entity;
                m_Alive = m_Entity.aliveEntity;
                m_Transform = m_Entity.peTrans;
                m_View = m_Entity.biologyViewCmpt;
                m_Motor = m_Entity.motionMove;
                m_Animator = m_Entity.animCmpt;
                m_Request = m_Entity.requestCmpt;
                m_Target = m_Entity.target;
                m_SkEntity = m_Entity.skEntity;
                m_Equipment = m_Entity.motionEquipment;
                m_Npc = m_Entity.NpcCmpt;
                m_Operator = m_Entity.operateCmpt;
                m_UseItem = m_Entity.UseItem;
                m_Common = m_Entity.commonCmpt;
                m_Passenger = m_Entity.passengerCmpt;
                m_Monster = m_Entity.monster;
                //m_Abnormal = m_Entity.Alnormal;
                m_Motion = m_Entity.motionMgr;
                m_IKAim = m_Entity.biologyViewCmpt.monoIKAimCtrl;

                //m_IKAim         = behave.GetComponentInChildren<IKAimCtrl>();
            }
        }

        #region Input
        internal PeEntity entity { get { return m_Entity; } }
        internal SkEntity skEntity { get { return m_SkEntity; } }
        internal BehaveCmpt behave { get { return m_Behave; } }

        //SkAliveCmpt
        internal float GetAttribute(AttribType type)
        {
            if (m_Alive != null)
            {
                return m_Alive.GetAttribute(type);
            }

            return 0.0f;
        }

        internal float HpPercent
        {
            get
            {
                PESkEntity skEnt = m_SkEntity as PESkEntity;
                return skEnt != null ? skEnt.HPPercent : 0.0f;
            }
        }

        //commoncmpt
        internal GameObject TDObj { get { return m_Common != null ? m_Common.TDObj : null; } }
        internal Vector3 TDpos { get { return m_Common != null ? m_Common.TDpos : Vector3.zero; } }

        //PeTrans
        internal Vector3    center { get { return m_Transform != null ? m_Transform.center : Vector3.zero; } }
        internal Vector3    position { get { return m_Transform != null ? m_Transform.position : Vector3.zero; } }
        internal Quaternion rotation { get { return m_Transform != null ? m_Transform.rotation : Quaternion.identity; } }
        internal Transform  transform { get { return m_Transform != null ? m_Transform.trans : null; } }
        internal Transform  existent { get { return m_Transform != null ? m_Transform.existent : null; } }
        internal float      radius { get { return m_Transform != null ? m_Transform.radius : 0.0f; } }
        internal bool       InBody(Vector3 pos)
        {
            if (m_Transform != null)
            {
                return m_Transform.InsideBody(pos);
            }

            return false;
        }
        internal Vector3    spawnPosition { get { return m_Transform != null ? m_Transform.spawnPosition : Vector3.zero; } }
        internal Vector3    spawnForward { get { return m_Transform != null ? m_Transform.spawnForward : Vector3.zero; } }

        //ViewCmpt
        internal Rigidbody modelRigid { get { return m_View != null && m_View.modelTrans != null ? m_View.modelTrans.GetComponentInChildren<Rigidbody>() : null; } }
        internal Transform GetModelName(string name)
        {
            Transform tr = null;

            if (m_View != null)
                tr = m_View.GetModelTransform(name);

			if (tr == null && m_View != null)
				return m_View.centerTransform;

            return tr;
        }
		internal bool hasModel{get{return m_View != null ? m_View.hasView : false;}}

        //Motion_Motor
        internal float      gravity { get { return m_Motor != null ? m_Motor.gravity : -1.0f; } }
        internal Vector3    velocity { get { return m_Motor != null ? m_Motor.velocity : Vector3.zero; } }
        internal bool       grounded { get { return m_Motor != null ? m_Motor.grounded : false; } }
        internal bool       Stucking(float time = 2.0f) 
        { 
            return m_Motor != null ? m_Motor.Stucking(time) : false; 
        }
        internal MovementField field 
        { 
            get 
            { 
                if(m_Motor is Motion_Move_Motor)
                    return (m_Motor as Motion_Move_Motor).Field;
                else if(m_Motor is Motion_Move_Human)
                    return MovementField.Land;
                else
                    return MovementField.None;
            } 
        }

        //RequestCmpt
        internal bool hasAnyRequest { get { return m_Request != null ? m_Request.HasRequest() : false;} }
        internal Request GetRequest(EReqType type)
        {
            return m_Request != null ? m_Request.GetRequest(type) : null;
        }
        internal void RemoveRequest (EReqType type)
        {
            if (m_Request != null)
            {
                m_Request.RemoveRequest(type);
            }
        }
        internal void RemoveRequest(Request request)
        {
            if (m_Request != null)
            {
                m_Request.RemoveRequest(request);
            }
        }

		internal bool ContainsRequest(EReqType type)
		{
			if (m_Request != null)
			{
				return m_Request.Contains(type);
			}
			return false;
		}

        //PassengerCmpt
        internal bool IsOnVCCarrier { get { return m_Passenger != null ? m_Passenger.IsOnVCCarrier : false; } }
		internal bool IsOnRail      { get { return m_Passenger != null ? m_Passenger.IsOnRail : false; }}

        //ScanTargetCmpt
        internal bool hasAnyEnemy { get { return m_Target != null ? m_Target.HasAnyEnemy() : false; } }
        internal bool hasAttackEnemy { get { return m_Target != null ? m_Target.GetAttackEnemy() != null : false; } }
        internal Enemy attackEnemy { get { return m_Target != null ? m_Target.GetAttackEnemy() : null; } }
        internal Enemy followEnemy { get { return m_Target != null ? m_Target.GetFollowEnemy() : null; } }
        internal Enemy escapeEnemy { get { return m_Target != null ? m_Target.GetEscapeEnemy() : null; } }
        internal Enemy threatEnemy { get { return m_Target != null ? m_Target.GetThreatEnemy() : null; } }
        internal Enemy afraidEnemy { get { return m_Target != null ? m_Target.GetAfraidEnemy() : null; } }
		internal List<Enemy> Enemies {get {return m_Target != null ? m_Target.GetEnemies() : null;}}
        internal Enemy selectattackEnemy { get { return attackEnemy; } } //return (m_Npc != null && !Enemy.IsNullOrInvalid(m_Npc.BattleMgr.choicedEnemy)) ? m_Npc.BattleMgr.choicedEnemy :

        internal PeEntity GetAfraidTarget()
        {
            if (m_Target != null)
                return m_Target.GetAfraidTarget();

            return null;
        }

        internal PeEntity GetDoubtTarget()
        {
            if (m_Target != null)
                return m_Target.GetDoubtTarget();

            return null;
        }

		internal void SetCambat(bool value)
		{
			if(m_Target != null)
			{
				m_Target.CanActiveAttck = value;
			}
		}

		internal void UseTool(bool value)
		{
			if(m_Target != null)
			{
				m_Target.UseTool = value;
			}
		}
        //SkAliveEntity
        internal SkInst StartSkill(PeEntity target, int id)
        {
			if (m_SkEntity != null)
            {
				if(!Enemy.IsNullOrInvalid(attackEnemy) && attackEnemy.entityTarget != null)
					attackEnemy.entityTarget.DispatchTargetSkill(entity.skEntity);

                SkEntity skTarget = target != null ? target.skEntity : null;

                return m_SkEntity.StartSkill(skTarget, id);
            }

            return null;
        }

        internal SkInst StartSkillSkEntity(SkEntity target, int id)
        {
			if (m_SkEntity != null)
            {
				return m_SkEntity.StartSkill(target, id);
            }

            return null;
        }

        internal void StopSkill(int id)
        {
            if (m_SkEntity != null)
            {
                m_SkEntity.CancelSkillById(id);
            }
        }

        internal bool IsSkillRunning(int id, bool cdInclude = false)
        {
            if (m_SkEntity != null)
            {
                return m_SkEntity.IsSkillRunning(id, cdInclude);
            }

            return false;
        }

        internal bool IsSkillRunnable(int id)
        {
            if (m_SkEntity != null)
            {
                return m_SkEntity.IsSkillRunnable(id);
            }

            return false;
        }

        //Motion_Equip
        internal IWeapon Weapon
        {
            get
            {
                if (m_Equipment != null && m_Equipment.Weapon != null && !m_Equipment.Weapon.Equals(null))
                    return m_Equipment.Weapon;

                return null;
            }
        }

        internal void WeaponAttack(IWeapon weapon,Enemy _attackEnmey, int index = 0, SkillSystem.SkEntity targetEntity = null)
        {
            if(weapon != null && !weapon.Equals(null))
            {
                weapon.Attack(index,targetEntity);

                if(entity.proto == EEntityProto.Monster && entity.commonCmpt != null && entity.commonCmpt.Race == ERace.Mankind
                    && !Enemy.IsNullOrInvalid(_attackEnmey) && _attackEnmey.entityTarget != null)
                {
                    _attackEnmey.entityTarget.DispatchWeaponAttack(entity.skEntity);
                }
            }
        }

        internal List<IWeapon> GetWeaponList()
        {
            if (m_Equipment != null)
                return m_Equipment.GetWeaponList();

            return new List<IWeapon>();
        }

		internal bool WeaponCanUse(IWeapon weapon)
		{
			if (m_Equipment != null)
				return m_Equipment.WeaponCanUse(weapon);

			return false;
		}

		internal void  ActiveWeapon(bool value)
		{
			if (m_Equipment != null)
			{
				m_Equipment.ActiveWeapon(value);
			}
		}

		internal void  ActiveWeapon(PEHoldAbleEquipment handEquipment, bool active, bool immediately = false)
		{
			if (m_Equipment != null)
			{
				m_Equipment.ActiveWeapon(handEquipment,active,immediately);
			}
		}

		
		internal bool GetEquipMaskState(PEActionMask mask)
        {
            if (m_Equipment != null)
            {
				return m_Motion.GetMaskState(mask);
            }

            return false;
        }

        internal bool IsActionRunning()
        {
			if (m_Motion != null)
            {
				return m_Motion.IsActionRunning();
            }

            return false;
        }

        internal bool IsActionRunning(PEActionType type)
        {
			if(m_Motion != null)
            {
				return m_Motion.IsActionRunning(type);
            }

            return false;
        }

		internal Action_Fell SetGlobalTreeInfo(GlobalTreeInfo treeInfo)
		{
			if(m_Motion != null)
			{
				Action_Fell fell = m_Motion.GetAction<Action_Fell>();
				if(null != fell)
					fell.treeInfo = treeInfo;

				return fell;
			}
			return null;
		}

		internal Action_Gather SetGlobalGatherInfo(GlobalTreeInfo treeInfo)
		{
			if(m_Motion != null)
			{
				Action_Gather gather = m_Motion.GetAction<Action_Gather>();
				if(null != gather)
					gather.treeInfo = treeInfo;
				
				return gather;
			}
			return null;
		}

        internal void ActivateEnergyShield(bool value)
        {
            if(m_Equipment != null && m_Equipment.energySheild != null)
            {
                if (value)
                    m_Equipment.energySheild.ActiveSheild(true);
                else
                    m_Equipment.energySheild.DeactiveSheild();
            }
        }

        //TowerCmpt
        internal bool TowerAngle(Vector3 position, float angle)
        {
            return entity.Tower != null ? entity.Tower.Angle(position, angle) : false;
        }

        internal bool TowerPitchAngle(Vector3 position, float angle)
        {
            return entity.Tower != null ? entity.Tower.PitchAngle(position, angle) : false;
        }

        internal bool TowerCanAttack(Vector3 position, Transform target = null)
        {
            return entity.Tower != null ? entity.Tower.CanAttack(position, target) : false;
        }

        internal bool TowerIsEnable()
        {
            return entity.Tower != null ? entity.Tower.IsEnable : false;
        }

        internal bool TowerHaveCost()
        {
            return entity.Tower != null ? entity.Tower.HaveCost() : false;
        }

        internal bool TowerSkillRunning()
        {
            return entity.Tower != null ? entity.Tower.IsSkillRunning() : false;
        }

        //NpcCmpt
        internal bool IsNpcFollowerWork { get { return m_Npc != null ? m_Npc.FollowerWork : false; } }
		internal bool IsNpcFollowerSentry { get { return m_Npc != null ? m_Npc.FollowerSentry : false;}}
		internal bool IsNpcFollowerCut { get { return m_Npc != null ? m_Npc.FollowerCut : false;}}
		internal bool IsNpcProcessing { get { return m_Npc != null ? m_Npc.Processing : false; } }
		internal bool IsNpcTrainning {get {return m_Npc != null ? m_Npc.IsTrainning : false;}}
        internal bool IsNpc { get { return m_Npc != null; } }
		internal bool IsNpcBase { get { return m_Npc != null && m_Npc.Creater != null && m_Npc.Creater .Assembly != null; } }
        internal bool IsNpcFollower { get { return m_Npc != null && m_Npc.Master != null; } }
        internal bool IsNpcCampsite { get { return m_Npc != null && m_Npc.Campsite != null; } }
		internal bool CanNpcWander{get {return m_Npc != null ? m_Npc.CanWander : false;}}
		internal ENpcJob NpcJob { get { return m_Npc != null ? m_Npc.Job : ENpcJob.None; } }
		internal ENpcState NpcJobStae { get { return m_Npc != null ? m_Npc.State : ENpcState.UnKnown; } }
		internal ENpcMedicalState NpcMedicalState { get { return m_Npc != null ? m_Npc.MedicalState : ENpcMedicalState.None; } }
        internal ENpcType NpcType { get { return m_Npc != null ? m_Npc.Type : ENpcType.None; } }
		internal ETrainerType NpcTrainerType{get{return m_Npc != null ? m_Npc.TrainerType : ETrainerType.none;}}
		internal ETrainingType NpcTrainingType {get{return m_Npc != null ? m_Npc.TrainningType : ETrainingType.Skill;}}
        internal ENpcSoldier NpcSoldier { get { return m_Npc != null ? m_Npc.Soldier : ENpcSoldier.None; } }
        internal ServantLeaderCmpt NpcMaster { get { return m_Npc != null ? m_Npc.Master : null; } }
        internal Vector3 GuardPosition { get { return m_Npc != null ? m_Npc.GuardPosition : Vector3.zero; } }
        internal float GuardRadius { get { return m_Npc != null ? m_Npc.GuardRadius : 0.0f; } }
        internal CSCreator Creater { get { return m_Npc != null ? m_Npc.Creater : null; } }
        internal Camp Campsite { get { return m_Npc != null ? m_Npc.Campsite : null; } }
        internal List<CSEntity> BaseEntities { get { return m_Npc != null ? m_Npc.BaseEntities : null; } }
        internal IOperation Sleep { get { return m_Npc != null ? m_Npc.Sleep : null; } }
        internal IOperation Work { get { return m_Npc != null ? m_Npc.Work : null; } }
		internal IOperation Cured { get { return m_Npc != null ? m_Npc.Cure : null; } }
		internal IOperation Trainner{get{return m_Npc != null ? m_Npc.Trainner : null;}}
		internal bool AskStop{get {return m_Npc != null ? m_Npc.MisstionAskStop : false;}}
		internal Vector3 FollowerHidePos{get{return m_Npc != null ? m_Npc.FollowerHidePostion : Vector3.zero;}}
		internal Vector3 FixedPointPostion{get{return m_Npc != null ? m_Npc.FixedPointPos : Vector3.zero;}}
		internal int NpcCmdId{get{return m_Npc != null ? m_Npc.NpcControlCmdId : 0;}}
		internal ENpcBattle NpcBattle { get {return m_Npc != null ? m_Npc.Battle : ENpcBattle.Defence;}}
		internal PEBuilding NpcOccpyBuild { get { return m_Npc != null ? m_Npc.OccopyBuild : null;}}
		//allyCmpt
		internal bool IsSkillCast { get { return m_Npc != null ? m_Npc.InAllys: false; } }
		internal PeEntity SkillTarget { get { return (m_Npc != null && m_Npc.NpcSkillTarget != null)? m_Npc.NpcSkillTarget.Entity : null; } }
		internal Vector3 CostPos { get { return (m_Npc != null )? m_Npc.NpcPostion: Vector3.zero; } }
		internal Vector3 AllyTargetPos {get { return (m_Npc != null && m_Npc.NpcSkillTarget != null)? m_Npc.NpcSkillTarget.NpcPostion : Vector3.zero; } }
		internal int GetAreadySkill()
		{
			if(m_Npc != null)
			{
				return m_Npc.GetReadySkill();
			}
			return -1;
		}
		internal float GetSkillRange(int SkillId)
		{
			if(m_Npc != null)
			{
				return m_Npc.GetNpcSkillRange(SkillId);
			}
			return 0.0f;
		}

		internal bool GetItemsSkill(Vector3 pos,float percent)
		{
			if(m_Npc != null)
			{
				return m_Npc.TryGetItemSkill(pos,percent);
			}
			return false;
		}


		//camp
		internal void SetOccpyBuild(PEBuilding buid)
		{
			if(m_Npc != null)
			{
				m_Npc.OccopyBuild = buid;
			}
		}

		internal bool GetHpJudge(int SkillId)
		{
			if(m_Npc != null && m_Npc.NpcSkillTarget != null)
			{
				float per = m_Npc.GetNpcChange_Hp(SkillId);
				if(per == 0)
					return true;

				float perTarg  = m_Npc.NpcSkillTarget.NpcHppercent;
				return perTarg <= per;
			}
			return false;
		}
		                            
		internal void CanWander(bool Iswork)
		{
			if(m_Npc != null)
			{
				m_Npc.CanWander = Iswork;
			}
		}

		internal void CallBackFollower()
		{
			if(m_Npc != null)
			{
				m_Npc.FollowerWork = false;
			}
		}

		internal void EndFollowerCut()
		{
			if(m_Npc != null)
			{
				m_Npc.FollowerCut = false;
				//ActiveWeapon(false);
				m_Npc.RmoveTalkInfo(ENpcTalkType.Follower_cut);
			}
		}

		internal void SetNpcAiType(ENpcAiType type)
		{
			if(m_Npc != null)
			{
				m_Npc.AiType = type;
			}
		}
		internal bool sendTalkMgs(int TalkId,float time = 0.0f,ENpcSpeakType type = ENpcSpeakType.TopHead)
		{
			if(m_Npc != null)
			{
				 return m_Npc.SendTalkMsg(TalkId,time,type);
			}
			return false;
		}
		internal bool SkillOver()
		{
			if(m_Npc != null)
			{
				m_Npc.Req_Remove(EReqType.UseSkill);
				m_Npc.InAllys = false;
				m_Npc.NpcSkillTarget =null;
				return true;
			}
			return false;
		}
		                       
		internal bool ContainAllys()
		{
			if(m_Npc != null)
			{
				return m_Npc.Containself();
			}
			return false;
		}

        internal bool ContainsTitle(ENpcTitle title) 
        { 
            if(m_Npc != null)
            {
                return m_Npc.ContainsTitle(title); 
            }

            return false;
        }
       
		internal CSEntity  WorkEntity { get { return m_Npc != null ? m_Npc.WorkEntity : null; }}

		internal bool NpcCanWalkPos(Vector3 center,float radiu,out Vector3 walkPos)
		{
			Vector3 newpos;
			Vector3 direction = position - center;
			for(int i=0;i<10;i++)
			{
				newpos  = PEUtil.GetRandomPositionInCircle(center, radiu * 0.7f, radiu,direction,60.0f,100.0f);
				newpos  = PEUtil.CheckPosForNpcStand(newpos);
				if(newpos != Vector3.zero) 
				{
					//3D距离（空中基地）
                    walkPos = newpos;
                    float dis = PEUtil.Magnitude(center, walkPos);
					if(dis <= radiu && dis > radiu * 0.7f)
					   return true;
				}
			}

			for(int i=0;i<10;i++)
			{
				newpos = PEUtil.GetRandomPositionOnGroundForWander(center,5.0f, radiu);
				newpos  = PEUtil.CheckPosForNpcStand(newpos);
				if(newpos != Vector3.zero) //&& AiUtil.GetNearNodePosWalkable(newpos,out walkPos)
				{
                    walkPos = newpos;
                    float dis = PEUtil.Magnitude(center, walkPos);
					if(dis <= radiu)
						return true;
				}
			}
			
			walkPos = PEUtil.GetRandomPositionOnGroundForWander(center, 5.0f, radiu);
			walkPos  = PEUtil.CheckPosForNpcStand(walkPos);

            return walkPos != Vector3.zero;
		}

		//CampiseCmpt
		internal Vector3 CampiseChanceTalk()
		{
			if(m_Npc !=null && m_Npc.Campsite !=null)
			{
				return m_Npc.Campsite.CalculatePostion(m_Npc.Entity.Id,m_Npc.NpcPostion,2.5f);
			}
			return Vector3.zero;
		}

		internal bool CampiseChanceTalk(out PeEntity talkTarg)
		{
			if(m_Npc !=null && m_Npc.Campsite !=null)
			{
				if(m_Npc.Campsite.CalculatePostion(entity,2.5f,out talkTarg))
				{
					return true;
				}
			}
			talkTarg = null;
			return false;
		}

		internal bool CantainTalkTarget(PeEntity Target)
		{
			if(m_Npc !=null && m_Npc.Campsite !=null)
			{
				return  m_Npc.Campsite.CantainTarget(2.5f,entity,Target);
			}
			return false;
		}

		internal bool IsSelfSleep(int entityID,out SleepPostion sleepInfo)
		{
			if(m_Npc !=null && m_Npc.Campsite !=null)
			{
				sleepInfo =  m_Npc.Campsite.HasSleep(entityID);
				if(sleepInfo != null)
				    return true;
				//return m_Npc.Campsite.HasSleep(entityID);
			}
			sleepInfo = null;
			return false;
		}

        //OperateCmpt
        internal IOperator Operator { get { return m_Operator; } }
		
        //MotionMgrCmpt
        internal bool IsMotionRunning(PEActionType type)
        {
            if (m_Motion != null)
                return m_Motion.IsActionRunning(type);

            return false;
        }
		internal bool CanDoAction(PEActionType type, PEActionParam objs = null)
		{
			if(m_Motion != null)
			{
				return  m_Motion.CanDoAction(type, objs);
			}
			return false;
		}

        internal bool DoAction(PEActionType type, PEActionParam objs = null)
        {
            if(m_Motion != null)
            {
                return  m_Motion.DoAction(type, objs);
            }
			return false;
        }

		internal void DoActionImmediately(PEActionType type, PEActionParam objs)
		{
			if(m_Motion != null)
			{
				m_Motion.DoActionImmediately(type, objs);
			}
		}
		
		internal bool EndAction(PEActionType type)
        {
            if (m_Motion != null)
            {
                return m_Motion.EndAction(type);
            }
			return false;
        }

		internal void EndImmediately(PEActionType type)
		{
			if(m_Motion != null)
			{
				m_Motion.EndImmediately(type);
			}
		}


        //AnimatorCmpt
        internal bool GetBool(string name)
        {
            if(m_Animator != null)
            {
                return m_Animator.GetBool(name);
            }

            return false;
        }
		
		//MedicineCmpt
		internal bool IsNeedMedicine{ get { return m_Npc != null ? m_Npc.IsNeedMedicine : false; }}

		internal void SetMedicineSate(ENpcMedicalState type)
		{
			if(m_Npc != null)
			{
				m_Npc.MedicalState = type;
			}
		}

        //MonsterCmpt
        internal bool IsFly()
        {
            if (m_Monster != null)
                return m_Monster.IsFly;

            return false;
        }

        internal NativeProfession nativeProfession
        {
            get { return m_Monster != null ? m_Monster.Profession : NativeProfession.None; }
        }

        internal NativeAge nativeAge
        {
            get { return m_Monster != null ? m_Monster.Age : NativeAge.None; }
        }

        internal NativeSex nativeSex
        {
            get { return m_Monster != null ? m_Monster.Sex : NativeSex.None; }
        }

        internal bool InsidePolarShield(Vector3 pos, out Vector3 position, out float radius)
        {
            radius = 0.0f;
            position = Vector3.zero;

            if (entity.monster == null)
                return false;

            if (entity.commonCmpt != null && entity.commonCmpt.TDObj != null)
                return false;

            return PolarShield.GetPolarShield(pos, entity.monster.InjuredLevel, out position, out radius);
        }

        internal bool EvadePolarShield(Vector3 pos)
        {
            if (entity.monster == null)
                return true;

            if (entity.commonCmpt != null && entity.commonCmpt.TDObj != null)
                return true;

            return !PolarShield.IsInsidePolarShield(pos, entity.monster.InjuredLevel);
        }

        internal Vector3 GetEvadePolarShieldPosition(Vector3 pos)
        {
            if(!EvadePolarShield(pos))
            {
                return PolarShield.GetRandomPosition(pos, entity.monster.InjuredLevel);
            }

            return Vector3.zero;
        }
        #endregion

        #region Output
        //SkEntity
        internal void SetAttribute(int key, float value)
        {
            if(m_SkEntity != null)
            {
                m_SkEntity.SetAttribute(key, value, false);
            }
        }

        internal void SetModelFadeIn() 
		{
			StandardAlphaAnimator NpcModelAlpha = m_Npc.GetComponentInChildren<StandardAlphaAnimator>();
			if (NpcModelAlpha != null)
			{
				NpcModelAlpha._GenFadeIn = true;
			}
		}
		
		//ScantargetCmpt
		internal void ClearEscape()
		{
			if (m_Target != null)
			{
				m_Target.ClearEscapeEnemy();
			}
		}

        internal void ClearEnemy()
        {
             if(m_Target != null)
             {
                 m_Target.ClearEnemy();
             }
        }
		

		internal void SetFloat(string name, float value)
		{
			if (m_Animator != null)
			{
				m_Animator.SetFloat(name, value);
			}
		}

        internal void CallHelp(float radius)
        {
            if(m_Target != null)
            {
                m_Target.CallHelp(radius);
            }
        }

        internal void SetEscapeEntity(PeEntity entity)
        {
            if(m_Target != null)
            {
                m_Target.SetEscapeEntity(entity);
            }
        }

        //MonsterCmpt
        internal void ActivateGravity(bool value)
        {
            if(m_Monster != null)
            {
                m_Monster.ActivateGravity(value);
            }
        }

        internal void Fly(bool value)
        {
            if (m_Monster != null)
            {
                m_Monster.Fly(value);
            }
        }

        //Motion_Move
        internal Seek AlterSeekBehaviour(Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
        {
            return m_Motor != null ? m_Motor.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight) : null;
        }

        internal Seek AlterSeekBehaviour(Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
        {
            return m_Motor != null ? m_Motor.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight) : null;
        }

        internal void MoveToPosition(Vector3 pos, SpeedState state = SpeedState.Walk,bool avoid = true)
		{
			if(m_Motor != null)
			{
				m_Motor.MoveTo(pos, state,avoid);
			}
		}

		internal bool ReachToPostion(Vector3 pos, SpeedState state = SpeedState.Walk)
		{
			if(m_Motor != null && m_Transform != null)
			{
				m_Motor.MoveTo(pos,state);			
				if(PEUtil.SqrMagnitudeH(m_Transform.position,pos) < 5.0f)				
					return true;
				else
					return false;
				
			}
			else
			{
				return false;
			}
		}
	
		internal bool IsReached(Vector3 pos, Vector3 targetPos,bool Is3D = false,float radiu = 1.0f)
		{
            float sqrDistanceH = PEUtil.Magnitude(pos, targetPos,Is3D);
			return sqrDistanceH < radiu;
		}
		
		internal void MoveDirection(Vector3 dir, SpeedState state = SpeedState.Walk)
		{
			if (m_Motor != null)
			{
				m_Motor.Move(dir, state);
			}
		}
		
		internal void FaceDirection(Vector3 dir)
		{
			if (m_Motor != null)
			{
				m_Motor.RotateTo(dir);
			}
		}

       
		
		internal void StopMove()
		{
			if (m_Motor != null)
			{
				m_Motor.Stop();
			}
		}
		
		internal void SetSpeed(float speed)
		{
			if (m_Motor != null)
			{
				m_Motor.SetSpeed(speed);
			}
		}
        //PeTrans
        //Network
		internal void SetPosition(Vector3 setPos,bool neeedrepair = true)
        {
			Vector3 pos = setPos;
            if (m_Transform != null)
            {
				if(!PEUtil.CheckErrorPos(pos))
				{
					Debug.LogError("[ERROR]Try to set error pos[" + pos.x + "," + pos.y + "," + pos.z + "] to entity " + m_Entity.name + " From  " +this.Name);
				}
               
				if (GameConfig.IsMultiClient)
				{
					NetworkInterface netObj = NetworkInterface.Get(entity.Id);

					if(netObj != null && !netObj.hasAuth)
					{
						if(entity.NpcCmpt != null)
						   entity.NpcCmpt.Req_Translate(pos);

						return ;
					}

                    if (null == netObj || !netObj.hasOwnerAuth)
						return;

					if (netObj is AiAdNpcNetwork)
					{
						AiAdNpcNetwork npc = (AiAdNpcNetwork)netObj;
#if UNITY_EDITOR
                        //if (VFVoxelTerrain.self != null && VFVoxelTerrain.self.Voxels != null && m_Entity.motionMgr != null && m_Entity.motionMgr.freezePhyState)
                        //{
                        //    bool inRange = LODOctreeMan.self._Lod0ViewBounds.Contains(pos);
                        //    if (inRange && VFVoxelTerrain.self.IsInTerrain(pos.x, pos.y + 0.4f, pos.z))
                        //    {
                        //        if (!inRange) Debug.LogError("[ERROR]Try to set OutOfLod0 pos[" + pos.x + "," + pos.y + "," + pos.z + "] to entity " + m_Entity.name);
                        //        else Debug.LogError("[ERROR]Try to set InTerrain pos[" + pos.x + "," + pos.y + "," + pos.z + "] to entity " + m_Entity.name);
                        //    }
                        //}
#endif
                        npc.RequestResetPosition(pos);
                        m_Transform.position = pos;
                        SceneMan.SetDirty(m_Entity.lodCmpt);
                        //npc.NpcMove();
                    }
				}
                else
                {
					if(neeedrepair)
					    pos = PETools.PEUtil.CorrectionPostionToStand(pos);

                    m_Transform.position = pos;
					m_Entity.DispatchOnTranslate(pos);
                    SceneMan.SetDirty(m_Entity.lodCmpt);
                }
            }
        }        

        //Network
        internal void SetRotation(Quaternion rot)
        {
            if (m_Transform != null)
            {
                m_Transform.rotation = rot;
            }
        }

        //NpcCmpt
        //Network
        internal void ClearNpcMount()
        {
            if (m_Npc != null)
            {
				m_Npc.MountID = 0;
            }
        }

        //Network
        internal void SetNpcUpdateCampsite(bool value)
        {
            if (m_Npc != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiAdNpcNetwork).RequestUpdateCampsite(value);
				}
           		m_Npc.UpdateCampsite = value;
            }
        }

        //Network
        internal void SetNpcState(ENpcState state)
        {
            if(m_Npc != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiAdNpcNetwork).RequestState((int)state);
				}
               	m_Npc.State = state;
            }
        }        

        //PassengerCmpt
        //Network
        internal void GetOn(WhiteCat.CarrierController clr, int index)
        {
            if(m_Passenger != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiAdNpcNetwork).RequestGetOn(clr.creationController.creationData.m_ObjectID,index);
				}

				if(PeGameMgr.IsSingle  && !entity.isRagdoll) 
					m_Passenger.GetOn(clr, index, false);
            }
        }

		internal void GetOn(int railRouteId,int entityId, bool checkState = true)
		{
			if(m_Passenger != null)
			{
				
				if (GameConfig.IsMultiMode)
				{
					RailwayOperate.Instance.RequestGetOnTrain(railRouteId, entityId);
				}
				else
				{
					if(!entity.isRagdoll)
					   RailwayOperate.Instance.RequestGetOnTrain(railRouteId, entityId);
				}

			}
		}
		
		//Network
		internal void GetOff()
		{
			if(m_Passenger != null)
            {
//				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
//				{
//
//				}

				if (GameConfig.IsMultiMode)
				{
					m_Passenger.GetOffCarrier();
				}
				else
				{
					if(!entity.isRagdoll)
					  m_Passenger.GetOffCarrier();
				}
               
            }
        }

		internal void GetOffRailRoute()
		{
			if(m_Passenger != null)
			{
				if (GameConfig.IsMultiMode)
				{
					RailwayOperate.Instance.RequestGetOffTrain(m_Passenger.railRouteId,m_Passenger.Entity.Id, Pathea.PeCreature.Instance.mainPlayer.position);
				}
				else
				{
					if(!entity.isRagdoll)
						RailwayOperate.Instance.RequestGetOffTrain(m_Passenger.railRouteId,m_Passenger.Entity.Id, Pathea.PeCreature.Instance.mainPlayer.position);
				}


			}
		}

        //TowerCmpt
        //Network
        internal void SetTowerAimPosition(Transform target)
        {
            if (entity.Tower != null && entity.Tower.Target != target)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					if (m_SkEntity._net is AiTowerNetwork)
						(m_SkEntity._net as AiTowerNetwork).RequestAimTarget(attackEnemy.entityTarget.Id);
					else if (m_SkEntity._net is MapObjNetwork)
						(m_SkEntity._net as MapObjNetwork).RequestAimTarget(attackEnemy.entityTarget.Id);
				}
               	entity.Tower.Target = target;
            }
        }

        //Network
        internal void TowerFire(SkEntity target)
        {
            if (entity.Tower != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
                    if (m_SkEntity._net is AiTowerNetwork)
                        (m_SkEntity._net as AiTowerNetwork).RequestFire(target.GetId());
                    else if (m_SkEntity._net is MapObjNetwork)
                        (m_SkEntity._net as MapObjNetwork).RequestFire(target.GetId());
                }
               	entity.Tower.Fire(target);
            }
        }

        //AnimatorCmpt
        //Network
        internal void SetBool(string name, bool value)
        {
			if(name.Length == 0 || m_Animator == null)
				return;
            if (m_Animator != null && m_Animator.GetBool(name) != value /*&& m_Animator.ContainsParameter(name)*/)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					if(m_SkEntity._net is AiNetwork)
					{
						//if(name == "Carry")
							(m_SkEntity._net as AiNetwork).RequestSetBool(Animator.StringToHash(name),value);
					}
				}
               	m_Animator.SetBool(name, value);
            }
        }

        //Network
        internal void SetTrigger(string name)
        {
            if (m_Animator != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiNetwork).RequestSetTrigger(name);
				}
               	m_Animator.SetTrigger(name);
            }
        }       

        //Network
        internal void SetMoveMode(MoveMode mode)
        {
            if (m_Motor != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiNetwork).RequestSetMoveMode((int)mode);
				}
               	m_Motor.mode = mode;
            }
        }

        //Motion_Equip
        //Network
        internal void HoldWeapon(IWeapon weapon, bool value)
        {
            if(weapon != null && !weapon.Equals(null))
			{
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
				}
               	weapon.HoldWeapon(value);
			}
        }



        //Network
        internal void SwitchHoldWeapon(IWeapon oldWeapon, IWeapon newWeapon)
        {
            if (m_Equipment != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
				}
               	m_Equipment.SwitchHoldWeapon(oldWeapon, newWeapon);
            }
        }

        //Network
        internal void SwordAttack(Vector3 dir)
        {
            if(m_Equipment != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiNetwork).RequestSwordAttack(dir);
				}
               	m_Equipment.SwordAttack(dir);
            }
        }

        //Network
        internal void TwoHandWeaponAttack(Vector3 dir, int handType = 0, int time = 0)
        {
            if (m_Equipment != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiNetwork).RequestTwoHandWeaponAttack(dir,handType,time);
				}
               	m_Equipment.TwoHandWeaponAttack(dir, handType, time);
            }
        }

        //UseItemCmpt
        internal void UseItem(ItemAsset.ItemObject item)
        {
            if (m_UseItem != null)
            {
               	m_UseItem.Use(item);
            }
        }

		internal float GetCdByProtoId(int protoId)
		{
			if (m_UseItem != null)
			{
				return  m_UseItem.GetCdByItemProtoId(protoId);
			}
			return 0.0f;
		}


        //IKAimCtrl
        //Network
        internal void SetIKAim(Transform aimTarget)
        {
            if (m_IKAim != null)
            {
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
				}
               	m_IKAim.SetTarget(aimTarget);
                m_IKAim.SetActive(null != aimTarget);
            }
        }


        internal void SetIkTarget(Transform aimTran)
        {
            if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
            {
                entity.IKCmpt.iKAimCtrl.SetTarget(aimTran);
            }
        }


        internal void SetIKTargetPos(Vector3 targetPos)
        {
            if(entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
            {
                SetIkTarget(null);
                entity.IKCmpt.iKAimCtrl.targetPos = targetPos;
            }
        }

        internal void SetIKActive(bool active)
        {
            if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
            {
                entity.IKCmpt.iKAimCtrl.SetActive(active);
            }
        }

        internal void SetIKLerpspeed(float lerpSpeed)
        {
            if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
            {
                entity.IKCmpt.iKAimCtrl.m_LerpSpeed = lerpSpeed;
            }
        }

        internal void SetIKFadeInTime(float time)
        {
            if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
            {
                entity.IKCmpt.iKAimCtrl.m_FadeInTime = time;
            }
        }

        internal void SetIKFadeOutTime(float time)
        {
            if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
            {
                entity.IKCmpt.iKAimCtrl.m_FadeOutTime = time;
            }
        }



        //viewcmpt
        //Network
        internal void Fadein(float time)
        {
            if (m_View != null)
			{
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiNetwork).RequestFadein(time);
				}
               	m_View.Fadein(time);
			}
        }
        
        internal void SetViewActive(bool value)
        {
            if(m_View != null && m_View.tView != null)
            {
                m_View.tView.gameObject.SetActive(value);
            }
        }

		//Network
		internal void Fadeout(float time)
		{
			if (m_View != null)
			{
				if(m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
				{
					(m_SkEntity._net as AiNetwork).RequestFadeout(time);
				}
				m_View.Fadeout(time);
			}
		}
        #endregion

        internal bool IsBlock()
        {
            if (attackEnemy != null)
            {
                RaycastHit[] hitInfos = Physics.RaycastAll(center, attackEnemy.centerPos - center, Vector3.Distance(center, attackEnemy.centerPos));
                for (int i = 0; i < hitInfos.Length; i++)
                {
                    if (hitInfos[i].collider.isTrigger)
                        continue;

                    if (hitInfos[i].transform.IsChildOf(entity.transform))
                        continue;

                    if (hitInfos[i].transform.IsChildOf(attackEnemy.trans))
                        continue;

                    return true;
                }
            }

            return false;
        }

        protected T GetCmpt<T>(Tree sender) where T : PeCmpt
        {
            return (sender.ActiveAgent as BehaveCmpt).gameObject.GetComponent<T>();
        }

        protected bool GetData<T>(Tree sender, ref T t)
        {
            if (m_TreeDataList.ContainsKey(sender.ActiveStringParameter))
            {
                try
                {
                    t = (T)m_TreeDataList[sender.ActiveStringParameter];
                    return t != null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(ex);
                    return false;
                }
            }
            else
            {
                //Debug.LogError("Do not find data : " + sender.ActiveStringParameter);
                return false;
            }
        }
    }

    [BehaveAction(typeof(BTIsNight), "IsNight")]
    public class BTIsNight : BTNormal
    {
         BehaveResult Tick(Tree sender)
        {
            if (GameConfig.IsNight)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsDarkInDaytime), "IsDarkInLight")]
    public class BTIsDarkInDaytime : BTNormal
    {
         BehaveResult Tick(Tree sender)
        {
            if (entity.IsDark && LightMgr.Instance.GetLight(entity.tr, entity.bounds) != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsFly), "IsFly")]
    public class BTIsFly : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (IsFly())
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTSucceed), "Succeed")]
    public class BTSucceed : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTFailure), "Failure")]
    public class BTFailure : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTCheckTime), "CheckTime")]
    public class BTCheckTime : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float checkTime;

            public float m_LastCheckTime;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCheckTime >= m_Data.checkTime)
            {
                m_Data.m_LastCheckTime = Time.time;
                return BehaveResult.Success;
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTCheckProbability), "CheckProbability")]
    public class BTCheckProbability : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float checkTime;
            [BehaveAttribute]
            public float prob;

            public float m_LastCheckTime;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCheckTime >= m_Data.checkTime)
            {
                m_Data.m_LastCheckTime = Time.time;

                if(Random.value < m_Data.prob)
                {
                    return BehaveResult.Success;
                }
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTProbability), "Probability")]
    public class BTProbability : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Random.value <= m_Data.prob)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTAnimatorBool), "AnimatorBool")]
    public class BTAnimatorBool : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float time = 0.0f;
            [BehaveAttribute]
            public bool value = false;

            public float m_StartTime = 0.0f;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            m_Data.m_StartTime = Time.time;
            SetBool(m_Data.anim, m_Data.value);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.time)
                return BehaveResult.Success;
            else
                return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTStopMove), "StopMove")]
    public class BTStopMove : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            StopMove();
            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTMoveDirection), "MoveDirection")]
    public class BTMoveDirection : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float time = 0.0f;
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float speed = 0.0f;
            [BehaveAttribute]
            public Vector3 anchor = Vector3.forward;

            public float m_StartTime;
            public float m_LastCooldownTime;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCooldownTime <= m_Data.cdTime)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_Data.m_StartTime = Time.time;

            MoveDirection(transform.TransformDirection(m_Data.anchor), SpeedState.Run);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime < m_Data.time)
                return BehaveResult.Running;

            m_Data.m_LastCooldownTime = Time.time;
            MoveDirection(Vector3.zero);
            return BehaveResult.Success;
        }

        void Reset(Tree sender)
        {
            SetSpeed(0.0f);
            MoveDirection(Vector3.zero);
        }
    }

    [BehaveAction(typeof(BTIsBurrow), "IsBurrow")]
    public class BTIsBurrow : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (GetBool("Burrow"))
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTBurrowIdle), "BurrowIdle")]
    public class BTBurrowIdle : BTNormal
    {
        BehaveResult Init(Tree sender)
        {
            bool isBurrow = !Enemy.IsNullOrInvalid(attackEnemy) || !Enemy.IsNullOrInvalid(escapeEnemy);
            if (isBurrow == GetBool("Burrow"))
                return BehaveResult.Success;

            StopMove();
            SetBool("Burrow", isBurrow);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (GetBool("Burrowing"))
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTSneakIdle), "SneakIdle")]
    public class BTSneakIdle : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;

            public float m_Time;
            public float m_StartTime;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy) || !Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            StopMove();
            SetBool("Snake", true);
            m_Data.m_StartTime = Time.time;
            m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy) || !Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
                return BehaveResult.Success;

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (GetBool("Snake"))
                SetBool("Snake", false);
        }
    }

    [BehaveAction(typeof(BTSneakAttack), "SneakAttack")]
    public class BTSneakAttack : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float enterRadius = 0.0f;
            [BehaveAttribute]
            public float exitMinRadius = 0.0f;
            [BehaveAttribute]
            public float exitMaxRadius = 0.0f;
            [BehaveAttribute]
            public float moveSpeed = 0.0f;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (attackEnemy.ThreatDamage > 0f)
                return BehaveResult.Failure;

            if (attackEnemy.DistanceXZ < m_Data.enterRadius)
                return BehaveResult.Failure;

            SetBool("Snake", true);
            SetSpeed(m_Data.moveSpeed);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (attackEnemy.ThreatDamage > 0f)
                return BehaveResult.Failure;

            if (attackEnemy.DistanceXZ < m_Data.exitMinRadius)
                return BehaveResult.Failure;

            if (attackEnemy.DistanceXZ > m_Data.exitMaxRadius)
                return BehaveResult.Failure;

            MoveToPosition(attackEnemy.position);
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (GetBool("Snake"))
            {
                SetBool("Snake", false);
                SetSpeed(0.0f);
            }
        }
    }
}