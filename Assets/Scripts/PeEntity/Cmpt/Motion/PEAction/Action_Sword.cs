using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using Pathea.Effect;

namespace Pathea
{
	[Serializable]
	public class Action_SwordAttack : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.SwordAttack; } }
		
		public HumanPhyCtrl	phyMotor{ get; set; }
		public SkEntity targetEntity{ get; set; }

		PeSword		m_Sword;
		public PeSword sword
		{
			get { return m_Sword; }
			set 
			{
				m_Sword = value;
				if(null == m_Sword)
					motionMgr.EndImmediately(ActionType);
			}
		}
		
		public float m_MoveAttackSpeed = 4f;
		public float m_SprintAttackSpeed = 6f;
		public float m_RotateAcc = 3f;

		[HideInInspector]
		public bool m_UseStamina = false;
		
		protected bool m_AttackInAir;
		protected bool m_AttackInWater;
		protected bool m_AttackInMove;
		protected bool m_AttackInSprint;
		
		protected Vector3 m_AttackDir;
		protected Vector3 m_PreDir;
		
		protected bool m_WaitInput;
		protected bool m_TstAttack;
		protected int m_Farmcount;
		
		protected SkInst  m_SkillInst;

		protected int	m_AttackModeIndex;
		
		PEAttackTrigger m_AttackTrigger;
		Collider 		m_AttackCol;

		public LayerMask m_AttackLayer;
		[HideInInspector]
		public bool		firstPersonAttack = false;
		public float	m_AttackHeight = 1.5f;
		public float	m_LockMaxAngle = 20f;
		public float	m_AttackChecRange = 3f;
		Vector3			m_AttackTargetDir;

		bool			m_EndAction;

		//AnimSpeedScale
		[Header("AnimSpeed")]
		public AnimationCurve animScaleCurve;
		[Range(0, 5f)]
		public float animScaleTime = 0.15f;
		public float animLerpF = 5f;
		float animScaleStartTime;
		//bool applyAnimSpeed;

		//Comb
		[Header("Comb")]
		bool attackInAnimTime = true;
		protected int 	m_CombTime;
		bool m_CombAttack;
//		bool m_AttackApplied;
		float m_LastAtackTime;
		//WeaponEffect
		GameObject m_EffectObj;

		EffectLateupdateHelperEX_Part1 m_EffectHelper;

		public override bool CanDoAction (PEActionParam para = null)
		{
			PEActionParamVVNN paramVVNN = para as PEActionParamVVNN;
			int attackModeIndex = paramVVNN.n2;
			if(attackModeIndex >= sword.m_AttackSkill.Length ||
			   attackModeIndex >= sword.m_AttackSkill.Length)
				return false;
			return true;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == skillCmpt || null == sword)
				return;
			PEActionParamVVNN paramVVNN = para as PEActionParamVVNN;
			trans.position = paramVVNN.vec1;
			m_AttackDir = paramVVNN.vec2;
			if(m_AttackDir == Vector3.zero)
				m_AttackDir = trans.forward;
			m_CombTime = paramVVNN.n1;
			m_AttackModeIndex = paramVVNN.n2;
			ApplySkill();
			anim.ResetTrigger("ResetFullBody");
			anim.SetBool("AttackLand", false);
			motionMgr.SetMaskState(PEActionMask.SwordAttack, true);
			m_WaitInput = false;
			m_TstAttack = false;
			m_EndAction = false;
			OnStartAttack();
		}
		
		public override void ResetAction (PEActionParam para = null)
		{
			if(!m_EndAction && m_WaitInput 
			   && (null != phyMotor && phyMotor.spineInWater) == m_AttackInWater
			   && motionMgr.GetMaskState(PEActionMask.InAir) == m_AttackInAir)
			{
				if(attackInAnimTime)
					m_CombAttack = true;
				else
					OnComboAttack();
			}
			PEActionParamVVNN paramVVNN = para as PEActionParamVVNN;
			m_PreDir = paramVVNN.vec2;
		}
		
		public override bool Update ()
		{
			if(null == skillCmpt || null == sword || EndAirAttack())
			{
				EndImmediately();
				return true;
			}

			UpdateAnimSpeed();

			UpdateEffect();

			if(!UpdateSkillState())
				return false;

			OnEndAction();
			return true;
		}

		public override void EndAction ()
		{
			m_EndAction = true;
		}
		
		public override void EndImmediately ()
		{
			m_EndAction = true;
			if(null != anim)
				anim.SetTrigger("ResetFullBody");
			
			if(null != phyMotor)
			{
				phyMotor.velocity = new Vector3(0, phyMotor.velocity.y, 0);
				phyMotor.ResetInertiaVelocity();
			}

			OnEndAction();
		}

		void StartEffect(int effectID = 0)
		{
			EffectBuilder.EffectRequest request = EffectBuilder.Instance.Register(effectID, null, viewCmpt.modelTrans);
			request.SpawnEvent += OnEffectSpawn;
		}

		void UpdateEffect()
		{
			if(null != m_EffectHelper)
			{
				for(int i = 0; i < m_EffectHelper.particleSystems.Length; i++)
					if(null != m_EffectHelper.particleSystems[i])
						m_EffectHelper.particleSystems[i].playbackSpeed = anim.speed;
			}
		}

		void ApplySkill()
		{			
			m_AttackInAir = motionMgr.GetMaskState(PEActionMask.InAir);
			m_AttackInWater = (null != phyMotor && phyMotor.spineInWater);
			if(m_AttackInAir)
				m_SkillInst = skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInAir);
			else if(m_AttackInWater)
				m_SkillInst = skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInWater);
			else
			{
				m_AttackInMove = null != move && move.velocity.magnitude > m_MoveAttackSpeed;
				m_AttackInSprint = null != move && move.velocity.magnitude > m_SprintAttackSpeed;
				if(m_AttackInSprint)
					m_SkillInst = skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInSprint);
				else if(m_AttackInMove)
					m_SkillInst = skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInMove);
				else
					m_SkillInst = skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillID);
			}
		}

		bool UpdateSkillState()
		{
			if(null != m_SkillInst && skillCmpt.IsSkillRunning(m_SkillInst.SkillID))
			{
				//				trans.rotation = anim.m_LastRot * trans.rotation;
				if(null != phyMotor)
				{
					Vector3 animVelocity = Vector3.ProjectOnPlane(anim.m_LastMove/Time.deltaTime, Vector3.up);
					Vector3 currentVelocity = Vector3.ProjectOnPlane(phyMotor.velocity, Vector3.up);
					Vector3 dVelocity = animVelocity - currentVelocity;
					if(dVelocity.magnitude >= 12f)
						animVelocity = dVelocity.normalized * 12f + currentVelocity;
					phyMotor.ApplyMoveRequest(animVelocity);
					if(m_AttackInAir)
						phyMotor.ApplyMoveRequest(anim.GetFloat("AirAttackDownSpeed") * Vector3.up);
				}
				if(ikCmpt && Vector3.zero != m_AttackTargetDir)
				{
					if(firstPersonAttack)
						m_AttackTargetDir = Vector3.Normalize(ikCmpt.aimTargetPos - PETools.PEUtil.MainCamTransform.position);
					else
					{
						Vector3 lookDir = m_AttackTargetDir;
						if(!firstPersonAttack)
							ikCmpt.aimTargetPos = trans.position + m_AttackHeight * Vector3.up + 10f * m_AttackTargetDir;
						else
						{
							lookDir = ikCmpt.aimTargetPos - (trans.position + m_AttackHeight * Vector3.up);
							ikCmpt.aimTargetPos += 10f * m_AttackTargetDir;
						}
						if(null == phyMotor || !phyMotor.spineInWater)
							lookDir.y = 0;
						trans.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
					}
				}
				else if(m_AttackDir != Vector3.zero)
					trans.rotation = Quaternion.Lerp(trans.rotation, Quaternion.LookRotation(m_AttackDir), m_RotateAcc * Time.deltaTime);
				
				return false;
			}
			else if(!m_EndAction && m_UseStamina)
			{
				ApplySkill();
				return false;
			}
			return true;
		}
		
		public bool CheckContinueAttack()
		{
			if(m_CombTime > 0)
			{
				m_TstAttack = true;
				m_CombTime--;
			} 
			if(m_TstAttack)
			{
				m_AttackDir = m_PreDir;
				if(m_AttackDir == Vector3.zero)
					m_AttackDir = trans.forward;
				m_TstAttack = false;
				return true;
			}
			return false;
		}
		
		bool EndAirAttack()
		{
			if(null != m_SkillInst && m_AttackInAir)
				return null == move || move.state != MovementState.Air;
			 
			return false;
		}

		public Vector3 GetHitPos()
		{
			if(null != m_SkillInst)
				return m_SkillInst.GetCollisionContactPoint();
			return Vector3.zero;
		}

		void CostStamina(int attackModeIndex)
		{
			if(m_UseStamina && null != sword && null != skillCmpt && null != sword.m_StaminaCost && sword.m_StaminaCost.Length > attackModeIndex)
			{
				float newValue = skillCmpt.GetAttribute(AttribType.Stamina) - sword.m_StaminaCost[attackModeIndex] * motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent);
//				newValue = Mathf.Clamp(newValue, 0f, skillCmpt.GetAttribute(AttribType.StaminaMax));
				skillCmpt.SetAttribute(AttribType.Stamina, newValue, false);
			}
		}

		void ActiveCol(string colName)
		{
			if(null != anim.animator)
			{
				Transform trans = PETools.PEUtil.GetChild(anim.animator.transform, colName);
				if(null != trans)
				{
					PEAttackTrigger findTrigger = trans.GetComponent<PEAttackTrigger>();
					if(null != findTrigger)
					{
						if(null != m_AttackTrigger)
						{
							if(findTrigger != m_AttackTrigger)
							{
								m_AttackTrigger.ClearHitInfo();
								m_AttackTrigger.onHitTrigger -= OnHitTrigger;
								m_AttackTrigger.active = false;
								m_AttackTrigger = findTrigger;
								m_AttackTrigger.onHitTrigger += OnHitTrigger;
								m_AttackTrigger.active = true;
							}
							else
							{
								m_AttackTrigger.ResetHitInfo();
								m_AttackTrigger.active = true;
							}
						}
						else
						{
							m_AttackTrigger = findTrigger;
							m_AttackTrigger.onHitTrigger += OnHitTrigger;
							m_AttackTrigger.active = true;
						}
					}
					else
					{
						m_AttackCol = trans.GetComponent<Collider>();
						if(null != m_AttackCol)
							m_AttackCol.enabled = true;
					}
				}
			}
		}

		void InactiveCol()
		{
			if(null != m_AttackTrigger)
			{
				m_AttackTrigger.ResetHitInfo();
				m_AttackTrigger.active = false;
			}
			else if(null != m_AttackCol)
			{
				m_AttackCol.enabled = false;
				m_AttackCol = null;
			}
		}
		
		void OnStartAttack()
		{
			CostStamina(m_AttackModeIndex);
			ChangeAttackTarget();
			if(null != sword && null != sword.m_AttackMode && sword.m_AttackMode.Length > m_AttackModeIndex)
			if(null != sword && null != sword.m_AttackMode && sword.m_AttackMode.Length > m_AttackModeIndex)
			{
				motionMgr.Entity.SendMsg(EMsg.Battle_Attack, sword.m_AttackMode[m_AttackModeIndex]);
				motionMgr.Entity.SendMsg(EMsg.Battle_OnAttack, sword.m_AttackMode[m_AttackModeIndex], sword.transform, 0);
			}
			m_CombAttack = false;
//			m_AttackApplied = false;
			//applyAnimSpeed = true;
			m_LastAtackTime = Time.time;
		}

		void OnComboAttack()
		{
			m_WaitInput = false;
			m_TstAttack = true;
			if(null != m_SkillInst)
				m_SkillInst.SkipWaitAll = true;
			OnStartAttack();
		}

		void OnEndAction()
		{
			if(null != skillCmpt && null != m_SkillInst && skillCmpt.IsSkillRunning(m_SkillInst.SkillID))
				skillCmpt.CancelSkillById(m_SkillInst.SkillID);

			motionMgr.SetMaskState(PEActionMask.SwordAttack, false);
			m_WaitInput = false;
			m_TstAttack = false;
			m_SkillInst = null;
			m_AttackTargetDir = Vector3.zero;

			if(null != phyMotor)
			{
				phyMotor.CancelMoveRequest();
				phyMotor.desiredMovementDirection = Vector3.zero;
			}
			
			if(null != m_AttackTrigger)
			{
				m_AttackTrigger.ClearHitInfo();
				m_AttackTrigger.onHitTrigger -= OnHitTrigger;
				m_AttackTrigger.active = false;
				m_AttackTrigger = null;
			}
			else if(null != m_AttackCol)
			{
				m_AttackCol.enabled = false;
				m_AttackCol = null;
			}

			if(null != ikCmpt)
			{
				ikCmpt.aimActive = false;
				if(null != ikCmpt.m_IKAimCtrl)
					ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
			}

			anim.speed = 1f;
			
			DestroyEffect();
		}

		void ChangeAttackTarget()
		{
			m_AttackTargetDir = Vector3.zero;
			if(null != ikCmpt && null != trans)
			{
				Vector3 checkCenterPos = trans.position + m_AttackHeight * Vector3.up;

				if(firstPersonAttack)
				{
					m_AttackTargetDir = Vector3.Normalize(ikCmpt.aimTargetPos - PETools.PEUtil.MainCamTransform.position);
				}
				else
				{
					float minDis = 100f;
					List<ViewCmpt> viewCmpts = new List<ViewCmpt>();
					Collider[] findCols = Physics.OverlapSphere(checkCenterPos, m_AttackChecRange, m_AttackLayer.value);
					Vector3 attackDirH = m_AttackDir;
					attackDirH.y = 0;
					int selfPlayerID = Mathf.RoundToInt(motionMgr.Entity.GetAttribute(AttribType.DefaultPlayerID));
					foreach(Collider col in findCols)
					{
						BiologyViewCmpt findView = col.GetComponentInParent<BiologyViewCmpt>();
						if(null != findView && viewCmpt != findView && !viewCmpts.Contains(findView))
						{
							viewCmpts.Add(findView);
							if(findView.IsRagdoll)
								continue;
							if(!ForceSetting.Instance.Conflict(selfPlayerID, Mathf.RoundToInt(findView.Entity.GetAttribute(AttribType.DefaultPlayerID))))
								continue;

							PEDefenceTrigger defencetrigger = col.GetComponent<PEDefenceTrigger>();
							if(null == defencetrigger)
							{
								Transform centerTrans = findView.centerTransform;
								Vector3 dir = centerTrans.position - checkCenterPos;
								float dis = dir.magnitude;
								dir.y = 0;
								if(null != centerTrans && dis < minDis && Vector3.Angle(dir, attackDirH) < m_LockMaxAngle)
								{
									minDis = dis;
									m_AttackTargetDir = Vector3.Normalize(centerTrans.position - checkCenterPos);
								}
							}
							else
							{
								PECapsuleHitResult result;
								if(defencetrigger.GetClosest(checkCenterPos, m_AttackChecRange, out result))
								{
									Vector3 dir = result.hitPos - checkCenterPos;
									float dis = dir.magnitude;
									Vector3 dirH = dir;
									dirH.y = 0;

									if(dis < minDis && Vector3.Angle(dirH, attackDirH) < m_LockMaxAngle)
									{
										float angle = Vector3.Angle(dir, attackDirH);
										if(angle < m_LockMaxAngle)
											m_AttackTargetDir = dir.normalized;
										else
											m_AttackTargetDir = Vector3.Slerp(attackDirH.normalized, dir.normalized, m_LockMaxAngle / angle);
									}
								}
							}
						}
					}
				}
				
				ikCmpt.aimActive = Vector3.zero != m_AttackTargetDir;
				if(ikCmpt.aimActive && null != ikCmpt.m_IKAimCtrl)
					ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
			}
		}

		void UpdateAnimSpeed()
		{
			if(null != anim && null != sword)
			{
				float dtTime = Time.time - animScaleStartTime;
//				float animScale = applyAnimSpeed?sword.m_AnimSpeed:1f;//;(1f - (1f - sword.m_AnimSpeed) *(1f - anim.GetFloat("AnimSpeed"))):1f;
//				if(applyAnimSpeed)
//				if(m_UseStamina && entity.GetAttribute(AttribType.Stamina) <= sword.m_AnimDownThreshold)
//					animScale = applyAnimSpeed?sword.m_LowAnimSpeed:1f;
				float stamina = entity.GetAttribute(AttribType.Stamina);
				float animSpeed = sword.m_AnimSpeed;
				for(int i = 0; i < sword.m_AnimDownThreshold.Length; ++i)
					if(stamina <= sword.m_AnimDownThreshold[i])
						animSpeed *= 0.9f;
				animSpeed = Mathf.Clamp(animSpeed, 0.5f, 1.5f);
//				Debug.LogError("AnimSpeedScale:" + anim.GetFloat("AnimSpeedScale") + "\t\t" + "animScale:" + animScale);
				if(dtTime < animScaleTime)
					anim.speed = animScaleCurve.Evaluate(dtTime / animScaleTime) * animSpeed;
				else
					anim.speed = animSpeed;
				skillCmpt._lastestTimeOfConsumingStamina = Time.time;
			}
		}
		
		void OnHitTrigger(PEDefenceTrigger hitTrigger, PECapsuleHitResult result)
		{
			if(hitTrigger.transform.IsChildOf(motionMgr.transform))
				return;
			animScaleStartTime = Time.time;
//			PESkEntity skEntity = hitTrigger.GetComponentInParent<PESkEntity>();
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(null != sword && motionMgr.IsActionRunning(ActionType))
			{
				string[] subStrs = eventParam.Split('_');

				bool fpEvent = subStrs[0] == "FP";
				bool tpEvent = subStrs[0] == "TP";
				bool specialEvent = (fpEvent || tpEvent);

				if(specialEvent && fpEvent != firstPersonAttack)
					return;

				string effectEvent = specialEvent ? subStrs[1] : subStrs[0];
				
				switch(effectEvent)
				{
				case "AnimEffect":
					StartEffect(Convert.ToInt32(specialEvent ? subStrs[2] : subStrs[1]));
					break;
				case "StartAttack":
					if(null != m_SkillInst)
						m_SkillInst.SkipWaitPre = true;
//					m_AttackApplied = true;					
					ActiveCol(specialEvent ? subStrs[2] : subStrs[1]);
					m_Farmcount = Time.frameCount;
					break;
				case "WeightInputStart":
					m_WaitInput = true;
					break;
				case "WeightInputEnd":
					m_WaitInput = false;
					break;
				case "EndAttack":
					m_WaitInput = false;
					motionMgr.StartCoroutine(OnEndAttack());
					break;
				case "MonsterEndAttack":					
					if(null != m_SkillInst)
						m_SkillInst.SkipWaitAll = true;
					break;
				case "EndAction":
				case "OnEndFullAnim":
					if(Time.time - m_LastAtackTime >= 0.3f) //for recive event right after combo
					{
						m_EndAction = true;
						m_CombAttack = false;

						if(null != m_SkillInst && null != skillCmpt)
						{
							skillCmpt.CancelSkillById(m_SkillInst.SkillID);
							m_SkillInst = null;
						}
					}
					break;
				}
			}
		}

		private IEnumerator OnEndAttack()
		{
			while(Time.frameCount < m_Farmcount + 2)
				yield return null;
			if(null != m_SkillInst)
				m_SkillInst.SkipWaitMain = true;
			//applyAnimSpeed = false;
			InactiveCol();
			if(attackInAnimTime && m_CombAttack && !m_EndAction)
				OnComboAttack();
		}
		
		void DestroyEffect()
		{
			if(null != m_EffectObj)
				GameObject.Destroy(m_EffectObj);
			m_EffectObj = null;
		}

		void OnEffectSpawn(GameObject obj)
		{
			DestroyEffect();
			
			if(m_EndAction)
			{
				GameObject.Destroy(obj);
				return;
			}
			
			m_EffectObj = obj;
			m_EffectHelper = obj.GetComponent<EffectLateupdateHelperEX_Part1>();
			UpdateEffect();
		}
	}
}