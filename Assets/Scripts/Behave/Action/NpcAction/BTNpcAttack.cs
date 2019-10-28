using UnityEngine;
using ItemAsset;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using PETools;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;

namespace Behave.Runtime
{
	[BehaveAction(typeof(BTSelectAttackWeapon),"SelectAttackWeapon")]
	public class BTSelectAttackWeapon : BTNormal
	{
		bool m_Attacked;
		int m_Index;
		float m_LastSwitchTime;
		float m_LastAttackTime;
		//float m_LastSightTime;
		float m_StartAttackTime;
		float m_LastRetreatTime;
		float m_LastChangeTime;
        float m_StartDefenceTime;
		IWeapon m_Weapon;
		AttackMode m_Mode;
		Vector3 m_Local;
		Vector3 m_RetreatLocal;
		Vector3 m_ChangeLocal;
		
		List<int> m_ModeIndex = new List<int>();
		
		Vector3 GetRetreatPos(Vector3 TargetPos,Transform selfTrans,float minr,float maxr)
		{
			Vector3 selfPos =selfTrans.position;
			Vector3 dir = (selfPos - TargetPos).normalized;
			Vector3 retreat = PEUtil.GetRandomPosition(selfTrans.position, dir, minr, maxr, -90.0f, 90.0f);
			if (PEUtil.CheckPositionUnderWater(retreat) || PEUtil.CheckPositionInSky(retreat))
			{
				dir = Random.value >0.5 ? selfTrans.right : -selfTrans.right;
				retreat = selfPos + dir * 20.0f;
			}
			
//			Vector3 newpos;
//			if(AiUtil.GetNearNodePosWalkable(retreat,out newpos))
//			{
//				return newpos;
//			}
			return retreat;
		}
		
		bool CanAttack(IWeapon weapon, int index)
		{
			AttackMode[] modes = weapon.GetAttackMode();
			if (index < 0 || index >= modes.Length)
				return false;
			
			AttackMode mode = modes[index];
			return mode.ignoreTerrain || !PEUtil.IsBlocked(entity, selectattackEnemy.entityTarget);
		}
		
		int SwitchAttackIndex(IWeapon weapon)
		{
			AttackMode[] modes = weapon.GetAttackMode();
			if (modes.Length == 0)
				return -1;
			
			m_ModeIndex.Clear();
			int length = modes.Length;
			
			if (entity.Group != null)
			{
				for (int i = 0; i < length; i++)
				{
					AttackMode mode = modes[i];
					
					if (mode.IsInCD()) continue;
					
					if (mode.type == AttackType.Ranged)
						m_ModeIndex.Add(i);
				}
			}
			else
			{
				for (int i = 0; i < length; i++)
				{
					AttackMode mode = modes[i];
					
					if (mode.IsInCD()) continue;
					
					if (selectattackEnemy.SqrDistanceLogic >= mode.minRange * mode.minRange
					    && selectattackEnemy.SqrDistanceLogic <= mode.maxRange * mode.maxRange)
						m_ModeIndex.Add(i);
				}
			}
			
			if (m_ModeIndex.Count == 0)
				return Random.Range(0, modes.Length);
			else
				return m_ModeIndex[Random.Range(0, m_ModeIndex.Count)];
		}
		
		Vector3 GetLocalCenterPos()
		{
			return selectattackEnemy.position + m_Local + Vector3.up * entity.maxHeight * 0.5f;
		}
		
		Vector3 GetLocalPos(Enemy e, AttackMode attack)
		{
            if (!PEUtil.IsBlocked(e.entity, e.entityTarget) && !Stucking() && !PEUtil.IsNpcsuperposition(e.entity, e))
				m_Local = Vector3.zero;
			else
			{
                if (m_Local == Vector3.zero || PEUtil.IsBlocked(e.entityTarget, GetLocalCenterPos()) || PEUtil.IsNpcsuperposition(GetLocalCenterPos(), e))
				{
					Vector3 local = Vector3.zero;
                    float ra = e.entityTarget.bounds.extents.x > e.entityTarget.bounds.extents.z ? e.entityTarget.bounds.extents.x : e.entityTarget.bounds.extents.z;
					for (int i = 0; i < 5; i++)
					{

                        Vector3 pos = PEUtil.GetRandomPositionOnGround(e.position, attack.minRange + ra, attack.maxRange + ra);
						Vector3 offCenter = pos + Vector3.up * entity.maxHeight * 0.5f;

                        if (!PEUtil.IsBlocked(e.entityTarget, offCenter) && !PEUtil.IsNpcsuperposition(offCenter, e))
						{
							local = pos - e.position;
							break;
						}
					}
					
					m_Local = local;
				}
			}
			
			return m_Local;
		}
		
		Vector3 GetMovePos(Enemy e)
		{
			return e.position + m_Local;
		}
		
		
		bool IsInEnemyFoward(Enemy enemy,PeEntity self)
		{
			Vector3 selfPos = self.position;
			Vector3 targetPos = enemy.position;
			
			Vector3 forward = enemy.entityTarget.peTrans.forward;
			Vector3 vec = (selfPos -targetPos).normalized ;
			
			float ang = Mathf.Abs(PETools.PEUtil.Angle(forward, vec));
			
			return ang <= 90.0f;
		}
		
		bool InRadiu(Vector3 self,Vector3 target,float radiu)
		{
			float sqrDistanceH = PEUtil.SqrMagnitudeH(self, target);
			return sqrDistanceH < radiu * radiu;
		}
		
		void DoStep()
		{
			Vector3 dir = position - selectattackEnemy.position;
			PEActionParamV paramV = PEActionParamV.param;
			paramV.vec = dir;
			DoAction(PEActionType.Step,paramV);
		}
		
		void DoSheid()
		{
			DoAction(PEActionType.HoldShield);
			Vector3 dir = selectattackEnemy.position - position ;
			FaceDirection(dir);
		}
		
		bool EndSheid()
		{
			if(IsMotionRunning(PEActionType.HoldShield))
			{
				EndAction(PEActionType.HoldShield);
			}
			return !IsMotionRunning(PEActionType.HoldShield);
		}
		
		bool CanStep()
		{
			Vector3 dir = position - selectattackEnemy.position;
			PEActionParamV param = PEActionParamV.param;
			param.vec = dir;
			return InRadiu(position, selectattackEnemy.position,3.0f) && IsInEnemyFoward(selectattackEnemy,entity) && CanDoAction(PEActionType.Step,param); 
		}
		
		bool RunAway()
		{
			if (InRadiu(entity.position, selectattackEnemy.position,3.0f))
			{
				Vector3 dir = position - selectattackEnemy.position;
				dir.y = 0;
				Vector3 pos = position + dir * 3.0f;
				MoveToPosition(pos,SpeedState.Sprint);
				return true;
			}
			return false;
			
		}

		//Only Npc
		IWeapon SwitchWeapon (Enemy e)
		{
			IWeapon tmpWeapon = null;
			if(!Enemy.IsNullOrInvalid(e) && entity.motionEquipment != null)
			{
				float minDis = Mathf.Infinity;
				
				List<IWeapon> weapons = entity.motionEquipment.GetCanUseWeaponList(entity);

				for (int i = 0; i < weapons.Count; i++)
				{
					if (!entity.motionEquipment.WeaponCanUse(weapons[i])) //|| !(Match(weapons[i], e)
						continue;
					
					float tmpMinDis = Mathf.Infinity;
					
					AttackMode[] modes = weapons[i].GetAttackMode();
					
					bool isBreak = false;
					
					for (int j = 0; j < modes.Length; j++)
					{
						//目标没有攻击目标或者攻击目标不是自己的时候用远程攻击！
						if (modes[j].type == AttackType.Ranged)
						{
							TargetCmpt targetCmpt = e.entityTarget.target;
							if (targetCmpt != null)
							{
								PeEntity targetEntity = targetCmpt.enemy != null ? targetCmpt.enemy.entityTarget : null;
								if (targetEntity == null || !targetEntity.Equals(entity))
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


		void AimTarget(IWeapon _weapon)
		{
			if(_weapon == null || _weapon.Equals(null))
				return ;

			
			m_Index = SwitchAttackIndex(_weapon);
			if (m_Index < 0 || m_Index >= _weapon.GetAttackMode().Length)
				return;
			
			m_Mode = _weapon.GetAttackMode()[m_Index];
			if(m_Mode == null)
				return ;

			IAimWeapon aimWeapon = _weapon as IAimWeapon;
			if (aimWeapon != null)
			{
				if (m_Mode.type == AttackType.Ranged)
				{
					aimWeapon.SetAimState(true);
					aimWeapon.SetTarget(selectattackEnemy.CenterBone);
				}
				else
				{
					aimWeapon.SetAimState(false);
					aimWeapon.SetTarget(null);
				}
			}

		}

        const float SpritTime = 5.0f;
        const float RunTime = 3.0f;
        float m_StartChaseTime = 0f;
        SpeedState CalculateChaseSpeed()
        {
            if (m_StartChaseTime == 0)
                m_StartChaseTime = Time.time;

            if (Time.time - m_StartChaseTime <= SpritTime)
                return SpeedState.Sprint;
            else if (Time.time - m_StartChaseTime > SpritTime && Time.time - m_StartChaseTime <= SpritTime + RunTime)
                return SpeedState.Run;
            else
                m_StartChaseTime = Time.time;
            
            return SpeedState.Run;
        }

		BehaveResult Init(Tree sender)
		{
			if (Enemy.IsNullOrInvalid(selectattackEnemy) || !entity.target.ContainEnemy(selectattackEnemy))
				return BehaveResult.Failure;

			m_Attacked = false;
			m_LastAttackTime = 0.0f;
			//m_LastSightTime = 0.0f;
			m_StartAttackTime = Time.time;
            m_StartDefenceTime = Time.time;
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{
            if (Enemy.IsNullOrInvalid(selectattackEnemy) || !entity.target.ContainEnemy(selectattackEnemy))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			entity.NpcCmpt.EqSelect.ClearSelect();
			entity.NpcCmpt.EqSelect.ClearAtkSelects();
			if(entity.NpcCmpt.EqSelect.SetSelectObjsAtk(entity,EeqSelect.combat))
			{
				entity.NpcCmpt.EqSelect.GetBetterAtkObj(entity,selectattackEnemy);
			}
			
			if(entity.NpcCmpt.EqSelect.BetterAtkObj != null && Weapon != null && !Weapon.Equals(null) && entity.NpcCmpt.EqSelect.BetterAtkObj != Weapon.ItemObj)
				return BehaveResult.Failure;

			if (!IsNpcBase && !SelectItem.MatchEnemyAttack(entity,selectattackEnemy.entityTarget))//entity.HPPercent < 0.3f && m_Mode.type == AttackType.Melee)
				return BehaveResult.Failure;


            ////是否被挡住
            bool _IsInSpSence = (Pathea.PeGameMgr.IsAdventure && RandomDungenMgr.Instance != null && RandomDungenMgrData.dungeonBaseData != null)
             || (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsSingle && Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand)
             || (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsTutorial && Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand)
             || (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand);
            bool _isBlock = !_IsInSpSence && PEUtil.IsBlocked(entity, selectattackEnemy.entityTarget);
            if (_isBlock)
            {
                Vector3 movePos = GetMovePos(selectattackEnemy);
                Vector3 v3 = position + transform.forward;
                bool _IsUnderBlock = PEUtil.IsUnderBlock(entity);
                bool _IsForwardBlock = PEUtil.IsForwardBlock(entity, existent.forward, 2.0f);
                if (_IsUnderBlock)
                {
                    if (_IsForwardBlock || _isBlock)
                    {
                        if (movePos.y >= v3.y) v3.y = movePos.y;
                        SetPosition(v3);
                    }
                    else
                    {
                        MoveDirection(movePos - position, SpeedState.Run);
                    }
                }
                else
                {
                    if (Stucking())
                    {
                        if (movePos.y >= v3.y) v3.y = movePos.y;
                        SetPosition(v3);
                    }
                    MoveToPosition(movePos, SpeedState.Run);
                }
                return BehaveResult.Running;
            } 

            if (Weapon == null || Weapon.Equals(null))
            {
				bool canSwitchWeapon = true;
				
				if (entity.motionMgr != null && entity.motionMgr.IsActionRunning(PEActionType.SwordAttack))
					canSwitchWeapon = false;
				
				if (entity.motionMgr != null && entity.motionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
					canSwitchWeapon = false;
				
				if (entity.motionEquipment.IsSwitchWeapon())
					canSwitchWeapon = false;
				
				if(entity.isRagdoll)
					canSwitchWeapon = false;
				
				if(entity.netCmpt != null && !entity.netCmpt.IsController)
					canSwitchWeapon = false;

				if(canSwitchWeapon)
				{
					IWeapon tempweapon = SwitchWeapon(selectattackEnemy);
					if (tempweapon != null && !tempweapon.Equals(null))
					{
						if (entity.motionEquipment.Weapon == null || entity.motionEquipment.Weapon.Equals(null))
						{
							//Vector3 forward = Vector3.ProjectOnPlane(entity.peTrans.trans.forward, Vector3.up);
							//Vector3 direction = Vector3.ProjectOnPlane(selectattackEnemy.Direction, Vector3.up);
							//float   angle = Vector3.Angle(forward, direction);
							//bool    canHold = (entity.Race != ERace.Puja && entity.Race != ERace.Paja) || angle < 45f;
							
							if (!tempweapon.HoldReady)
							{
                                StopMove();
								tempweapon.HoldWeapon(true);
								AimTarget(Weapon);
								return BehaveResult.Running;
							}
						}
						else
						{
							if (!entity.motionEquipment.Weapon.Equals(tempweapon))
							{
                                StopMove();
								entity.motionEquipment.SwitchHoldWeapon(entity.motionEquipment.Weapon, tempweapon);
								AimTarget(Weapon);
								return BehaveResult.Running;
							}
						}
					}
				}
				return BehaveResult.Running;
				
			}
			else
			{ 
				m_Index = SwitchAttackIndex(Weapon);
				if (m_Index < 0 || m_Index >= Weapon.GetAttackMode().Length)
					return BehaveResult.Failure;
				
				m_Mode = Weapon.GetAttackMode()[m_Index];
				if (Time.time - m_LastAttackTime <= m_Mode.frequency)
					return BehaveResult.Failure;

				if (Weapon == null || Weapon.Equals(null) || m_Mode == null)
					return BehaveResult.Failure;

				//只能使用拳套，则把装备栏里不能使用的装备收回去
				if(Weapon is PEGloves && entity.motionEquipment.ActiveableEquipment != null)			
					SelectItem.TakeOffEquip(entity);

				IAimWeapon aimWeapon = Weapon as IAimWeapon;
				if (aimWeapon != null)
				{
					if (m_Mode.type == AttackType.Ranged)
					{
						aimWeapon.SetAimState(true);
						aimWeapon.SetTarget(selectattackEnemy.CenterBone);
					}
					else
					{
						aimWeapon.SetAimState(false);
						aimWeapon.SetTarget(null);
					}
				}

                if (selectattackEnemy.entityTarget.target != null)
                {
                    int n = selectattackEnemy.entityTarget.monsterProtoDb != null && selectattackEnemy.entityTarget.monsterProtoDb.AtkDb != null ? selectattackEnemy.entityTarget.monsterProtoDb.AtkDb.mNumber : 3;
                    if (m_Mode.type == AttackType.Melee)
                        selectattackEnemy.entityTarget.target.AddMelee(entity, n);
                    else
                        selectattackEnemy.entityTarget.target.RemoveMelee(entity);
                }

                if (selectattackEnemy.GroupAttack == EAttackGroup.Threat)
                    return BehaveResult.Failure;

				float minRange = m_Mode.minRange;
				float maxRange = m_Mode.maxRange;
				float sqrDistanceXZ = selectattackEnemy.SqrDistanceLogic; //PETools.PEUtil.Magnitude(position,selectattackEnemy.position);
				Vector3 direction = selectattackEnemy.Direction;
				
				//是否被挡住
                bool isBlock = !m_Mode.ignoreTerrain && (PEUtil.IsBlocked(entity, selectattackEnemy.entityTarget) || PEUtil.IsNpcsuperposition(entity,selectattackEnemy));
				//距离是否可以攻击
				bool isRange = sqrDistanceXZ <= maxRange * maxRange  && sqrDistanceXZ >= minRange * minRange;
				//角度是否可以攻击
				bool isAngle = PEUtil.IsScopeAngle(direction, transform.forward, Vector3.up, m_Mode.minAngle, m_Mode.maxAngle);
				//是否可以攻击
				bool isAttack = isRange && isAngle && !isBlock;
				//是否瞄准
				bool isAimed = m_Mode.type == AttackType.Melee || aimWeapon == null || aimWeapon.Aimed;
				//是否需要调整站位
				bool ischangeStand = !isBlock && m_Mode.type == AttackType.Ranged && entity.target.beSkillTarget;
				//寻找可攻击位置
				m_Local = GetLocalPos(selectattackEnemy, m_Mode);
				m_Local = Vector3.ProjectOnPlane(m_Local, Vector3.up);
				//开始后退的距离
				float retreatRange = minRange;
				if (m_Mode.type == AttackType.Ranged)
					retreatRange += Mathf.Lerp(minRange, maxRange, 0.2f);

				//目标发动技能攻击（移动）
				bool isSkillAttacking = entity.target.beSkillTarget;
				if (Time.time - m_LastRetreatTime > 3.0f)
				{
					m_RetreatLocal = Vector3.zero;
					m_LastRetreatTime = Time.time;

					m_ChangeLocal = Vector3.zero;
					m_LastChangeTime = Time.time;
				}


				//攻击移动
				if (sqrDistanceXZ > maxRange * maxRange || isBlock)
				{
					Vector3 movePos = GetMovePos(selectattackEnemy);
					if(isBlock)
					{
						Vector3 v3 = position + transform.forward;
						if (Stucking())
						{ 
							if(movePos.y >= v3.y) v3.y = movePos.y;
							SetPosition(v3);
						}
						MoveToPosition(movePos,SpeedState.Run);
					}
					else
					{
						Vector3 dir0 = movePos - position;
						Vector3 dir = isSkillAttacking ? Vector3.Lerp(Vector3.Cross(dir0,Vector3.up),dir0,Time.time) : dir0;
                        SpeedState speed = CalculateChaseSpeed();

                        MoveDirection(dir, speed);
					}
				} 
				else  if (sqrDistanceXZ < retreatRange*retreatRange)
				{
					if (Time.time - m_LastRetreatTime < 2.0f)
					{
						if (m_RetreatLocal == Vector3.zero)
							m_RetreatLocal = GetRetreatPos(selectattackEnemy.position, transform,minRange,maxRange);
						
						FaceDirection(direction);
						MoveToPosition(m_RetreatLocal, SpeedState.Run);
					}
					else
						StopMove();
				}
				else if(ischangeStand)
				{
					if (Time.time - m_LastChangeTime < 2.0f)
					{
						if (m_ChangeLocal == Vector3.zero)
							m_ChangeLocal = GetRetreatPos(selectattackEnemy.position, transform,minRange,maxRange);
						
						FaceDirection(direction);
						MoveToPosition(m_ChangeLocal, SpeedState.Run);
					}
				}
				else
				{
					StopMove();
				}
				
				//攻击旋转
				if(!isBlock)
				{
					if (!isAngle || !isAimed){						
						FaceDirection(direction);
					}else{
						FaceDirection(Vector3.zero);
					}
				}

				if(selectattackEnemy.entityTarget.target != null)
				{
					Enemy TargetEnemy = selectattackEnemy.entityTarget.target.GetAttackEnemy();
					if (!Enemy.IsNullOrInvalid(TargetEnemy) &&  selectattackEnemy.entityTarget.IsAttacking
					    && TargetEnemy.entityTarget == entity && IsInEnemyFoward(selectattackEnemy,entity)
					    && Time.time - m_StartDefenceTime >= 3.0f)
					{
						m_StartDefenceTime = Time.time;
						bool canShield = CanDoAction(PEActionType.HoldShield);
						bool canStep = CanStep();
						if(canShield && canStep)
						{
							if(Random.value > 0.5f)
								DoStep();
							else			
								DoSheid();	
							
							m_Attacked = false;
							m_StartAttackTime = Time.time;
							return BehaveResult.Running;
						}else if(canShield)
						{
							DoSheid();
							m_Attacked = false;
							m_StartAttackTime = Time.time;
							return BehaveResult.Running;
						}else if(canStep)
						{
							DoStep();
							m_Attacked = false;
							m_StartAttackTime = Time.time;
							return BehaveResult.Running;
						}
					}
					
				}

				if(!m_Attacked)
				{
					//是否需要瞄准
					if (isAttack && isAimed)
					{
                        WeaponAttack(Weapon,selectattackEnemy, m_Index);
						//Weapon.Attack(m_Index);
						m_Attacked = true;
					}
					return BehaveResult.Running;
				}
				else
				{
                    //range need CD 
                    bool _IsCD = m_Mode != null && m_Mode.type == AttackType.Ranged ? Weapon.IsInCD() : false;
					//是否连击
                    if (isAttack && Weapon != null && !Weapon.Equals(null) && !_IsCD)
					{
						m_StartAttackTime = Time.time;        
						//Weapon.Attack(m_Index);
                        WeaponAttack(Weapon, selectattackEnemy,m_Index);
					}
					
					return BehaveResult.Running;
				}
            }
		}
		
		void Reset(Tree sender)
		{
			if(IsMotionRunning(PEActionType.HoldShield))
				EndAction(PEActionType.HoldShield);
			
			if(m_StartAttackTime > PETools.PEMath.Epsilon)
			{
				FaceDirection(Vector3.zero);
				
				m_StartAttackTime = 0.0f;
			}
			
			if(Enemy.IsNullOrInvalid(selectattackEnemy))
            {
                MoveDirection(Vector3.zero);
                StopMove();
            }
				
                    
            if(!Enemy.IsNullOrInvalid(selectattackEnemy) && entity != null && selectattackEnemy.entityTarget != null && selectattackEnemy.entityTarget.target != null)
                selectattackEnemy.entityTarget.target.RemoveMelee(entity);
		}
	}
}


