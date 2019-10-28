using UnityEngine;
using System;
using System.Collections;
using RootMotion.FinalIK.Demos;
using PEIK;
using SkillSystem;
using RootMotion.FinalIK;

namespace Pathea
{
	public class Motion_Beat : PeCmpt , IPeMsg
	{
		public enum EffectType
		{
			Null = 0,
			Whacked,
			Repulsed,
			Wentfly,
			Knocked
		}

		private SkAliveEntity	m_SkillCmpt;
        private PeTrans     	m_PeTrans;
		private HumanPhyCtrl	m_PhyCtrl;
		private MotionMgrCmpt	m_MotionMgr;

		private	IKAnimEffectCtrl m_IKAnimCtrl;

		public float m_RandomScale = 0.1f;

		public float m_ThresholdScaleInAir = 0.5f;

		BeatParam m_Param;
		float m_LastHitSoundTime;

		public Action_Whacked 	m_Whacked;
		public Action_Repulsed 	m_Repulsed;
		public Action_Wentfly 	m_Wentfly;
		public Action_Knocked 	m_Knocked;
		public Action_GetUp		m_GetUp;

		public Action_Death m_Death;
		public Action_AlienDeath m_AlienDeath;
		public Action_Revive m_Revive;

		public event Action<PeEntity> onHitTarget;

#if UNITY_EDITOR
		public bool m_WriteLog;
#endif

		public override void Start ()
		{
			base.Start ();
			m_SkillCmpt = Entity.aliveEntity;
            m_PeTrans = Entity.peTrans;

			m_Repulsed.m_Behave = Entity.GetCmpt<BehaveCmpt>();
			m_Repulsed.m_Move = Entity.GetCmpt<Motion_Move_Motor>();

			m_MotionMgr = Entity.motionMgr;
			if(null != m_MotionMgr)
			{
				m_MotionMgr.AddAction(m_Whacked);
				m_MotionMgr.AddAction(m_Repulsed);
				m_MotionMgr.AddAction(m_Wentfly);
				m_MotionMgr.AddAction(m_Knocked);
				m_MotionMgr.AddAction(m_GetUp);
				m_MotionMgr.AddAction(m_Death);
				m_MotionMgr.AddAction(m_AlienDeath);
				m_MotionMgr.AddAction(m_Revive);
			}
		}

		void ApplyHitEffect(Transform trans, Vector3 forceDir, float forcePower)
		{
			if(null == m_Param)
				return;

			float hitForce = forcePower * (1f + UnityEngine.Random.Range(-m_Param.m_RandomScale, m_Param.m_RandomScale));
			float hitWeight = m_Param.m_ForceToHitWeight.Evaluate(hitForce);
			float hitTime = m_Param.m_ForceToHitTime.Evaluate(hitForce);
			if(null != m_IKAnimCtrl)
				m_IKAnimCtrl.OnHit(trans, forceDir, hitWeight, hitTime);
		}

		void ApplyMoveEffect(SkEntity skEntity, Transform trans, Vector3 forceDir, float forcePower)
		{
			if(null == m_Param )
				return;
            float angle = Vector3.Angle(m_PeTrans.existent.forward, forceDir);
			float thresholdScale = m_Param.m_AngleThresholdScale.Evaluate(angle);
			forcePower *= m_Param.m_AngleForceScale.Evaluate(angle);

			thresholdScale *= m_MotionMgr.GetMaskState (PEActionMask.InAir) ? m_Param.m_ThresholdScaleInAir : 1f;

			EffectType spType = EffectType.Repulsed;
			if(forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdWhacked) * thresholdScale) spType = EffectType.Null;
			else if(forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdRepulsed) * thresholdScale) spType = EffectType.Whacked;
			else if(forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdWentfly) * thresholdScale) spType = EffectType.Repulsed;
			else if(forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdKnocked) * thresholdScale) spType = EffectType.Wentfly;
			else spType = EffectType.Knocked;

#if UNITY_EDITOR
			if(m_WriteLog)
			{
				Debug.LogError("ForcePower:" + forcePower);
				Debug.LogError("ThresholdRepulsed:" + m_SkillCmpt.GetAttribute(AttribType.ThresholdRepulsed).ToString());
				Debug.LogError(spType.ToString());
			}
#endif
			SkAliveEntity skAliveEntity = skEntity as SkAliveEntity;
			if(null != skAliveEntity)
			{
				switch(spType)
				{
				case EffectType.Null:
					ApplyHitEffect(trans, forceDir, forcePower);
					break;
				case EffectType.Whacked:
					m_MotionMgr.DoAction(PEActionType.Whacked);
					break;
				case EffectType.Repulsed:
					ApplyHitEffect(trans, forceDir, forcePower);
					PEActionParamVVF param = PEActionParamVVF.param;
					param.vec1 = m_PeTrans.position;
					param.vec2 = forceDir;
					param.f = forcePower;
					m_MotionMgr.DoAction(PEActionType.Repulsed, param);
					break;
				case EffectType.Wentfly:
				case EffectType.Knocked:
					PEActionParamVFNS paramVFNS = PEActionParamVFNS.param;
					paramVFNS.vec = forceDir;
					paramVFNS.f = forcePower;
					paramVFNS.n = skAliveEntity.Entity.Id;
					if(null != trans)
						paramVFNS.str = trans.name;
					else
						paramVFNS.str = "";
					m_MotionMgr.DoAction(PEActionType.Wentfly, paramVFNS);
					break;
				}
			}
		}

#if UNITY_EDITOR

		public bool testHit;
		public float hitPower = 2000f;

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			if(testHit && Input.GetMouseButtonDown(1))
				ApplyMoveEffect(Entity.skEntity, Entity.centerBone, Camera.main.transform.forward, hitPower);
		}

#endif

		void ApplyBeenHitSound(Transform trans)
		{
			if(!Entity.IsDeath())
			{
				switch(Entity.proto)
				{
				case EEntityProto.Monster:
					MonsterProtoDb.Item monsterProto = MonsterProtoDb.Get(Entity.ProtoID);
					if(monsterProto.beHitSound[0] > 0 
					   && Time.time - m_LastHitSoundTime >= monsterProto.beHitSound[monsterProto.beHitSound.Length - 1])
					{
						m_LastHitSoundTime = Time.time;
						Vector3 pos = (null != trans)?trans.position:Entity.position;
						AudioManager.instance.Create(pos, monsterProto.beHitSound[0]);
					}
					break;
				case EEntityProto.Player:
				case EEntityProto.Npc:
					PlayMaleAudio();
					break;
				}
			}
		}

		void PlayMaleAudio()
		{
			AudioManager.instance.Create(Entity.position, UnityEngine.Random.Range(935, 937));
		}

		public void Beat(SkEntity skEntity, Transform trans, Vector3 forceDir, float forcePower)
		{
//			ApplyHitEffect(col, forceDir, forcePower);
			ApplyMoveEffect(skEntity, trans, forceDir, forcePower);
			ApplyBeenHitSound(trans);
		}

		#region IPeMsg implementation

		public void OnMsg (EMsg msg, params object[] args)
		{
			switch (msg)
			{
	        case EMsg.View_Prefab_Build:
				BiologyViewRoot viewRoot = args[1] as BiologyViewRoot;
				m_Param = viewRoot.beatParam;
				m_PhyCtrl = viewRoot.humanPhyCtrl;
	            m_IKAnimCtrl = viewRoot.ikAnimEffectCtrl;
				m_Repulsed.phyCtrl = m_PhyCtrl;
				m_Wentfly.phyCtrl = m_PhyCtrl;
				m_Knocked.phyCtrl = m_PhyCtrl;
				m_Repulsed.m_Param = m_Param;
				break;
//		    case EMsg.View_Ragdoll_Getup_Finished:
//				if(null != m_Wentfly && m_MotionMgr.IsActionRunning(m_Wentfly.ActionType))
//				m_MotionMgr.EndImmediately(m_Wentfly.ActionType);
//				m_MotionMgr.EndImmediately(m_Revive.ActionType);
//				m_MotionMgr.EndImmediately(m_GetUp.ActionType);
//				m_MotionMgr.EndImmediately(m_Knocked.ActionType);
//			    break;
				
			case EMsg.View_Ragdoll_Fall_Finished:
				if(null != m_PhyCtrl)
					m_PhyCtrl.grounded = true;
				break;
			case EMsg.Battle_AttackHit:
				if(null != onHitTarget)
				{
					PeEntity hitEntity = ((PECapsuleHitResult)args[0]).hitTrans.GetComponentInParent<PeEntity>();
					onHitTarget(hitEntity);
				}
				break;
			}
		}

		#endregion
	}
}
