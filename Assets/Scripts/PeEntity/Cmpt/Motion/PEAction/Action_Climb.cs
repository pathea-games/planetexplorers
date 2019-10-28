using UnityEngine;

namespace Pathea
{
	[System.Serializable]
	public class Action_Climb : PEAction 
	{
		public override PEActionType ActionType { get { return PEActionType.Climb; } }

		public HumanPhyCtrl	m_PhyCtrl;

		const float UpperHeight = 2f;
		const float LowerHeight = -0.2f;
		const float AnimUpDis = 1.75f;
		const float HumanWith = 0.37f;
		const float StepHeight = 0.8f;
		const float	DisToLadder = 0.37f;

		float	m_ClimbDir;
		bool	m_EndClimb;
		bool	m_CheckLadder;
		ItemScript_ClimbLadder 	m_CurrentLadder;
		ItemScript_ClimbLadder	m_LowerLadder;
		ItemScript_ClimbLadder	m_UpperLadder;

		/// <summary>
		/// Set the move dir.up:1 down:-1
		/// </summary>
		public void SetMoveDir(float moveDir, bool checkLadder = false)
		{
			m_ClimbDir = moveDir;
			m_CheckLadder = checkLadder;
		}

		public override void DoAction (PEActionParam para = null)
		{
			m_ClimbDir = 0;
			motionMgr.SetMaskState(PEActionMask.Climb, true);
			if(null != trans)
			{
				PEActionParamVQN paramVQN = para as PEActionParamVQN;
				Quaternion ladderRot = paramVQN.q;
				ItemScript_ClimbLadder.OpSide opSide = (ItemScript_ClimbLadder.OpSide)paramVQN.n;
				switch(opSide)
				{
				case ItemScript_ClimbLadder.OpSide.Both:
					if(Vector3.Angle(trans.forward, ladderRot * Vector3.forward) > 90)
						trans.rotation = Quaternion.AngleAxis(180f, Vector3.up) * ladderRot;
					else
						trans.rotation = ladderRot;
					break;
				case ItemScript_ClimbLadder.OpSide.Backward:
					trans.rotation = Quaternion.AngleAxis(180f, Vector3.up) * ladderRot;
					break;
				case ItemScript_ClimbLadder.OpSide.Forward:
					trans.rotation = ladderRot;
					break;
				}
				trans.position = paramVQN.vec;
			}
			if(null != anim)
			{
				anim.ResetTrigger("ResetFullBody");
				anim.ResetTrigger("LadderUpEnd");
				anim.SetTrigger("LadderClimb");
			}
			motionMgr.FreezePhyState(GetType(), true);
			if(null != m_PhyCtrl)
			{
				m_PhyCtrl.velocity = Vector3.zero;
				m_PhyCtrl.CancelMoveRequest();
			}
			if(null != ikCmpt)
				ikCmpt.ikEnable = false;
			m_EndClimb = false;
		}

		public override bool Update ()
		{
			UpdateLadderState();
			return m_EndClimb;
		}

		public override void EndAction ()
		{
			motionMgr.SetMaskState(PEActionMask.Climb, false);
			if(null != trans)
				trans.position += -DisToLadder * trans.forward;
			motionMgr.FreezePhyState(GetType(), false);
			if(null != anim)
				anim.SetTrigger("ResetFullBody");
			if(null != ikCmpt)
				ikCmpt.ikEnable = true;
			m_EndClimb = true;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Climb, false);
			if(null != trans)
				trans.position += -DisToLadder * trans.forward;
			motionMgr.FreezePhyState(GetType(), false);
			if(null != anim)
				anim.SetTrigger("ResetFullBody");
			if(null != ikCmpt)
				ikCmpt.ikEnable = true;
		}

		public void UpdateLadderState()
		{
			if(null != trans)
				trans.position += anim.m_LastMove;
			if(null != anim)
			{
				if(m_CheckLadder)
				{
					if(m_ClimbDir < -0.5f)
					{
						RaycastHit[] hitInfos = Physics.RaycastAll(trans.position - 0.2f * trans.forward, Vector3.down, StepHeight, -1, QueryTriggerInteraction.Ignore);

						for(int i = 0; i < hitInfos.Length; i++)
						{
							if(null == hitInfos[i].collider.transform.GetComponentInChildren<ItemScript_ClimbLadder>())
							{
								motionMgr.EndAction(ActionType);
								return;
							}
						}
						
						m_CurrentLadder = GetLadder(trans.position, trans.forward);
						m_LowerLadder = GetLadder(trans.position + LowerHeight * Vector3.up, trans.forward);
							
						if(null == m_CurrentLadder && null == m_LowerLadder)
							motionMgr.EndAction(ActionType);

					}
					else if(m_ClimbDir > 0.5f)
					{
						
						RaycastHit[] hitInfos = Physics.RaycastAll(trans.position + AnimUpDis * Vector3.up - 0.2f * trans.forward, Vector3.up, StepHeight, -1, QueryTriggerInteraction.Ignore);
						
						for(int i = 0; i < hitInfos.Length; i++)
						{
							if(!hitInfos[i].collider.isTrigger && null == hitInfos[i].collider.transform.GetComponentInChildren<ItemScript_ClimbLadder>())
							{
								anim.SetFloat("LadderClimbDir", 0);
								return;
							}
						}

						m_CurrentLadder = GetLadder(trans.position + AnimUpDis * Vector3.up, trans.forward);
						m_UpperLadder = GetLadder(trans.position + UpperHeight * Vector3.up, trans.forward);
						if(null == m_CurrentLadder && null == m_UpperLadder)
						{
							if(null != anim)
								anim.SetTrigger("LadderUpEnd");
						}
					}
					else
					{
						m_CurrentLadder = GetLadder(trans.position, trans.forward);
						m_UpperLadder = GetLadder(trans.position + UpperHeight * Vector3.up, trans.forward);
						if(null == m_CurrentLadder && null == m_UpperLadder)
						{
							motionMgr.EndImmediately(ActionType);
						}
					}
				}
				
				anim.SetFloat("LadderClimbDir", m_ClimbDir);
			}
		}

		ItemScript_ClimbLadder GetLadder(Vector3 pos, Vector3 dir)
		{
			ItemScript_ClimbLadder retLadder = null;
			RaycastHit[] hitInfos = Physics.RaycastAll(pos - 0.5f * dir, dir, 1f);
			hitInfos = PETools.PEUtil.SortHitInfo(hitInfos);
			for(int i = 0; i < hitInfos.Length; i++)
			{
				RaycastHit hitInfo = hitInfos[i];
				if(hitInfo.distance < 1f)
				{
					retLadder = hitInfo.collider.transform.GetComponent<ItemScript_ClimbLadder>();
					if(null != retLadder)
						break;
				}
			}
			return retLadder;
		}

		
		void FixedHeight()
		{
			if(null != trans)
			{
				m_CurrentLadder = GetLadder(trans.position + StepHeight/2 * Vector3.up, trans.forward);
				if(null != m_CurrentLadder)
				{
					if(null != m_CurrentLadder)
					{
						Vector3 newPos = m_CurrentLadder.transform.position - trans.forward * m_CurrentLadder.m_LadderWith;
						float dy = (trans.position.y - m_CurrentLadder.transform.position.y + StepHeight/2) % StepHeight - StepHeight/2;
						newPos.y = trans.position.y - dy;
						trans.position = newPos;
					}
				}
			}
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				switch(eventParam)
				{
				case "EndClimb":
					motionMgr.EndAction(ActionType);
					break;
				case "OnHandLadder":
					FixedHeight();
					break;
				}
			}
		}
	}
}
