using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Pathea;

namespace Behave.Runtime.Action
{
	[BehaveAction(typeof(BTDodge),"Dodge")]
	public class BTDodge : BTNormal
	{

		BehaveResult Tick(Tree sender)
		{
			if(attackEnemy == null ||  attackEnemy.entityTarget == null ||  attackEnemy.entityTarget.target == null)
				return BehaveResult.Failure;

			Enemy TargetEnemy = attackEnemy.entityTarget.target.GetAttackEnemy();
			if(TargetEnemy == null)
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Dodge))
				return BehaveResult.Failure;

			if(attackEnemy.SqrDistanceXZ <1.0f && attackEnemy.entityTarget.IsAttacking && TargetEnemy.entityTarget == entity)//) //&& Random.value >= 0.3f
			{
				float value = Random.value >0.5f ? -1.0f : 1.0f;
				Vector3 ward = Random.value >0.5f ?  value * transform.right : -transform.forward;
				
				if(Random.value >= 0.3f && attackEnemy.entityTarget.IsAttacking)
				{
					PEActionParamV param = PEActionParamV.param;
					param.vec = ward;
					DoAction(PEActionType.Step, param);
				}
				
				return BehaveResult.Success;
			}
			return BehaveResult.Failure;
		}
	}
	
	[BehaveAction(typeof(BTBlock),"Block")]
	public class BTBlock : BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			
			if(Random.value <= 0.7f)
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Block))
				return BehaveResult.Failure;
			
			DoAction(PEActionType.HoldShield);
			return BehaveResult.Success;
			
		}
	}
	
	
	[BehaveAction(typeof(BTSelfDefense),"SelfDefense")]
	public class BTSelfDefense : BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			return BehaveResult.Failure;
		}
	}
	
	[BehaveAction(typeof(BTAssist),"Assist")]
	public class BTAssist : BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			return BehaveResult.Failure;
		}
	}

	[BehaveAction(typeof(BTRecourse),"Recourse")]
	public class BTRecourse : BTNormal
	{
		//float weight =0.0f;
		PeTrans playerTrans;
		//List<Vector3> mdirs= new List<Vector3>();
		//Vector3 hidePos = Vector3.zero;

//		bool specialRecurse = false;
//		float specialRecurseTime = 5.0f;
//		float specialRecursestartTime;
		class Data
		{

		}
		
		Data m_Data;
		

		float hideStarTime = 0.0f;
		float HIDE_TIME = 1.0f;
		FindHidePos mfind;
		Vector3 mdir;

		float checkStarTime = 0.0f;
		float Check_TIME = 3.0f;
		BehaveResult Init(Tree sender)
		{
			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Recourse))
				return BehaveResult.Failure;
			
			float Hp = GetAttribute(AttribType.Hp);
			float HpMax =GetAttribute(AttribType.HpMax);

//			Request req = GetRequest(EReqType.FollowPath) as RQFollowPath;
//			if(req != null && !req.CanRun())
//			{
//				specialRecurse = true;
//				specialRecursestartTime = Time.time;
//			}
//			else
//			{
//				specialRecurse = false;
//				specialRecursestartTime = -1;
//				if(req != null && req.CanRun())
//				{
//					if(entity.target != null && entity.target.GetAttackEnemy() != null)
//						entity.target.ClearEnemy();
//
//					return BehaveResult.Success;
//				}
//			}

			if(Hp/HpMax > 0.25f) //!specialRecurse 
				return BehaveResult.Failure;
			
			if(PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null && playerTrans == null)
				playerTrans =PeCreature.Instance.mainPlayer.peTrans;

			SetCambat(false);
			hideStarTime = Time.time;
			checkStarTime = Time.time;
			mfind = new Pathea.FindHidePos(8.0f,false);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{


			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Recourse))
				return BehaveResult.Failure;

//			Request req = GetRequest(EReqType.FollowPath) as RQFollowPath;
//			if(req != null && !req.CanRun())
//			{
//				specialRecurse = true;
//			}
//			else
//			{
//				specialRecurse = false;
//			}


			if(Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Success;

			if(Time.time - checkStarTime > Check_TIME)
			{
				checkStarTime = Time.time;
				if(!entity.NpcCmpt.HasEnemyLocked())
				{
					entity.target.ClearEnemy();
				}
			}

			float Hp = GetAttribute(AttribType.Hp);
			float HpMax =GetAttribute(AttribType.HpMax);
			if(Hp/HpMax >=0.3f) //!specialRecurse && 
			{
				return BehaveResult.Success;
			}

			if(Time.time -hideStarTime > HIDE_TIME)
			{
				mdir= mfind.GetHideDir(playerTrans.position,position,Enemies);
				hideStarTime = Time.time;
			}

			Vector3 dirPos = position + mdir.normalized * 10.0f;
			if(mfind.bNeedHide)
			{
				MoveToPosition(dirPos,SpeedState.Run);
			}
			else
			{
			  StopMove();
			  FaceDirection(playerTrans.position - position);
			}

			//吃到了合适的属性值或者没有合适的药品时停止
			ItemAsset.ItemObject mEatItem;
			if(NpcEatDb.IsContinueEat(entity,out mEatItem))
			{
				if(entity.UseItem.GetCdByItemProtoId(mEatItem.protoId) < PETools.PEMath.Epsilon)
				{
					UseItem(mEatItem);
				}

			}
			SetCambat(false);
			return BehaveResult.Running;
			
		}

		void Reset(Tree sender)
		{
			SetCambat(true);
		//	specialRecurse = false;
		}
	}

	[BehaveAction(typeof(BTAttackCheck),"AttackCheck")]
	public class BTAttackCheck :BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			if(Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(attackEnemy.entityTarget == null || attackEnemy.entityTarget.animCmpt == null || attackEnemy.entityTarget.target == null)
				return BehaveResult.Success;

            Vector3 dir = position - attackEnemy.position;
			PEActionParamV param = PEActionParamV.param;
			param.vec = dir;
			if(!CanDoAction(PEActionType.Step, param))
                return BehaveResult.Success;

            if (Weapon != null && Weapon.GetAttackMode() != null)
            {
                AttackMode[] atts = Weapon.GetAttackMode();
                if(atts.Length >0 && atts[0].type == AttackType.Ranged)
                    return BehaveResult.Success;
            }

			Enemy TargetEnemy = attackEnemy.entityTarget.target.GetAttackEnemy();
			if(TargetEnemy == null)
				return BehaveResult.Success;

			EAttackCheck check =(EAttackCheck)attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
			if(TargetEnemy.entityTarget == entity && check!= EAttackCheck.None)
				return BehaveResult.Failure;
			else
				return BehaveResult.Success;
		}
	}

	[BehaveAction(typeof(BTRunAway),"RunAway")]
	public class BTRunAway : BTNormal
	{
		class Data
		{
		   [BehaveAttribute]
		   public float  Radius;
		   [BehaveAttribute]
		    public float RunAwayTime;

			public EAttackCheck m_AttackCheck;
			public Vector3 m_Dirction;
			public float m_StarRunTime;
		}

		Data m_Data;
		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(attackEnemy) || attackEnemy.entityTarget == null || attackEnemy.entityTarget.animCmpt == null)
				return BehaveResult.Failure;

			m_Data.m_AttackCheck = (EAttackCheck)attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
			if(m_Data.m_AttackCheck == EAttackCheck.All)
			{
				m_Data.m_AttackCheck = Random.value >0.5f ? m_Data.m_AttackCheck = EAttackCheck.RunAway : m_Data.m_AttackCheck = EAttackCheck.Roll;
			}

			if(m_Data.m_AttackCheck != EAttackCheck.RunAway)
				return BehaveResult.Failure;

			float sqrDistanceH = PEUtil.SqrMagnitudeH(position, attackEnemy.position);
			if(sqrDistanceH > m_Data.Radius * m_Data.Radius)
				return BehaveResult.Failure;

			Vector3 dir = position - attackEnemy.position;
			m_Data.m_Dirction = position +dir*m_Data.Radius;
			m_Data.m_StarRunTime = Time.time;
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			m_Data.m_AttackCheck = (EAttackCheck)attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
			if(m_Data.m_AttackCheck != EAttackCheck.RunAway)
				return BehaveResult.Failure;

			if(!IsReached(position,m_Data.m_Dirction,false))
			{
				MoveToPosition(m_Data.m_Dirction,SpeedState.Run);
			}
			else
			{
			   StopMove();
			   FaceDirection(attackEnemy.position - position);
			}
	
//			if(Time.time - m_Data.m_StarRunTime >= m_Data.RunAwayTime)
//				m_Data.m_AttackCheck = EAttackCheck.None;
	
				return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
			if(m_Data != null)
			{
				m_Data.m_Dirction = Vector3.zero;
				m_Data.m_AttackCheck = EAttackCheck.None;
			}
		}

	}

	
	[BehaveAction(typeof(BTRunRoll),"RunRoll")]
	public class BTRunRoll : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float  Radius;
			[BehaveAttribute]
			public float RunAwayTime;

			public EAttackCheck m_AttackCheck;
			public bool m_HasRoll;
		}

        bool IsInEnemyFoward(Enemy enemy, PeEntity self)
        {
            Vector3 selfPos = self.position;
            Vector3 targetPos = enemy.position;

            Vector3 forward = enemy.entityTarget.peTrans.forward;
            Vector3 vec = (selfPos - targetPos).normalized;

            float ang = Mathf.Abs(PETools.PEUtil.Angle(forward, vec));

            return ang <= 90.0f;
        }

        bool InRadiu(Vector3 self, Vector3 target, float radiu)
        {
            float sqrDistanceH = PEUtil.SqrMagnitudeH(self, target);
            return sqrDistanceH < radiu * radiu;
        }

		Data m_Data;
		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(attackEnemy) || attackEnemy.entityTarget == null || attackEnemy.entityTarget.animCmpt == null)
				return BehaveResult.Failure;

		
			m_Data.m_AttackCheck = (EAttackCheck)attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
			if(m_Data.m_AttackCheck == EAttackCheck.All)
			{
				m_Data.m_AttackCheck = Random.value >0.5f ? m_Data.m_AttackCheck = EAttackCheck.RunAway : m_Data.m_AttackCheck = EAttackCheck.Roll;
			}

			if(m_Data.m_AttackCheck != EAttackCheck.Roll)
				return BehaveResult.Failure;

			if(m_Data.m_HasRoll)
				return BehaveResult.Failure;

            if (!InRadiu(position, attackEnemy.position, 3.0f))
                return BehaveResult.Failure;

            if (!IsInEnemyFoward(attackEnemy, entity))
                return BehaveResult.Failure;

			Vector3 dir = position - attackEnemy.position;
			PEActionParamV param = PEActionParamV.param;
			param.vec = dir;
			if(!CanDoAction(PEActionType.Step, param))
				return BehaveResult.Failure;

			PEActionParamV paramV = PEActionParamV.param;
			paramV.vec = dir;
			m_Data.m_HasRoll = DoAction(PEActionType.Step, paramV);

			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
			if(m_Data != null)
			{
				m_Data.m_AttackCheck = EAttackCheck.None;
				m_Data.m_HasRoll = false;
			}
		}

	}

//	[BehaveAction(typeof(BTRunJump),"RunJump")]
//	public class BTRunJump : BTNormal
//	{
//		class Data
//		{
//			[BehaveAttribute]
//			public float  Radius;
//			[BehaveAttribute]
//			public float RunAwayTime;
//
//			public EAttackCheck m_AttackCheck;
//			public bool m_HasJump;
//		}
//		
//		Data m_Data;
//		BehaveResult Tick(Tree sender)
//		{
//			if (!GetData<Data>(sender, ref m_Data))
//				return BehaveResult.Failure;
//
//			if(Enemy.IsNullOrInvalid(attackEnemy))
//				return BehaveResult.Failure;
//			
//			m_Data.m_AttackCheck = (EAttackCheck)attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
//			if(m_Data.m_AttackCheck != EAttackCheck.Jump)
//				return BehaveResult.Failure;
//			
//			if(m_Data.m_HasJump)
//				return BehaveResult.Failure;
//			
//			Vector3 dir = position - attackEnemy.position;
//			if(!CanDoAction(PEActionType.Jump))
//				return BehaveResult.Failure;
//
//			m_Data.m_HasJump = DoAction(PEActionType.Jump);
//			
//			return BehaveResult.Running;
//		}
//		
//		void Reset(Tree sender)
//		{
//			if(m_Data != null)
//			{
//				m_Data.m_AttackCheck = EAttackCheck.None;
//				m_Data.m_HasJump = false;
//			}
//		}
//	}


}