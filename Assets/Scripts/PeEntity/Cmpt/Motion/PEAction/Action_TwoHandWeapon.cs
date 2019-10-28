using UnityEngine;
using Pathea;

[System.Serializable]
public class Action_TwoHandWeaponHold : Action_HandChangeEquipHold
{
	public override PEActionType ActionType { get { return PEActionType.TwoHandSwordHold; } }

	bool m_SubWeaponHoldState = false;
	
	public PETwoHandWeapon twoHandWeapon
	{
		get { return handChangeEquipment as PETwoHandWeapon; }
		set { handChangeEquipment = value; }
	}
	public override void DoAction (PEActionParam para = null)
	{
		base.DoAction (para);
		m_SubWeaponHoldState = false;
	}

	public override void EndImmediately ()
	{
		base.EndImmediately ();
		if(null != twoHandWeapon)
			viewCmpt.Reattach(twoHandWeapon.m_LHandWeapon, twoHandWeapon.m_LHandPutOffBone);
	}

	protected override void OnAnimEvent (string eventParam)
	{
		if(motionMgr.IsActionRunning(ActionType))
		{
			switch(eventParam)
			{
			case "PutOnAnimEnd":
				m_PutOnAnimEnd = true;
				break;
			case "PutOffAnimEnd":
				m_PutOffAnimEnd = true;
				break;
			case "PutOnEquipment":
				ChangeHoldState(true);
				break;
			case "PutOffEquipment":
				ChangeHoldState(false);
				break;
			case "PutOnLHandEquipment":
				ChangeLHandHoldState(true);
				break;
			case "PutOffLHandEquipment":
				ChangeLHandHoldState(false);
				break;
			}
		}
	}

	void ChangeLHandHoldState(bool hold)
	{
		if(null == twoHandWeapon)
			return;
		if(hold)
		{
			if(!m_SubWeaponHoldState)
			{
				m_SubWeaponHoldState = true;
				if(null != viewCmpt)
					viewCmpt.Reattach(twoHandWeapon.m_LHandWeapon, twoHandWeapon.m_LHandPutOnBone);
			}
		}
		else
		{
			if(m_SubWeaponHoldState)
			{
				m_SubWeaponHoldState = false;
				if(null != viewCmpt)
					viewCmpt.Reattach(twoHandWeapon.m_LHandWeapon, twoHandWeapon.m_LHandPutOffBone);
			}
		}
	}
}

[System.Serializable]
public class Action_TwoHandWeaponPutOff : PEAction
{
	public override PEActionType ActionType { get { return PEActionType.TwoHandSwordPutOff; } }
	public override void DoAction (PEActionParam para = null)
	{
		motionMgr.EndAction(PEActionType.TwoHandSwordHold);
	}
}

[System.Serializable]
public class Action_TwoHandWeaponAttack : Action_SwordAttack
{
	public override PEActionType ActionType { get { return PEActionType.TwoHandSwordAttack; } }

//	public PETwoHandWeapon twoHandWeapon
//	{
//		get { return sword as PETwoHandWeapon; }
//		set { sword = value; }
//	}
//
//	public override void DoAction (IPEActionParam para = null)
//	{
//		if(null == skillCmpt || null == sword)
//			return;
//		if(null != trans)
//			trans.position = (Vector3)para[0];
//		m_AttackDir = (Vector3)para[1];
//		if(m_AttackDir == Vector3.zero)
//			m_AttackDir = trans.existent.forward;
//		int handType = (int)para[2];
//		m_CombTime = (int)para[3];
//		switch(handType)
//		{
//		case 0:
//			m_AttackInAir = m_MotionMgr.GetMaskState(PEActionMask.InAir);
//			if(m_AttackInAir)
//				m_SkillInst = skillCmpt.StartSkill(null, sword.m_SkillInAir, true);
//			else
//			{
//				m_AttackInMove = null != move && move.velocity.magnitude > m_MoveAttackSpeed;
//				m_AttackInSprint = null != move && move.velocity.magnitude > m_SprintAttackSpeed;
//				if(m_AttackInSprint)
//					m_SkillInst = skillCmpt.StartSkill(null, sword.m_SkillInSprint, true);
//				else if(m_AttackInMove)
//					m_SkillInst = skillCmpt.StartSkill(null, sword.m_SkillInMove, true);  
//				else
//					m_SkillInst = skillCmpt.StartSkill(null, sword.m_SkillID, true);
//			}
//			break;
//		case 1:
//			m_SkillInst = skillCmpt.StartSkill(null, twoHandWeapon.m_RHandSkillID, true);
//			break;
//		case 2:
//			m_SkillInst = skillCmpt.StartSkill(null, twoHandWeapon.m_LHandSkillID, true);
//			break;
//		}
//
//		anim.SetTrigger("ResetUpbody");
//		anim.SetBool("EndAttack", false);
//		anim.SetBool("AttackLand", false);
//		m_MotionMgr.SetMaskState(PEActionMask.SwordAttack, true);
//		m_WaitInput = false;
//		m_TstAttack = false;
//	}
}
