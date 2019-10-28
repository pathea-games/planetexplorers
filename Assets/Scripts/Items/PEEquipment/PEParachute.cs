using UnityEngine;
using System.Collections;

public class PEParachute : PECtrlAbleEquipment
{
	public float	m_TurnOnSpeed = -10f;
	public float	BalanceDownSpeed = -3f;
	public float 	m_HorizonalSpeed = 5f;
	Animator		m_Anim;
	const string	AttachBone = "Bow_box";

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
