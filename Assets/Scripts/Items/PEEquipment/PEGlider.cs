using UnityEngine;
using System.Collections;

public class PEGlider : PECtrlAbleEquipment 
{
	const string	AttachBone = "Bow_box";
	public float	m_TurnOnSpeed = -10f;
	public float	m_RotateAcc = 5f;
	public float	m_BoostPower = 2f;
	public float	m_BalanceForwardSpeed = 20f;
	public float	m_BalanceDownSpeed = 5f;
	public float	m_AreaDragF = 0.004f;

	const float 	GravityAcc = 10f;

	Animator		m_Anim;
	
	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Anim = GetComponentInChildren<Animator>();
		m_View.AttachObject(gameObject, AttachBone);
		SetOpenState(false);
	}
	
	public void SetOpenState(bool open)
	{
		if(null != m_Anim)
			m_Anim.SetBool("Open", open);
	}
}
