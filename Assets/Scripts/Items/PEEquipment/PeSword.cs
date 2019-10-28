using UnityEngine;
using System.Collections;
using Pathea;
using System;
using ItemAsset;

//[System.Serializable]
//public class 

public class PeSword : PEHoldAbleEquipment , IWeapon
{
	[System.Serializable]
	public class AttackSkill
	{
		public int 			m_SkillID;
		public int			m_SkillInMove;
		public int			m_SkillInSprint;
		public int			m_SkillInAir;
		public int			m_SkillInWater;
	}
	
	public AttackMode[]		m_AttackMode;
	public AttackSkill[]	m_AttackSkill;
	public float[]			m_StaminaCost;

    public string[]         m_Idles;
	
	public float[] 	m_AnimDownThreshold = new float[2]{40f, 20f};
	public float			m_AnimSpeed = 1f;

	public ItemAsset.ItemObject ItemObj{ get{ return m_ItemObj; } }
	
	public void HoldWeapon(bool hold)
	{
//		if(hold)
			m_MotionEquip.ActiveWeapon(this, hold);
//		else
//			m_MotionEquip.ActiveWeapon(this, hold, true);
	}

	public bool HoldReady { get { return m_MotionMgr.GetMaskState(m_HandChangeAttr.m_HoldActionMask); } }

	public bool UnHoldReady { get { return !m_MotionMgr.IsActionRunning(m_HandChangeAttr.m_ActiveActionType); } }

    public string[] leisures
    {
        get
        {
            return m_Idles;
        }
    }

    public AttackMode[] GetAttackMode ()
	{
		return m_AttackMode;
	}
	
	public virtual bool CanAttack(int index = 0)
	{
		PEActionParamVVNN param = PEActionParamVVNN.param;
		param.vec1 = m_MotionMgr.transform.position;
		param.vec2 = m_MotionMgr.transform.forward;
		param.n1 = 0;
		param.n2 = index;
		return m_MotionMgr.CanDoAction(PEActionType.SwordAttack, param);
	}

	public virtual void Attack(int index = 0, SkillSystem.SkEntity targetEntity = null)
	{
		if(null != m_MotionEquip)
		{
			m_MotionEquip.SetTarget(targetEntity);
			m_MotionEquip.SwordAttack(m_Entity.forward, index);
		}
		if(null != m_AttackMode && m_AttackMode.Length > index)
			m_AttackMode[index].ResetCD();
	}

	public virtual bool AttackEnd(int index = 0)
	{
		if(null != m_MotionMgr)
			return !m_MotionMgr.IsActionRunning(Pathea.PEActionType.SwordAttack);
		return true;
	}

	public virtual bool IsInCD(int index = 0)
	{
		if(null != m_AttackMode && m_AttackMode.Length > index)
			return m_AttackMode[index].IsInCD();
		return false;
	}
}
