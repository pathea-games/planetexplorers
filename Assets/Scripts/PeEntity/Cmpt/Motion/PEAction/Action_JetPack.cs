using UnityEngine;
using System.Collections;

namespace Pathea
{
	[System.Serializable]
	public class Action_JetPack : PEAction 
	{
		public override PEActionType ActionType { get { return PEActionType.JetPack; } }

		public HumanPhyCtrl	m_PhyCtrl;

		PEJetPack m_JetPack;
		public PEJetPack jetPack
		{
			get { return m_JetPack; }
			set 
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_JetPack = value;
			}
		}

		public PEJetPackLogic jetPackLogic;

		public float m_RotAcc = 5f;

		Vector3 m_MoveDir;

		AudioController m_Audio;

		public void SetMoveDir(Vector3 moveDir)
		{
			m_MoveDir = moveDir;
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null != jetPackLogic && jetPackLogic.enCurrent >= jetPackLogic.m_EnergyThreshold)
			{
				return true;
			}
			return false;
		}

		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.JetPack, true);
			m_MoveDir = Vector3.zero;
			if(null != jetPack)
			{
				jetPack.m_EffectObj.SetActive(true);
				if(0 != jetPack.m_StartSoundID && null != jetPack.m_EffectObj)
					m_Audio = AudioManager.instance.Create(jetPack.m_EffectObj.transform.position,
					                                       jetPack.m_StartSoundID,
					                                       jetPack.m_EffectObj.transform);
			}

			if(null != jetPackLogic && null != m_PhyCtrl)
			{
				m_PhyCtrl.ResetSpeed(jetPackLogic.m_BoostHorizonalSpeed);
				m_PhyCtrl.desiredMovementDirection = m_PhyCtrl.currentDesiredMovementDirection;
			}

			motionMgr.StartCoroutine(ChangeAudio());

			if(null != anim)
				anim.ResetTrigger("EndJump");
		}

		private IEnumerator ChangeAudio()
		{
			yield return new WaitForSeconds(1.5f);
			if(!motionMgr.IsActionRunning(ActionType))
				yield break;

			if(null != m_Audio)
			{
				m_Audio.Delete(0.5f);
				m_Audio = null;
			}

			if(null != jetPack && null != jetPack.m_EffectObj && 0 != jetPack.m_SoundID)
			{
				m_Audio = AudioManager.instance.Create(jetPack.m_EffectObj.transform.position,
				                                       jetPack.m_SoundID,
				                                       jetPack.m_EffectObj.transform,
				                                       false, false);
				m_Audio.PlayAudio(0.5f);
			}
		}

		public override bool Update ()
		{
			if(PeGameMgr.IsMulti && null != entity.netCmpt && !entity.netCmpt.IsController)
				return false;

			if(null != jetPackLogic)
			{
				bool endAction = false;

				if(null != m_PhyCtrl)
				{
					if(m_PhyCtrl.grounded)
						endAction = true;
					else
					{
						m_PhyCtrl.m_SubAcc = (m_PhyCtrl.velocity.y < jetPackLogic.m_MaxUpSpeed) ? (Vector3.up * jetPackLogic.m_BoostPowerUp) : Vector3.zero;
						if(!m_PhyCtrl.spineInWater && null != anim)
							anim.SetTrigger("Fall");
					}
				}

				if(!endAction)
				{
					jetPackLogic.enCurrent = Mathf.Clamp(jetPackLogic.enCurrent - jetPackLogic.m_CostSpeed * Time.deltaTime, 0, jetPackLogic.enMax);
					if(jetPackLogic.enCurrent <= PETools.PEMath.Epsilon)
						endAction = true;
				}
				if(endAction)
				{
					EndImmediately();
					return true;
				}

				jetPackLogic.lastUsedTime = Time.time;
			}

			if(null != trans && m_MoveDir != Vector3.zero 
			   && !(motionMgr.GetMaskState(PEActionMask.AimEquipHold) 
			     || motionMgr.GetMaskState(PEActionMask.GunHold)
			     || motionMgr.GetMaskState(PEActionMask.BowHold)))
				trans.rotation = Quaternion.Lerp(trans.rotation, Quaternion.LookRotation(m_MoveDir, Vector3.up), m_RotAcc * Time.deltaTime);
			if(null != trans && null != m_PhyCtrl)
				m_PhyCtrl.desiredMovementDirection = Vector3.Lerp(m_PhyCtrl.desiredMovementDirection, m_MoveDir, 5f * Time.deltaTime);
			return false;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.JetPack, false);
			if(null != jetPackLogic)
				jetPackLogic.lastUsedTime = Time.time;

			if(null != jetPack && null != jetPack.m_EffectObj)
				m_JetPack.m_EffectObj.SetActive(false);

			if(null != m_Audio)
			{
				m_Audio.Delete();
				m_Audio = null;
			}

			if(null != m_PhyCtrl)
			{
				m_PhyCtrl.m_SubAcc = Vector3.zero;
				m_PhyCtrl.desiredMovementDirection = Vector3.zero;
			}

			if(null != anim)
				anim.ResetTrigger("Fall");
		}
	}
}