using UnityEngine;
using Pathea;

public class PETwoHandWeapon : PeSword
{
	public string	m_LHandPutOnBone = "mountMain";
	public string	m_LHandPutOffBone = "mountBack";

	public GameObject	m_LHandWeapon;

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		if(null != m_LHandWeapon)
			m_View.AttachObject(m_LHandWeapon, m_LHandPutOffBone);
	}
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment();

		if(null != m_LHandWeapon)
		{
			m_View.DetachObject(m_LHandWeapon);
			GameObject.Destroy(m_LHandWeapon);
		}
	}
	
	public override bool CanAttack(int index = 0)
	{
		PEActionParamVVNN param = PEActionParamVVNN.param;
		param.vec1 = m_MotionMgr.transform.position;
		param.vec2 = m_MotionMgr.transform.forward;
		param.n1 = 0;
		param.n2 = index;
		return m_MotionMgr.CanDoAction(PEActionType.TwoHandSwordAttack, param);
	}
	
	public override void Attack(int index = 0, SkillSystem.SkEntity targetEntity = null)
	{
		if(null != m_MotionEquip)
		{
			m_MotionEquip.SetTarget(targetEntity);
			m_MotionEquip.TwoHandWeaponAttack(m_Entity.forward, index);
		}
	}
	
	public override bool AttackEnd(int index = 0)
	{
		if(null != m_MotionMgr)
			return !m_MotionMgr.IsActionRunning(Pathea.PEActionType.TwoHandSwordAttack);
		return true;
	}
}
