using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	[System.Serializable]
	public class Action_RopeGunShoot : PEAction 
	{
		public override PEActionType ActionType { get {	return PEActionType.RopeGunShoot; } }
		public HumanPhyCtrl	phyCtrl{ get; set; }

		PERopeGun mRopeGun;
		public PERopeGun ropeGun 
		{
			get { return mRopeGun; }
			set
			{
				mRopeGun = value;
				if(null != mRopeGun)
				{
					int dt = mLinePosList.Count - mRopeGun.lineList.Count;
					if(dt < 0)
					{
						for(int i = 0; i < -dt; ++i)
							mLinePosList.Add(Vector3.zero);
					}
					else if(dt > 0)
					{
						for(int i = 0; i < dt; ++i)
							mLinePosList.RemoveAt(0);
					}
				}
			}
		}
			
		enum HookGunState
		{
			Null,
			Shooting,
			Climbing,
			Back
		}
		HookGunState mState = HookGunState.Null;

		const string ClimbAnim = "RopeGunClimb";
		//static readonly float HumanWith = 0.4f;

		Vector3 m_ShootDir;
		int 	m_CurrentMoveIndex;
		AudioController m_Audio;
		float 	m_ExDoClimbTime;

		Vector3 m_LastHookPos;
		
		List<Vector3> mLinePosList = new List<Vector3>();

		public override bool CanDoAction (PEActionParam para)
		{
			return null != mRopeGun && viewCmpt.hasView && null != phyCtrl;
		}

		public override void DoAction (PEActionParam para)
		{
			if(null == mRopeGun) return;
			if(!viewCmpt.hasView) return;
			m_LastHookPos = mRopeGun.hook.position;
			motionMgr.SetMaskState(PEActionMask.RopeGunShoot, true);
			mState = HookGunState.Shooting;
			m_ShootDir = (ikCmpt.aimTargetPos - mRopeGun.gunMuzzle.position).normalized;
			m_CurrentMoveIndex = 0;
			PeCamera.SetFloat("Sensitivity Multiplier", 0);
			AudioManager.instance.Create(mRopeGun.gunMuzzle.position, mRopeGun.fireSound);
//			Collider[] hitInfos = Physics.OverlapSphere(mRopeGun.gunMuzzle.position, 0.2f, mRopeGun.effectLayer.value, QueryTriggerInteraction.Ignore);
//			for(int i = 0; i < hitInfos.Length; ++i)
//			{
//				if(!hitInfos[i].transform.IsChildOf(entity.transform))
//				{
//					mState = HookGunState.Null;
//					return;
//				}
//			}
		}

		public override bool Update ()
		{
			if(null == mRopeGun || null == phyCtrl || !viewCmpt.hasView || mState == HookGunState.Null)
			{
				OnEndAction();
				return true;
			}

			switch(mState)
			{
			case HookGunState.Shooting:
				UpdateShoot();
				break;
			case HookGunState.Climbing:
				UpdateClimbing();
				break;
			case HookGunState.Back:
				UpdateBack();
				break;
			}

			return false;
		}

		void UpdateShoot()
		{
			RaycastHit hitInfo;
			float moveDis = mRopeGun.bulletSpeed * Time.deltaTime;
			if(Physics.Raycast(m_LastHookPos, m_ShootDir, out hitInfo, 10, -1, QueryTriggerInteraction.Ignore)
			   && hitInfo.distance <= moveDis)
			{
				if((1 << hitInfo.transform.gameObject.layer & mRopeGun.effectLayer.value) != 0 
				   && Vector3.SqrMagnitude(hitInfo.point - mRopeGun.gunMuzzle.position) >= mRopeGun.minDis * mRopeGun.minDis)
				{
					mRopeGun.hook.position = m_LastHookPos = hitInfo.point;
					StartClimb();
				}
				else
					mState = HookGunState.Back;
				
				m_Audio = AudioManager.instance.Create(mRopeGun.gunMuzzle.position, mRopeGun.ropeSound, mRopeGun.gunMuzzle, true, false);
			}
			else
			{
				m_LastHookPos += moveDis * m_ShootDir;
				if(Vector3.SqrMagnitude(m_LastHookPos - mRopeGun.gunMuzzle.position) >= mRopeGun.range * mRopeGun.range)
				{
					mState = HookGunState.Back;					
					m_Audio = AudioManager.instance.Create(mRopeGun.gunMuzzle.position, mRopeGun.ropeSound, mRopeGun.gunMuzzle, true, false);
				}
			}
			UpdateLinePos();
		}

		void UpdateClimbing()
		{
			Vector3 projectPos = Vector3.zero;
			for(; m_CurrentMoveIndex < mLinePosList.Count - 1; ++m_CurrentMoveIndex)
			{
				Vector3 indexDir = mLinePosList[m_CurrentMoveIndex + 1] - mLinePosList[m_CurrentMoveIndex];
				Vector3 currentDir = mRopeGun.gunMuzzle.position - mLinePosList[m_CurrentMoveIndex];
				Vector3 projectDir = Vector3.Project(currentDir, indexDir);
				if(projectDir.sqrMagnitude < PETools.PEMath.Epsilon 
				   || Vector3.Angle(projectDir, indexDir) > 90f
				   || projectDir.sqrMagnitude < indexDir.sqrMagnitude)
				{
					projectPos = mLinePosList[m_CurrentMoveIndex] + projectDir;
					break;
				}
			}

			
			if(phyCtrl.velocity.magnitude > 5f * mRopeGun.climbSpeed || Vector3.SqrMagnitude(trans.position - m_LastHookPos) > 40000f)
			{
				trans.position = m_LastHookPos + 2f * Vector3.up;
				mState = HookGunState.Null;
				ResetPhy();
				return;
			}

			if(m_CurrentMoveIndex == mLinePosList.Count - 1 
			   || Vector3.SqrMagnitude(projectPos - m_LastHookPos) <= 0.25f
			   || (Time.time >= m_ExDoClimbTime && phyCtrl.grounded))
			{
				mState = HookGunState.Null;
				ResetPhy();
				return;
			}
						
			mRopeGun.hook.position = m_LastHookPos;
			Vector3 targetToProjectPos = projectPos - m_LastHookPos;
			Vector3 targetToHookPos = mRopeGun.gunMuzzle.position - m_LastHookPos;
			float angle = Vector3.Angle(targetToHookPos, targetToProjectPos);
			if(angle > PETools.PEMath.Epsilon)
			{
				Vector3 normal = Vector3.Cross(targetToProjectPos.normalized, targetToHookPos.normalized);
				Quaternion rot = Quaternion.AngleAxis(angle, normal);
				for(int i = m_CurrentMoveIndex; i < mLinePosList.Count - 1; ++i)
				{
					Vector3 dir = mLinePosList[i] - m_LastHookPos;
					dir = rot * dir;
					mLinePosList[i] = dir + m_LastHookPos;
				}
			}

			Vector3 moveDir = Vector3.Normalize(mLinePosList[m_CurrentMoveIndex + 1] - mLinePosList[m_CurrentMoveIndex]);
			phyCtrl.m_SubAcc = moveDir.y > 0 ? Physics.gravity.y * Vector3.down : Vector3.zero;
			phyCtrl.ResetSpeed(((moveDir.y > 0)?1f:mRopeGun.moveDownSpeedScale) * mRopeGun.climbSpeed);
			phyCtrl.desiredMovementDirection = moveDir;
			
			for(int i = 0;i < mRopeGun.lineList.Count; i++)
			{
				if(i < m_CurrentMoveIndex + 1)
					mRopeGun.lineList[i].localPosition = Vector3.zero;
				else
					mRopeGun.lineList[i].position = mLinePosList[i];
			}
		}

		void UpdateBack()
		{
			float moveDis = mRopeGun.bulletSpeed * Time.deltaTime;
			Vector3 dir = mRopeGun.gunMuzzle.position - m_LastHookPos;
			if(dir.sqrMagnitude <= moveDis * moveDis)
			{
				mState = HookGunState.Null;
				mRopeGun.hook.localPosition = Vector3.zero;
				ResetLine();
			}
			else
			{
				m_LastHookPos += moveDis * dir.normalized;
				UpdateLinePos();
			}
		}

		public override void EndImmediately ()
		{
			OnEndAction();
		}

		void UpdateLinePos()
		{
			Vector3 Dir = m_LastHookPos - mRopeGun.gunMuzzle.position;
			int lineCount = mRopeGun.lineList.Count;
			for(int i = 0;i < lineCount; i++)
			{
				Vector3 offsetPos = Dir * i / (lineCount - 1);
				mLinePosList[i] = mRopeGun.lineList[i].position = mRopeGun.gunMuzzle.position + offsetPos
					+ (1f - Mathf.Abs(i * 2f /lineCount - 1f) * Mathf.Abs(i * 2f /lineCount - 1f)) * Dir.magnitude * mRopeGun.lineDownF;
			}
		}

		void ResetLine()
		{
			if(null != ropeGun)
			{
				ropeGun.hook.localPosition = Vector3.zero;
				for(int i = 0; i < ropeGun.lineList.Count; ++i)
					ropeGun.lineList[i].localPosition = Vector3.zero;
			}
		}

		void OnEndAction()
		{
			motionMgr.SetMaskState(PEActionMask.RopeGunShoot, false);
			if(null != m_Audio) GameObject.Destroy(m_Audio.gameObject);
			PeCamera.SetFloat("Sensitivity Multiplier", 1f);
			if(Vector3.SqrMagnitude(trans.position - m_LastHookPos) > 40000f)
				trans.position = m_LastHookPos + 2f * Vector3.up;
			motionMgr.StartCoroutine(FixPhyError());
			ResetPhy();
			ResetLine();
		}

		void ResetPhy()
		{
			if(null != phyCtrl)
			{
				phyCtrl.useRopeGun = false;
				phyCtrl.m_SubAcc = Vector3.zero;
				phyCtrl.desiredMovementDirection = Vector3.zero;
				phyCtrl.ResetInertiaVelocity();
				phyCtrl.CancelMoveRequest();				
				phyCtrl._rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}

		void StartClimb()
		{			
			mState = HookGunState.Climbing;
			anim.SetBool(ClimbAnim, true);
			phyCtrl.m_SubAcc = Physics.gravity.y * Vector3.down;
			phyCtrl.useRopeGun = true;
			phyCtrl._rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			m_ExDoClimbTime = Time.time + mRopeGun.minClimbTime;
			Effect.EffectBuilder.Instance.Register(mRopeGun.hitEffectID, null, mRopeGun.hook.position, Quaternion.identity);
		}

		IEnumerator FixPhyError()
		{
			Vector3 lastPos = trans.position;
			yield return new WaitForSeconds(0.5f);
			if(Vector3.Distance(trans.position, m_LastHookPos) > 100f)
				trans.position = lastPos + Vector3.up;
		}
	}
}